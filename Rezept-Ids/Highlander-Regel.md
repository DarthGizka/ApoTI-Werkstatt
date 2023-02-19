# Die Highlander-Regel

Seit mindestens Version 1.06 des Protokolls beschreiben Abschnitte 3.7 und 3.8 der Spezifikation ausführlich, daß jedes E-Rezept im RZ nur genau einmal vorliegt und daß jede neu gelieferte Inkarnationen ein eventuell bereits vorliegendes Rezept mit gleicher E-Rezept-Id ersetzt. Außerdem wird ausdrücklich klargestellt, daß sich Statusrückmeldungen immer auf die aktuelle Inkarnation eines E-Rezeptes beziehen, auch wenn bei der Statusabfrage eine ältere Liefer-Id verwendet wird.

Erläuterungskästen in Abschnitt 3.7 weiten diese Regelung effektiv auf P-Rezepte sowie auf Rezepte in der Vorprüfung¹ aus. Für die Vorprüfung wird das Verfahren zwar etwas anders beschrieben², aber die resultierende Semantik ist exakt die gleiche. 

Über `sendeRezepte` eingereichte Muster16 sind diesbezüglich nicht direkt erwähnt, fallen aber unter den in 3.7 verwendet Begriff 'Verfahren zur Vorprüfung' (zumindest im Sinne des Anwendungsfalls).

Damit gilt effektiv generell, daß für jede Rezept-Id immer nur die neueste Inkarnation vorgehalten bzw. berücksichtigt werden soll. 

Vorprüfbestand (und `pruefeRezept`) und Abrechnungsbestand (aus `sendeRezepte`) sind dabei jeweils separat zu betrachten. Die Unterscheidung basiert dabei ausschließlich auf der Methode der Einreichung; über `sendeRezepte` eingereichte Muster16 gehören diesbezüglich zum Abrechnungsbestand, obwohl der Anwendungsfall bei dieser Rezeptart unabhängig vom Weg der Einreichung immer die Vorprüfung ist.

Bei Abfragen über Liefer-Id determiniert die Herkunft der Liefer-Id (`pruefeRezept` oder `sendeRezepte`) den zu jeweils verwendenden Bestand. Bei allen anderen Operationen ist der zu verwendende Bestand durch die Art der Anfrageparameter a priori determiniert.

---

<sup>*1) Die Erläuterung zur Vorprüfung findet sich letztmalig in der Version 1.08; in 1.10 ist sie offensichtlich verschütt gegangen. Vermutlich hielt man sie für überflüssig, weil Rezepte seit 1.10 vor der Neueinreichung generell nicht mehr storniert werden müssen und der entsprechende Passus in der Erläuterung der Vorprüfung damit hinfällig ist.*</sup>

<sup>*2) O-Ton: 'Der alte Datensatz wird in diesem Falle immer komplett verworfen. Auch die Statusrückmeldungen beziehen sich immer auf den aktuellsten Datensatz, welcher im RZ vorliegt.'*</sup>

---
# Trennung von Vorprüf- und Abrechnungsbestand

Rezepte im Abrechnungsbetrieb - also die über `sendeRezepte` eingereichten - sind vollkommen separat von denen aus der Vorprüfung, welche per `pruefeRezept` einzeln gesendet werden. 

Die Ergebnisse der asynchronen Vorprüfung sind generell über die `perLieferID`-Variante der Statusabfrage zugänglich, und im Fall von E-Rezepten zusätzlich auch `perRezeptID` (via `eRezeptIdPruef`). Alle anderen Formen der Statusabfrage beziehen sich implizit ausschließlich auf den Abrechnungsbestand.

Damit muß für den Zugriffspfad über Rezept-Id nur im Fall von E-Rezepten zwischen Vorprüf- und Abrechnungsbestand unterschieden werden, nicht bei Muster16¹. 

Für P-Rezepte ist die Vorprüfung über `pruefeRezept`seltsamerweise nicht vorgesehen, obwohl gerade dort aufgrund der TA1-induzierten hohen Fehlerrate schon immer ein erhöhter Bedarf² für die (Synchron-)Vorprüfung besteht.

---

<sup>*1) Asynchrone Muster16-Vorprüfung über `pruefeRezept` hat vermutlich außerhalb von Akzeptanz-Testsuites keinerlei praktische Bedeutung, weil dafür der bequemere Weg über `sendeRezepte` zur Verfügung steht. Mit letzterem kann man alle aufgelaufenen Rezepte in einem Schwung einreichen, auch gemischt mit E- und P-Rezepten. Bei E-Rezepten bietet asynchrone Vorprüfung im Gegensatz zu `sendeRezepte` absolute Gewißheit, daß das so geprüfte E-Rezept nicht in die Abrechnung gehen kann. Bei `sendeRezepte` müßte man dafür die Quittung weglassen, ab dann würde die Quittung nicht geprüft.*</sup>

<sup>*2) insbesondere im Gefolge größerer Änderungen der TA1 (2021 hatten sogar die besten AVS ein paar Anfangsschwierigkeiten); auch jetzt gibt es immer noch regelmäßig Verstöße gegen TA1, da offensichtlich einige AVS ihre Apotheken nicht durch Verhinderung von Fehleingaben unterstützen*</sup>

---
Stand 2023-02-19