# Abfrage von Statusänderungen: `ladeStatusRezept(GEAENDERTE)`
<sup>*Stand 2023-02-15*</sup>


Hier ist ein Vorschlag für eine einfache, effiziente und minimal-invasive Umsetzung von Abfragen für Statusänderungen. 

Die einzige dafür notwendige Änderung auf __Protokollebene__ ist das Hinzufügen eines Werts `GEAENDERTE` zu der Aufzählung `rezeptStatus` (`ALLE`, `BEANSTANDETE`, ...), welche den bei der Operation `ladeStatusRezept` anzuwendenden Statusfilter angibt. 

Die Grundidee ist simpel: vom RZ ausgelöste Statusänderungen setzen ein Flag "Rezeptstatus geändert", und mittels `ladeStatusRezept(GEAENDERTE)` kann das AVS diese Änderungen in Schüben von maximal 300 Stück abholen. 

Das ist insbesondere nach Rechnungsläufen von Vorteil, weil die Änderung einer Monatsladung Rezepte auf `ABGERECHNET` so in ca. einem Dutzend Anfragen übertragen werden kann und das AVS nicht für Tausende RZ-Lieferids Einzelanfragen senden muß. Bei abgesetzten Rezepten ist der Unterschied noch gravierender, da ein AVS zum Ermitteln abgesetzter Rezepte über Id-basierte Statusanfragen auch viele ältere Monate durchforsten müßte und so nach dem alten Schema Zigtausende Anfragen notwendig wären.

Damit kommen wir auch gleich zur Notwendigkeit einer kleinen Komplikation. Es liegt in der Natur das Frage-Antwort-Protokolls, daß das RZ vom AVS keine Bestätigung über den erfolgreichen Empfang einer Antwort erhält. Falls beim Empfang einer Antwort etwas schiefginge (Timeout, Netzwerkfehler, ...), dann könnte das AVS nur durch Id-basierte Statusanfragen für den Gesamtbestand - also Zigtausende Lieferungen - die verlorengegangenen Änderungen wiederfinden und wieder auf den gleichen Stand kommen wie das RZ.

Dieses Problem wird so gelöst, daß abgeholte Statusänderungen jeweils erst bei der *nachfolgenden* Änderungsabfrage als erledigt abgehakt werden. Abholen der Statusänderungen setzt nur ein 'abgeholt'-Flag, das dann bei der nachfolgenden Änderungsabfrage zusammen mit dem 'geändert'-Flag zurückgesetzt wird. Das gibt dem AVS die Möglichkeit, nach Fehlschlag einer Statusänderungsabfrage eine Untervariante der Änderungsabfrage ohne Rücksetzen der zuvor abgeholten Änderungen zu verwenden.

Damit benötigt jeder Rezeptdatensatz zwei Boolesche Flags:
- `Status_geaendert` - das RZ hat den Pfeifferstatus des Rezepts geändert
- `Ruecksetzen_ausstehend` - Änderung wurde abgeholt, aber noch nicht quittiert

Jede vom AVS ausgelöste Statusänderung - also Neueinreichung oder Stornierung eines Rezepts - setzt beide Flags auf `false`.

Jede vom RZ ausgelöste Statusänderung - zum Beispiel von `VOR_PRUEFUNG` in einen geprüften Zustand, von einem abrechenbaren Zustand in `ABGERECHNET`, oder von `ABGERECHNET` in `RUECKWEISUNG` - setzt das Änderungsflag auf `true` und das Rücksetzflag auf `false`.

Eine normale Änderungsabfrage setzt zunächst beide Flags auf `false` bei allen Rezepten mit dem Rücksetzflag. Dann wählt es die ersten 300 Rezepte mit Änderungsflag, setzt dort das Rücksetzflag auf `true` und sendet die Statusinformationen an das AVS.

Eine Änderungsabfrage nach Fehlschlag läßt die Änderungsflags unverändert; alles andere ist genau wie bei einer normalen Änderungsabfrage.

Parameter für eine normale Änderungsabfrage ist `perStatus` mit `rezeptStatus = GEAENDERTE`. 

Für eine Änderungsabfrage nach Fehlschlag gilt stattdessen `perLieferID` mit `rezeptStatus = GEAENDERTE` und `rzLieferId = 'WiederaufsetzenNachEmpfangsfehler'` (sorry, Pattern '\w' erlaubt keine Leerzeichen oder Unterstriche).

---
## Terminierung von Abfrageserien

Es gibt zwei Möglichkeiten zu erkennen, daß keine weiteren Statusänderungen vorliegen:

1) Wiederholen der Änderungsabfrage bis zum Erhalt einer Antwort mit weniger als 300 Werten
2) Wiederholen der Änderungsabfrage bis zum Erhalt einer leeren Antwort

Aus meiner Sicht ist die erste Variante - Antwort mit weniger als 300 Sätzen signalisiert Ende - eindeutig die bessere. Insbesondere kann das übliche Polling alle 10 Minuten fast immer mit jeweils einem einzigen Frage-Antwort-Zyklus abgehandelt werden; bei Variante 2 wären das doppelt soviel, ohne jeden Mehrwert.

Bei Variante 1 kann das AVS mit einer einzigen Statusabfrage alle 10 Minuten nicht nur den Prüfstatus der kurz zuvor eingelieferten Rezepte abholen, sondern es bekomnmt auch Wind von Abrechnungen, Absetzungen und eventuell sogar von Änderungen des Fehlerstatus von Rezepten des laufenden Monats (i.e. wenn eine Verfeinerung der RZ-Prüflogik neue Problem zu Tage gefördert hat).

---
## Interaktion zwischen Änderungsabfragen und normalen Statusabfragen

