# Abfrage von Statusänderungen: `ladeStatusRezept(GEAENDERTE)`
<sup>*Stand 2023-03-23*</sup>


Hier ist ein Vorschlag für eine einfache, effiziente und minimal-invasive Umsetzung von Abfragen für Statusänderungen. 

Die einzige dafür notwendige Änderung auf __Protokollebene__ ist das Hinzufügen eines Werts `GEAENDERTE` zu der Aufzählung `rezeptStatus` (`ALLE`, `BEANSTANDETE`, ...), welche den bei der Operation `ladeStatusRezept` anzuwendenden Statusfilter angibt. 

__Bestehende Implementierungen__ auf Serverseite werden - abgesehen vom notwendigen Dispatch innerhalb von `ladeStatusRezept` - nur dahingehend tangiert, daß jede vom RZ angestoßene statusändernde Operation zusätzlich zum Status noch ein Boolesches Flag setzen muß.

Die Grundidee ist simpel: vom RZ ausgelöste Statusänderungen setzen ein Flag,  und mittels `ladeStatusRezept(GEAENDERTE)` kann das AVS diese Änderungen in Schüben von maximal 300 Stück abholen. 

Das ist insbesondere nach Rechnungsläufen von Vorteil, weil die Änderung einer Monatsladung Rezepte auf `ABGERECHNET` so in ca. einem Dutzend Anfragen übertragen werden kann und das AVS nicht für Tausende RZ-Lieferids Einzelanfragen senden muß. Bei abgesetzten Rezepten ist der Unterschied noch gravierender, da ein AVS zum Ermitteln abgesetzter Rezepte über Id-basierte Statusanfragen auch viele ältere Monate durchforsten müßte und so nach dem alten Schema Zigtausende Anfragen notwendig wären.

Damit kommen wir auch gleich zur Notwendigkeit einer kleinen Komplikation. Es liegt in der Natur das Frage-Antwort-Protokolls, daß das RZ vom AVS keine Bestätigung über den erfolgreichen Empfang der Antwort für eine Protokollanfrage erhält. Falls beim Empfang einer Antwort etwas schiefginge (Timeout, Netzwerkfehler, ...), dann könnte das AVS nur durch Id-basierte Statusanfragen für den Gesamtbestand - also Zigtausende Lieferungen - die verlorengegangenen Änderungen wiederfinden und wieder auf den gleichen Stand kommen wie das RZ.

Dieses Problem wird so gelöst, daß abgeholte Statusänderungen jeweils erst bei der *nachfolgenden* Änderungsabfrage als erledigt abgehakt werden. Abholen von Statusänderungen ändert nur das Flag in den betreffenden Datensätzen um anzuzeigen, daß das Flag bei der nachfolgenden Änderungsabfrage zurückgesetzt werden kann. Das gibt dem AVS die Möglichkeit, nach Fehlschlag einer Statusänderungsabfrage diese ohne Informationsverlust zu wiederholen.

Das Frage-Antwort-Protokoll erlaubt es nicht, den Erfolg oder Fehlschlag des Empfangs einer Antwort innerhalb der gleichen HTTP- bzw. SOAP-Transaktion zu signalisieren. Ergo wird diese Signalisierung effektiv in die nachfolgende Änderungsabfrage verlagert. Dafür gibt es zwei Möglichkeiten.

__Variante 1__: es muß explizit angezeigt werden, daß die Serie fortgesetzt werden soll (`rzLieferId = 'StatusabfrageserieFortsetzen'`)

__Variante 2__: es muß explizit angezeigt werden, daß die vorhergehende Antwort nicht empfangen wurde und die Serie demzufolge __nicht__ fortgesetzt werden soll (`rzLieferId = 'WiederaufsetzenNachEmpfangsfehler'`)

Variante 1 läßt den Normalfall komisch aussehen (`perLieferID` mit seltsamer Id und `rezeptStatus = GEAENDERTE`), während Variante 2 nur den in der Praxis eigentlich nie auftretenden Sonderfall komisch aussehen läßt, während der Normalfall ganz plausibel daherkommt (`perStatus` mit `rezeptStatus = GEAENDERTE`).

## DETAILS

Jeder Rezeptdatensatz benötigt ein nullbares Boolesches Flag `Status_abgeholt` mit folgender Interpretation:
- `NULL` - alles normal; move on, nothing to see here
- `false` - RZ hat den Status geändert, aber das AVS hat ihn noch nicht abgeholt
- `true` - das AVS hat die Statusänderung abgeholt, aber noch nicht bestätigt

Jede vom __AVS__ ausgelöste Statusänderung - also Neueinreichung oder Stornierung eines Rezepts - setzt das Flag auf __`NULL`__.

