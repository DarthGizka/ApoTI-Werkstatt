# Zeitbasierte Generierung von Transaktionsnummern (a.k.a. Monte-Carlo-Methode)

Die ApoTI-Spezifikation beschreibt in Abschnitt 5.2.2.2.2.3.1 eine Methode zum Umkodieren eines Zeitstempels in eine maximal 8stellige Zahl, welche nach Hinzufügen einer Prüfziffer als Transaktionsnummer verwendet werden kann. Die darstellbare Präzision liegt bei circa einer Drittelsekunde¹.

Der Aufwand zum Vergeben einer Transaktionsnummer beschränkt sich damit effektiv auf Umkodieren des Erstellungszeitpunkts des Datensatzes, ohne daß dafür eine zentrale Koordinierung der Vergabe notwendig wäre.

Der Vorteil des Verfahrens liegt darin, daß mehrere Stellen unabhängig voneinander Transaktionsnummern für ein und dieselbe Apotheke erzeugen können, z.B. die Apotheke selbst und diverse Fremdhersteller für künstliche Ernährung oder parenterale Zubereitungen. Das Risiko einer zufälligen Doppeltvergabe bleibt dabei so klein, daß es praktisch gesehen nicht mehr ins Gewicht fällt.

Innerhalb der Reichweite eines AVS kann auch bei Verwendung des zeitbasierten Verfahrens das Risiko einer zufälligen Doppelvergabe vollkommen eliminiert werden. Dazu muß nur die zuletzt vergebene Transaktionsnummer - a.k.a. 'Hochwassermarke' - in einer globalen Variablen aufbewahrt und der Zugriff auf diese Variable serialisiert werden.

 Bei Neuvergabe einer Transaktionsnummer wird diese zunächst nach dem zeitbasierten Verfahren erzeugt und dann mit der Hochwassermarke verglichen. Sollte die neue Nummer nicht größer sein als die Hochwassermarke, dann wird die Hochwassermarke nach Erhöhung um 1 Inkrement zur neuen Nummer; anderenfalls wird die neue Nummer zur neuen Hochwassermarke. Bei Vergabe von mehreren Transaktionsnummern zum gleichen Zeitpunkt wandern die später vergebenen dadurch sukzessive um jeweils eine Drittelsekunde weiter in die Zukunft.

Dieses Verfahren ist analog zum dem bei der Erzeugung von TP3-EDI-Dateien, wo die Kombination aus Absender-IK, Empfänger-IK und sekundengenauem Erstellungszeitpunkt eindeutig sein muß. Mit dem Trick der Hochwassermarke kann man trotzdem in der gleichen Sekunde beliebig viele Dateien für den gleichen Empfänger erzeugen.

---
<sup>*1) Die in der Spezifikation angegebene Formel entspricht nicht der deklarierten Intention, also Verwendung von Drittelsekunden. Die Formel `Abrunden(Millisekunden / 334)` hat zur Folge, daß 667 Millisekunden als 1/3 Sekunde kodiert werden statt als 2/3 Sekunde. Aufgrund des stochastischen Charakters des Verfahrens ist es jedoch weitgehend unschädlich, die Zeitstempel korrekt zu konvertieren.*</sup>

---
## Die Silvesterlücke (entsprechend Zeitstempeln nach dem 31. Dezember eines Jahres)

Die größte zeitbasierte Transaktionsnummer ist 964223994, was 23:59:59.6680000¹ Uhr am 31. Dezember eines Jahres entspricht. 

Die darauf folgenden 3577600 Transaktionsnummern - also 964224001 bis 999999994 - bleiben bei Verwendung zeitbasierter Transaktionsnummern normalerweise ungenutzt. 

Einige Fremdhersteller verwenden diesen Bereich zur Vergabe ihrer Transaktionsnummern, vermutlich um das Risiko einer Kollision mit von der Apotheke vergebenen Transaktionsnummern komplett auszuschließen. 

Das funktioniert perfekt, solange eine Apotheke nicht mehr als einen nach diesem Schema arbeitenden Fremdhersteller hat. Die Wahrscheinlichkeit von Kollisionen steigt jedoch drastisch an, sobald mehr als ein Fremdhersteller bei der gleichen Apotheke dieses Verfahren anwendet. 

In diesem Fall kann das Risiko je nach Anzahl der von den Fremdhersteller belieferten Apotheken um Größenordnungen über dem des zeitbasierten Verfahrens liegen. Je weniger Apotheken die Fremdhersteller beliefern, desto höher die Wahrscheinlichkeit, daß sie im Lauf des Jahres der gleichen Apotheke die gleiche Transaktionssnummer zuteilen.

---
<sup>*1) bzw. 23:59:59.6666667 bei der Verwendung von Drittelsekunden statt der Formel laut Spezifikation (siehe Fußnote 1 im vorhergehenden Abschnitt)*</sup>

---
## Koordinierte Vergabe von Transaktionsnummern

---
Stand 2023-02-20