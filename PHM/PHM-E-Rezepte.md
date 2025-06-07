# E-Rezepte & Pflegehilfsmittel zum Verbrauch
<sup>*Stand 2025-06-07*</sup>

* [PHM-E-Rezept: simples Datentransportgehäuse oder elektronisch signierter Nachweis?](#phm-e-rezept-simples-datentransportgehäuse-oder-elektronisch-signierter-nachweis)
* [Beihilfe vs. Beihilfe](#beihilfe-vs-beihilfe)
* [Übermittlung des Zuzahlungsstatus](#übermittlung-des-zuzahlungsstatus)
* [Vertragskonforme Preisermittlung und Datenübermittlung nach § 302](#vertragskonforme-preisermittlung-und-datenübermittlung-nach--302-sgb-v)
* [Abbilden der Mehrkosten als 'Abschlagsposition' mit negativem Betrag (54.00.99.0088)](#abbilden-der-mehrkosten-als-abschlagsposition-mit-negativem-betrag-5400990088)
---

### PHM-E-Rezept: simples Datentransportgehäuse oder elektronisch signierter Nachweis?

***Reines Datentransportgehäuse***


Derzeit wird das PHM-E-Rezept ausschließlich für den Datentransport von der Apotheke zum Rechenzentrum verwendet. Die Kassen können lediglich die PDF-Form der Empfangsbestätigung als Nachweis verlangen, nicht aber das E-Rezept¹. 

Solange das so bleibt, kann man das PHM-E-Rezept als ein reines Datentransportgehäuse betrachten, also als eine rein interne Angelegenheit zwischen Apotheken und ihren Rechenzentren. 

Eine zweckentfremdete Nutzung von Datenfeldern ist in diesem Fall zum einen relativ unproblematisch und zum anderen auch gar nicht vermeidbar, da die bestehenden Strukturen ohne Profilanpassungen genutzt werden sollen.

In Ermangelung von *belegbezogenen* Feldern für den Patientenanteil im Beihilfefall oder für die Mehrkosten über dem Festbetrag könnte man somit diese Beträge freizügig auf die vorhandenen *positionsbezogenen* Felder verteilen, ohne daß die einzelnen Abrechnungszeilen dabei rechnerisch korrekt/konsistent sein müßten. 

So wäre es zum Beipiel auch möglich, die Gesamtwerte für Mehrkosten und/oder Beihilfe-Patientenanteil an eine beliebige Positionszeile anzuhängen, selbst wenn diese Gesamtwerte den Bruttowert dieser Position übersteigen. In extremis könnte man sogar die Mehrkosten und den Beihilfe-Patientenanteil als Text in irgendwelche Freitextfelder stopfen; das hätte zwar nicht übermäßig viel Sinn, aber es wäre zulässig.

Die sinnvollste Übergangslösung wäre sicher eine Festlegung, bei Pflegehilfsmitteln zum Verbrauch die vorhandenen positionsgebundenen Felder für Mehrkosten und Beihilfe-Patientenanteil (a.k.a. 'Eigenbeteiligung § 27a') grundsätzlich als belegbezogen zu betrachten, ohne wertmäßigen Bezug zur enthaltenden Position. 

Die Belegwerte für Mehrkosten und Patientenanteil ergeben sich dann ganz normal durch Summieren der in den Positionen vorgefundenen Werte, so daß sowohl die Strategie 'alles an die erste Position dranhängen' als auch das sinnvolle Verteilen auf die einzelnen Positionen gleichermaßen zum Erfolg führen. In jedem Fall sollte aber ausdrücklich darauf hingewiesen werden, daß auf generierender Seite die Berechnung des Beihilfebetrages vertragskonform anhand der Summenwerte zu erfolgen hat, ungeachtet der Übermittlung in Abrechnungszeilen.

***Abrechnungsnachweis***

Die Situation ist komplett anders bei einer Datenlieferung als Abrechnungsnachweis, analog zur Rolle des E-Rezept-Abgabedatensatzes bei der Rezeptabrechnung oder des ABRP-Datenformats bei der klassischen Datenlieferung.

In diesem Fall müssen alle positionsweise *ausgewiesenen* Werte auch zwingend positionsweise *ermittelt* werden, und bei der Abrechnung werden die Positionswerte dann nur noch aufsummiert.

So wird zum Beispiel auch die Eigenbeteiligung des Patienten bei der Abrechnung nach § 27a positionsweise ermittelt und ausgewiesen, was das direkte Äquivalent zum Beihilfefall bei PHM ist. Die Mehrkosten über dem Festbetrag werden zwar auch positionsweise ermittelt und ausgewiesen, aber aufgrund der anders strukturierten Berechnungsgrundlage ist ein Analogieschluß für die PHM-Abrechnung weniger aussagekräftig.

Für das PHM-E-Rezept bedeutet das entweder die zeilenweise Berechnung und Ausweisung (mit Inkaufnahme von bis zu 12 Rundungs-Cents zu Lasten von Beihilfe-Patienten) **oder** die profiltechnische Ergänzung der Felder für belegbezogene Gesamtwerte von Mehrkosten und Beihilfe-Patientenanteil.

***Summa summarum***

Vor einer möglichen Umwidmung des PHM-E-Rezeptes in einen (kassensichtbaren) elektronischen Abrechnungsnachweis sollte es eigentlich ausreichend Vorlaufzeit geben, um die notwendigen Anpassungen der Profile und der technischen Spezifikation (Anhang 5 von TA1) vorzunehmen. 

Bis dahin könnte das PHM-E-Rezept als reines Datentransportgehäuse betrachtet werden, ohne daß wegen seiner Struktur von einer fallbezogenen Berechnung des Beihilfe-Patientenanteils auf eine positionsbezogene Berechnung umgestellt werden müßte.

Für eine korrekte Abbildung der Mehrkosten über dem Festbetrag sind dagegen keine Strukturänderungen notwendig, da für die Berechnung der Mehrkosten keine Multiplikation mit einem Faktor wie 50% und anschließender kaufmännischer Rundung notwendig ist. Eine positionsweise Anwendung der Rechenregeln mit anschließender Summierung ergibt hier genau den gleichen Betrag wie eine fallbezogene Berechnung auf Basis des Gesamtbruttos.

<sup>*1) sofern es überhaupt ein PHM-E-Rezept gibt und nicht die bis einschließlich Oktober 2025 noch zulässige Papierform zur Übermittlung genutzt wurde*</sup>

---

### Beihilfe vs. Beihilfe

Nach dem Konzept des Beihilfefalles im Sinne von § 4 Absatz 4 des seit 01.06.2025 gültigen Vertrages zahlt die Kasse 50% des erstattungsfähigen Betrages, also derzeit maximal 21,- € wg. der Deckelung des erstattungsfähigen Betrages auf 42,- €. Das ist eine direkte Umsetzung von [§ 28 (2) SGB XI][SGB11_28_2].

Praktisch gesehen können einige Formen der Beihilfe aber auch eine Vollerstattung bis zur Grenze von 42,- € vorsehen, wie z.B. [im Fall des Bundesversorgungsamtes][BVA].

[SGB11_28_2]: https://www.gesetze-im-internet.de/sgb_11/__28.html

[BVA]: https://www.bva.bund.de/DE/Services/Bundesbedienstete/Gesundheit-Vorsorge/Beihilfe/4_Beihilfeanspruch/41_Beihilfeberechtigte/5_Pflegebeduerftige/56_Leistungen_Pflegebeduerftige/566_Pflegehilfsmittel/566_pflegehilfsmittel.html

---

### Übermittlung des Zuzahlungsstatus

Die [Spezfikation für PHM-E-Rezepte (Stand 23.04.2025)][EPHM] schreibt vor, daß der Zuzahlungsstatus des Patienten in der Verordnung als 'befreit' anzugeben ist. Eine Angabe des Zuzahlungsstatus als 'pflichtig' in der Verordnung würde somit eine Abweichung vom Buchstaben der Spezifikation darstellen.

Man kann natürlich den Zuzahlungsstatus in der Verordnung einfach trotzdem wahrheitsgemäß setzen und darauf vertrauen, daß entweder die Spezifikation hinreichend schnell korrigiert wird, oder daß der Empfänger des PHM-E-Rezepts nicht auf Einhaltung dieses - vermutlich versehentlich aus dem PharmDL-/Impfbereich übernommenen - Details der Spezifikation pocht.

Wem das zu heikel ist, der kann im Fall der Zuzahlungspflichtigkeit diese über ein Zusatzattribut im Abgabedatensatz angeben (Gruppe 15). Die Spezifikation schreibt die Verwendung dieser Gruppe zwar nicht vor, ab sie schließt sie auch nicht aus.

[EPHM]: https://www.abda.de/fileadmin/user_upload/assets/Formulare/250423_Schiedsspruch_PflegeHiMi_elektr_Datenlieferung_TA.pdf

---

### Vertragskonforme Preisermittlung und Datenübermittlung nach § 302 SGB V

Laut Vertrag erfolgt die Preisermittlung durch Multiplizieren eines Nettopreises mit der Stückzahl bzw. der Anzahl der 100-ml-Einheiten, gefolgt vom Aufschlagen der Umsatzsteuer. Die Nettopreise sind dabei mit 2 Nachkommastellen angegben, also auf ganze Cent.

NB: in der [FAQ-Liste des GKV-Spitzenverbandes][FAQ] gibt es an mehreren Stellen eine gleichlautende Festlegung. Bei der Frage 'Wie ist mit Rundungsdifferenzen umzugehen?' gibt es zwar scheinbar eine anderslautende Angabe (
'Bruttobetrag = (Einzelbetrag mit zwei Nachkommastellen + (ggfs.) MwSt.-Betrag) * Menge'), aber das ist möglicherweise nur eine unglücklich formulierte Empfehlung, durch Rechnen mit erhöhter Genauigkeit ein sachlich falsches Berechnungsmodell eventuell noch zu retten.

Die Preisberechnunng laut Vertrag entspricht auch exakt dem Modell, daß der Datenlieferung nach § 302 für den Fall von Preisvereinbarungen mit Nettopreisen zugrundeliegt. Damit können die PHM-Daten verlustfrei und korrekt in diesem Format geliefert werden, modulo kleinerer Fehlimpedanzen bei Mehrkosten und Beihilfe-Patientenanteil.

Das Datenformat gemäß § 105 SGB XI ist dagegen in seiner jetzigen Form für die PHM-Abrechnung nicht wirklich geeignet. Zum einen wird dort die Preisberechnung als 'Anzahl x Brutto-Einzelpreis' vorgeschrieben, und zum anderen können dort die Mehrkosten nur durch Zusammenwerfen mit der Zuzahlung abgebildet werden. 

[FAQ]: https://www.gkv-spitzenverband.de/media/dokumente/pflegeversicherung/phm_vertraege/2024_12_16_PHM_FAQ_Vertraege.pdf

---

### Abbilden der Mehrkosten als 'Abschlagsposition' mit negativem Betrag (54.00.99.0088)

Gemäß § 5 Absatz 5 des Vertrages kann zum Abbilden des Mehrkostenbetrages 'im Einvernehmen mit der Pflegekasse die Abschlagspositionsnummer 54.00.99.0088 verwendet werden'.

In Bezug auf die elektronische Datenlieferung ist eine solche Abbildung in mehrerlei Hinsicht problematisch. 

Zum einen enthalten ausnahmslos alle relevanten Datenformate schon strukturelle Felder zum Ausweisen der Mehrkosten. Lediglich beim Datenformat nach § 105 SGB XI hakt es etwas, weil das dort vorhandene Feld für die Aufnahme einer Zuzahlung *oder* eines Mehrkostenbetrages gedacht ist, während bei Vorliegen eines Zuzahlung *und* eines Mehrkostenbetrages dann die beiden Beträge zwangsweise vermischt werden und nicht mehr einzeln erkennbar sind. Für eine Verwendung der Abschlagsposition 54.00.99.0088 müssen die strukturellen Mehrkostenfelder unterdrückt werden, weil sonst am Ende vollkommen falsche Werte herauskommen.

Zum zweiten muß die Abschlagsposition 54.00.99.0088 bei der Verarbeitung in vielen Stellen als Sonderfall erkannt und behandelt werden, um einerseits Erscheinungen wie negative Zuzahlungsanteil usw. zu vermeiden und andererseits die Mehrkosten jeweils an den richtigen Stellen in die Berechnungsvorgänge einzuspeisen.