Bei normalen Statusabfragen gibt es keinerlei Änderungen der Flags. Man könnte natürlich in Versuchung kommen, die zurückgegebenen Statuswerte als bekannt anzusehen. Aber mangels Empfangsquittierung kann diese Annahme nicht gesichert sein.

---
## Zwei Boolesche Flags anstelle einer Aufzählung mit drei Werten

Das System der beiden Flags - Änderungsflag und Rücksetzflag - hat nur drei gültige Zustände, weil bei gesetztem Rücksetzflag der Wert des Änderungsflags unerheblich ist (bzw. ein anderer Wert als `true` nur auf eine Inkonsistenz in Code oder Datenbank zurückgehen kann).

Man könnte argumentieren, daß ein Aufzählungstyp mit drei möglichen Zuständen - z.B. auch in Form eines nullbaren Booleschen Flags - hier die bessere Wahl wäre.

Rein mathematisch mag das zutreffen, aber sinnvolles Benennen der drei Zustandswerte sowie des Datenbankfeldes ist ein Ding der Unmöglichkeit (ich habe es jedenfalls lange genug erfolglos versucht). Mit den beiden Booleschen Flags ist das Schema einfach zu erklären, einfach zu verstehen und einfach umzusetzen.

---
## Warum nicht `rezeptStatus = GEAENDERTE` mit `rzLieferId = 'FORTSETZEN'` als Normalfall?

In gewissem Sinne wäre es logischer gewesen, den Fortsetzungsfall (also mit vorherigem Rücksetzen der als abgeholt markierten Änderungsflags) wie o.a. zu kodieren und den Fall ohne vorheriges Rücksetzen als `rezeptStatus = GEAENDERTE` ohne Schnörkel. Damit würden die API-Anfragen präziser ausdrücken, was passieren soll.

Andererseits ist es so, daß der Spezialfall (ohne vorheriges Rücksetzen) eigentlich praktisch nie auftreten sollte, da er nur zur Reparatur nach einem relativ unerwarteten Fehler benötigt wird. 

Ergo wurde die Bürde der syntaktischen Komplikation - Hinzufügen von `rzLieferId = 'WiederaufsetzenNachEmpfangsfehler'` mit Wechsel von `perStatus` auf `perLieferID` - dem nur alle Jubeljahre auftretenden Spezialfall auferlegt. Damit wird der ununterbrochen rund um die Uhr zum Einsatz kommende Normalfall schlanker und logischer - i.e. wirklich eine Statusabfrage mit Statusfilter `GEAENDERTE` und nicht eine Lieferid-Abfrage mit Pseudo-Lieferid und seltsamer Semantik.

---
## Umsetzungsaspekte

Die notwendigen Änderungen existierender SQL-Abfragen (einschließlich DML) sind minimal und beschränken sich auf das Setzen fester Werte für die zwei Booleschen Flags. Notwendige neue Abfragen lassen sich über die zwei Booleschen Flags mit gefilterten Indexen einfach und effizient implementieren. 

Durch die gefilterten Indexe wird die Bürde des Aktualisierens der notwendigen Tabellen der SQL-Engine aufgeladen; expliziter Anwendungscode muß nur die Flags setzen und den Rest macht die Engine unterm Hut.

Bei der Variante mit gefilterten Indexen ist die I/O-Last in etwa die gleiche, wie sie eine der gefundenen Rezeptmenge entsprechenden Anzahl Einzelanfragen über Lieferid produziert. Also definitiv im grünen Bereich, so daß einer Verwendung der Änderungsabfragen auch für die zehnminütlichen Routineabfragen nichts im Wege steht. 

Bei MSSQL lohnt es sich, für die Massentransfers mit 300 Rezepten je Abfrage - i.e. nach der Abrechnung - einen separaten Codepfad vorzusehen, damit das Teil hier nicht auch mit Nested Loops arbeitet wie im wenige-Rezepte-Fall. In meiner Probeimplementierung mußte ich allerdings MSSQL fest bei der Hand nehmen und jeden Schritt explizit vorgeben, um von Tausenden I/Os mit Nested Loops runterzukommen auf ein paar Dutzend. Das Übliche halt. Postgres kann sowas ganz alleine, weil es mangels geclusterter Tabellen egal kaum andere Strategien fahren kann. ;-)

Bei expliziter Modellierung der den Indexen entsprechenden Tabellen (i.e. Verwenden expliziter Tabellen anstelle von gefilterten Indexen) in Verbindung mit geschickter Definition der geclusterten Indexe läßt sich die I/O-Last an einigen Stellen noch einmal deutlich senken, aber dann werden die notwendigen Anpassungen beim existierenden SQL um Größenordnungen komplizierter als das Setzen zweier Flags (sofern man die Komplikationen nicht in Triggern versteckt).

Die Effizienzsteigerung beruht darauf, daß man bei einer selbstgebauten Tabelle Felder aus beliebigen Quelltabellen hernehmen und auch zum Clustern verwenden kann, während gefilterte Indexe (== vom Server implizit gepflegte geclusterte Tabellen) nur auf die Felder einer einzigen Tabelle zugreifen können. Außerdem liegen die Sätze in der dedizierten Tabelle dicht an dicht, während sie in der Rezepttabelle weit verstreut sein können. 

So reduziert sich das Löschen von 300 Rücksetzen-Flags aus einer dedizierten Tabelle auf einen einzigen Seek plus Wegsemmeln von 1 oder 2 hintereinander liegenden Blöcken (5 .. 10 I/Os), während die gleiche Aktion in der Rezepttabelle über Nested Loops auf (3 .. 4) * 300 I/Os für den Zugriff und bis zu 300 wegzuschreibende Blöcke hinausläuft.

