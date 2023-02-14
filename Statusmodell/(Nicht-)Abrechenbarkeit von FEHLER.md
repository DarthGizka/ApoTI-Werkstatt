# (Nicht-)Abrechenbarkeit von `FEHLER`

Historisch hatten die verschiedenen Statuswerte nur bei Muster16 Relevanz für die Apotheke, und bei Muster16 gibt es in ApoTI - unabhängig von der Einreichung über `pruefeRezept` oder `sendeRezepte` - ausschließlich die Vorprüfung. Abgerechnet werden ausschließlich die Papierrezepte, vollkommen unabhängig vom Schicksal eventuell im ApoTI-System vorliegender Muster16-Datensätze.

Aus diesem Grund war es bisher nicht nur unschädlich, sondern in der Tat sogar eher sachdienlich, z.B. Rezepte mit Rabattvertragsverstößen auf `FEHLER` zu setzen statt auf `VERBESSERBAR`. `FEHLER` wurde also de facto als eine weitere Eskalationsstufe behandelt - wie `VERBESSERBAR`, aber schlimmer.

Bei E-Rezepten - und eigentlich schon immer bei P-Rezepten - ist diese laxe Handhabung nicht akzeptabel bzw. führt dazu, daß offenbar einige RZ auch Rezepte mit Status `FEHLER` als abrechenbar ansehen.

Ein Rezept mit Status `FEHLER` ist nicht abrechenbar und wird nicht abgerechnet. Punkt. Grund für die Vergabe dieses Status durch das RZ ist in der Regel ein Fehler, der entweder zu einer Komplettabweisung der Datenlieferung führen würde oder sonstwie die Abrechnung des Rezeptes kategorisch verhindert. Hier nimmt das RZ dem Apotheker jede Entscheidungsmöglichkeit, idealerweise nur mit gutem Grund. ;-)

Ein Rezept mit Rabattvertragsverstoß wird zwar aller Voraussicht nach zur Vollabsetzung der betreffenden Position führen und ist somit aus der Sicht der Apotheke mit einem Defekt behaftet. Aus der Sicht von ApoTI ist es aber trotzdem abrechenbar. 

Damit ist `VERBESSERBAR` derzeit der 'schlimmste' Statuswert, der bei Problemen wie Rabattvertragsverstößen gesetzt werden kann. Wenn mehr Drama als wünschenswert oder notwendig angesehen wird, dann muß halt ein zusätzlicher Status `PROBLEMATISCH` o.ä. eingeführt werden. `FEHLER` bedeutet im Protokoll schon immer 'nicht abrechenbar', zumindest bei E- und P-Rezepten.

Im Interesse von Einheitlichkeit und Klarheit sollte zukünftig auch bei Muster16 auf eine korrekte Verwendung des Statuswerts `FEHLER` geachtet werden, so daß inhaltsgleiche Muster16 und E-Rezepte auch die gleichen Statuswerte als Prüfergebnis bekommen.

---
<sup>*Stand 2023-02-14*</sup>