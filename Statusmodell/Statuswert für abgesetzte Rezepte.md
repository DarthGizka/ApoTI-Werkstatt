# Statuswert für abgesetzte Rezepte 

Rein technisch/praktisch können abgesetzte (E-)Rezepte vom Rechenzentrum auf `FEHLER` gesetzt werden, mit entsprechenden Hinweisen in den Meldungstexten.

Das hat mehrere Vorteile:

1) Jedes AVS kann a priori mit `FEHLER` umgehen und der Apotheke die Begleitmeldungen anzeigen. 

2) Die Apotheke muß alle E-Rezepte mit `FEHLER` ohnehin durchsehen und abarbeiten. 

3) Jedes AVS unterstützt jetzt schon die Neueinreichung von Rezepten mit Status `FEHLER`.

Der Status `RUECKWEISUNG` ist in der Protokollspezifikation nicht klar definiert, und die Unterstützung seitens der AVS liegt daher derzeit vermutlich irgendwo zwischen rudimentär und komplett abwesend.

Trotzdem sprechen einige Dinge für eine eventuelle zukünftige Unterstützung von `RUECKWEISUNG` bzw. `ABGESETZT`:

1) Es ist a priori klar, daß es um historische Rezepte und den Verlust bereits erhaltenen Geldes geht, nicht um Rezepte aus dem laufenden Betrieb.

2) Es steht außer Zweifel, daß bei Rezepten mit diesen Statuswerten die durch den Vorgängerzustand `ABGERECHNET` induzierte Bearbeitungs-/Neueinreichungssperre im AVS aufgehoben werden kann. Bei einem Wechsel von `ABGERECHNET` auf `FEHLER` ist der Störabstand deutlich geringer (siehe Punkt 3).

3) Ein AVS ohne vollständigen historischen Datenbestand¹ kann nur aus einem dedizierten Statuswert wie `ABGESETZT` zweifelsfrei ablesen, daß die Wiederfreischaltung eines Rezeptes zur Bearbeitung/Neueinrichtung beabsichtigt ist. Anderenfalls könnte schon eine ansonsten relativ harmlose Fehlfunktion wie die Resurrektion einer alten Fehlermeldung zu einer unbeabsichtigten Neuabrechnung eines Rezepts führen.

---
*¹) z.B. nach Umstellung der Apotheke auf ein anderes AVS, i.V.m. Statusabfrage über `GEAENDERTE`*

---
<sup>*Stand 2023-02-14*</sup>