Jede vom __RZ__ ausgelöste Statusänderung - zum Beispiel von `VOR_PRUEFUNG` in einen geprüften Zustand, von einem abrechenbaren Zustand in `ABGERECHNET`, oder von `ABGERECHNET` in `RUECKWEISUNG` - setzt das Flag auf __`false`__.

Eine normale Änderungsabfrage nullt zunächst das Flag überall dort, wo es den Wert `true` hat. Dann wählt es die ersten 300 Rezepte mit Wert `false`, setzt dort den Wert auf __`true`__ und sendet die Statusinformationen an das AVS.

Bei Änderungsabfrage nach Fehlschlag fällt der erste Schritt (Nullen von `true`) weg; alles andere ist genau wie bei einer normalen Änderungsabfrage.

Parameter für eine normale Änderungsabfrage ist `perStatus` mit `rezeptStatus = GEAENDERTE`. 

Für eine Änderungsabfrage nach Fehlschlag gilt stattdessen `perLieferID` mit `rezeptStatus = GEAENDERTE` und `rzLieferId = 'WiederaufsetzenNachEmpfangsfehler'` (sorry, Pattern '\w' erlaubt keine Leerzeichen oder Unterstriche).

Rezepte aus der asynchronen Vorprüfung via `pruefeRezept` bleiben grundsätzlich außen vor. Zum einen gibt es bei diesen Rezepten keine Abrechnung und somit auch keine gesteigerte Notwendigkeit zum Verfolgen von Statusänderungen. Zum anderen sind Rezepte aus Vorprüf- und Abrechnungsbestand in `rzeLeistungStatus` nur anhand der Liefer-Id unterscheidbar, so daß Einbeziehen des Vorprüfbestandes den AVS erhöhten Verwaltungsaufwand aufbürden würde.

---
## Terminierung von Abfrageserien

Es gibt zwei Möglichkeiten zu erkennen, daß keine weiteren Statusänderungen vorliegen:

1) Wiederholen der Änderungsabfrage bis zum Erhalt einer Antwort mit weniger als 300 Werten
2) Wiederholen der Änderungsabfrage bis zum Erhalt einer leeren Antwort

Aus meiner Sicht ist die erste Variante - Antwort mit weniger als 300 Sätzen signalisiert Ende - eindeutig die bessere. Insbesondere kann das übliche Polling alle 10 Minuten fast immer mit jeweils einem einzigen Frage-Antwort-Zyklus abgehandelt werden; bei Variante 2 wären das doppelt soviel, ohne jeden Mehrwert.

Bei Variante 1 kann das AVS mit einer einzigen Statusabfrage alle 10 Minuten nicht nur den Prüfstatus der kurz zuvor eingelieferten Rezepte abholen, sondern es bekommt auch Wind von Abrechnungen, Absetzungen und eventuell sogar von Änderungen des Fehlerstatus von Rezepten des laufenden Monats (i.e. wenn eine Verfeinerung der RZ-Prüflogik neue Probleme zu Tage gefördert hat).

---
## Interaktion zwischen Änderungsabfragen und normalen Statusabfragen

Bei normalen Statusabfragen gibt es keinerlei Änderungen des Flag, das von den Statusänderungsabfragen verwendet wird. 

Man könnte natürlich in Versuchung kommen, die zurückgegebenen Statuswerte als bekannt anzusehen. Aber mangels Empfangsquittierung kann diese Annahme nicht gesichert sein. Eine Signalisierung mit der jeweils nachfolgenden Abfrage ist im Gegensatz zur Statusänderungsabfrage nicht möglich, so daß ein explizites NAK-Signal definiert werden müßte.

Einbeziehen der normalen Statusabfragen in die Änderungsabfragelogik hätte auch den Nachteil, daß diese Abfragen ihre Idempotenz verlören und auch diese Abfragen globalen Zustand mutieren würden. Damit müßte das AVS auch diese Abfragen global koordinieren und unabhängige Abfragen seitens AVS-Support und/oder RZ wären nicht mehr möglich.

---
## Umsetzungsaspekte

Die notwendigen Änderungen existierender SQL-Abfragen (einschließlich DML) sind minimal und beschränken sich auf das Setzen fester Werte für ein Boolesches Flag. Notwendige neue Abfragen lassen sich über das Boolesche Flag mit gefilterten Indexen einfach und effizient implementieren. 

Durch die gefilterten Indexe wird die Bürde des Aktualisierens der notwendigen Tabellen der SQL-Engine aufgeladen; expliziter Anwendungscode muß nur die Flags setzen und den Rest macht die Engine unterm Hut.

Bei der Variante mit gefilterten Indexen ist die I/O-Last in etwa die gleiche, wie sie eine der gefundenen Rezeptmenge entsprechenden Anzahl Einzelanfragen über Lieferid produziert. Also definitiv im grünen Bereich, so daß einer Verwendung der Änderungsabfragen auch für die zehnminütlichen Routineabfragen nichts im Wege steht. 


