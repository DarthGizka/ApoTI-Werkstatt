# Transienz von `FEHLER` und `RUECKWEISUNG`

Bei Papierrezepten sind die ApoTI-Statuswerte der Rezepte rein informativ und nicht relevant für die Abrechnung. Insbesondere P-Rezepte wurden historisch oft nur im RZ abgeladen, ohne jemals den Status abzufragen.

Bei Problemen werden vom RZ die Papierrezepte gelöscht und an die Apotheke zurückgegeben, ohne daß ApoTI irgendwie involviert wäre. P-Rezepte bekommen bei einer eventuellen Reparatur normalerweise eine neue Transaktionsnummer und damit aus ApoTI-Sicht eine neue Identität. Daher gibt es bei P-Rezepten und Muster16 schon immer größere Mengen von Datensätzen mit Status `FEHLER`, die im ApoTI-System aufliegen.

Bei E-Rezepten ist das grundlegend anders. Hier ist es unumgänglich, daß Apotheken alle auf `FEHLER` oder `RUECKWEISUNG` stehenden Rezepte durchsehen und bearbeiten; anderenfalls droht Geldverlust.

Die Rechenzentren können die Apotheken diesbezüglich nur dann effektiv unterstützen, wenn Apotheken auf `FEHLER` oder `RUECKWEISUNG` stehende Rezepte entweder neu einreichen (nach Heilung) oder stornieren. Damit sollten bei E- und P-Rezepten diese beiden Statuswerte idealerweise ausschließlich übergangsweise auftreten, also während die Sache in der Apotheke noch in Klärung ist.

---
<sup>*Stand 2023-02-14*</sup>