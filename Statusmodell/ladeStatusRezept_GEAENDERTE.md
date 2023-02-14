# Abfrage von Statusänderungen: `ladeStatusRezept(GEAENDERTE)`

Hier ist ein Vorschlag für eine einfache, effiziente und minimal-invasive Umsetzung von Abfragen für Statusänderungen. 

Die einzige dafür notwendige Änderung auf __Protokollebene__ ist das Hinzufügen eines Enum-Werts `GEAENDERTE` zu der Aufzählung `rezeptStatus` (`ALLE`, `BEANSTANDETE`, `FEHLER` ...), welche die bei der Operation `ladeStatusRezept` anzuwendenden Statusfilter angibt. 

Das vorgeschlagene Schema basiert darauf, in der Datenbank bei jedem Rezept den Statuswert-Replikationszustand wie folgt zu kennzeichnen:

- `BEKANNT` - Nullzustand; die Apotheke kennt den aktuellen Statuswert des Rezepts
- `GEAENDERT` - der Statuswert hat sich geändert und muß propagiert werden
- `ABGEHOLT` - der neue Statuswert wurde abgeholt, aber der Erfolg ist noch unklar

Eine Variable mit drei statt zwei Zuständen ist notwendig, weil die Apotheke aufgrund der Frage-Antwort-Natur des APIs den tatsächlichen Empfang der abgeholten Statusänderungen nicht direkt quittieren kann. Aus diesem Grund kippt ein Rezept nach Abholung seines geänderten Statuswerts erst mal in den Zustand `ABGEHOLT`. Mit einer speziell präparierten Protokollanfrage können diese Rezepte wieder in den Zustand `GEAENDERT` zurückgekippt werden, wenn z.B. beim Lesen des Anfrageergebnisses ein Netzwerkfehler aufgetreten ist. Ansonsten wird bei der nächsten Änderungsanfrage der Replikationszustand dieser Rezepte wieder auf den Nullwert `BEKANNT` zurückgesetzt und die nächsten 300 Kandidaten mit `GEAENDERT` kommen an die Reihe.

Jede RZ-seitig angestoßenen Änderung des Rezeptstatus - also zum Beispiel von `VOR_PRUEFUNG` auf einen geprüften Zustand oder von einem abrechenbaren Zustand auf `ABGERECHNET` - setzt den Statuswert-Replikationszustand auf 'GEAENDERT'. Punto e basta.

resultiert in der folgenden Wertzuweisung:

```
Rezeptstatus_geaendert := true;
Rezeptstatus_abgeholt := false;
```

Die Abfrage von Statusänderungen läuft wie folgt ab:

1) D
werden die zuletzt abgeholten Änderungen


Ebenfalls zur Protokollebene gehört die Vereinbarung einer magischen Pseudo-P-RezeptId wie 9999:000000013 zum Rücksetzen des Serverzustands nach einer fehlgeschlagenen Änderungsabfrage, aber das involviert keine Änderungen auf XSD-Ebene.

