# Besonderheiten der Statusaktualisierung bei E-Rezepten

Bisher wurde die ApoTI-Schnittstelle vorwiegend für die Muster16-Vorprüfung und für das Einreichen von elektronischen Zusatzdaten für Papierrezepte verwendet. 

Aus diesem Grund war es für die AVS bis jetzt ausreichend¹, genau eine RZ-induzierte Statusänderung wenige Minuten nach Einreichung eines Muster16- oder P-Rezepte zu ermitteln, nämlich die von `VOR_PRUEFUNG` auf einen geprüften Zustand wie `ABRECHENBAR` oder `HINWEIS`. Die übliche Statusabfrage `perLieferID` ca. 10 Minuten nach Einreichung war dafür ausreichend. Das AVS mußte nur ggf. diese Abfrage solange wiederholen, bis ein Status ungleich `VOR_PRUEFUNG` zurückgeliefert wurde.

Bei E-Rezepten ergeben sich drei neue Arten von Statusänderungen bzw. - im Fall der Änderung auf `ABGERECHNET` - interessieren sich die AVS neuerdings für eine schon länger existierende Statusänderung.

1) **vor der Abrechnung**: nachträgliche Änderung eines Prüfergebnisses aufgrund von Erkenntnissen aus nachgelagerten Verarbeitungsprozessen oder aufgrund von Verfeinerungen der Prüflogik nach der initialen Prüfung

2) **im Rahmen der Abrechnung**: Übergang von einem abrechenbaren Zustand in `ABGERECHNET`

3) **nach der Abrechnung**: Übergang von `ABGERECHNET` in `RUECKWEISUNG`² oder `FEHLER` aufgrund einer Vollabsetzung durch die Krankenkassen

Bei 2) handelt es sich um eine erwartete Statusänderung für eine große Menge von Rezepten (alle abrechenbaren Rezepte bis zum einem über ApoTI nicht direkt ermittelbaren Stichzeitpunkt). Aufgrund dieser Besonderheit sind hier effiziente, ressourcenschonende Abfragestrategien³ möglich, auch ohne Erweiterung von ApoTI um Statusänderungsabfragen.

Bei den anderen Änderungen ist weder der Zeitpunkt noch ihr Eintreten für das AVS vorhersehbar, weswegen gemäß Benjamins hier derzeit die Apotheke auf einem separaten Kanal informiert werden muß. 

Fall 3) hat außerdem die Besonderheit, daß die Spezifikation hierfür die Operation `ladeRueckweisungen` direkt vorsieht (auch wenn sie bislang nur wenig oder gar nicht genutzt wird). Allerdings ergibt sich aufgrund des API-Limits von maximal 300 Datensätzen je Operation das gleiche Problem bzgl. Transaktionsalität bzw. Fortsetzbarkeit wie bei den generellen Statusänderungsabfragen, die in [ladeStatusRezept_GEAENDERTE.md][lSR_G] beschrieben sind.

[lSR_G]: ladeStatusRezept_GEAENDERTE.md

---
<sup>*1) P-Rezepte haben schon seit ihrer Einführung vor mehr als 10 Jahren das volle Statusmodell einschließlich `ABGERECHNET` usw., aber bisher hat sich dafür offensichtlich kein einziges AVS interessiert. Manche AVS laden auch jetzt noch die P-Rezepte nur auf dem Server ab, ohne jemals den Status abzufragen.*</sup>

<sup>*2) `RUECKWEISUNG` ist zwar für diesen Fall explizit vorgesehen, ist aber aufgrund der unscharfen Spezifikation und des Verwendungsverbots im Leitfaden derzeit nicht anwendbar. `FEHLER` ist auch ohne Kooperation seitens eines AVS auch jetzt schon direkt verwendbar.*</sup>

<sup>*3) basierend auf der Tatsache, daß zum Abrechnungszeitpunkt __alle__ abrechenbaren Rezepte auf `ABGERECHNET` geändert werden, sofern ihr Lieferzeitpunkt vor den Einsendeschluß für die jeweilige Abrechnung fällt; wenn eine Abfrage für eine Handvoll der ältesten offenen Liefer-Ids diese immer noch als abrechenbar statt `ABGERECHNET` zeigt, dann werden Abfragen für Tausende weitere Liefer-Ids auch kein anderes Ergebnis bringen*</sup>

---
Stand 2023-03-24