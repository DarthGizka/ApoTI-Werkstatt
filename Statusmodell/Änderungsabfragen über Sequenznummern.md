# Änderungsabfragen über Sequenznummern

<sup>Stand 2023-05-07</sup>

Hauptnachteil der in `ladeStatusRezept(perStatus: GEAENDERTE)` skizzierten Lösung ist, daß dort der Zustand der Iteration über die Statusänderungen vom Server verwaltet wird. Dadurch sind Änderungsabfragen nicht idempotent, sondern verändern den Zustand des Servers. Außerdem kann so nur eine einzige Gegenstelle überhaupt Änderungsabfragen durchführen.

Beide Probleme lassen sich dadurch umgehen, daß die Kontrolle über die Iteration an den API-Aufrufer zurückgegeben wird. 

```
rzeParamStatus
   sendHeader ...
   aenderungen
      nachSequenznummer: 1234567890123 // oder seitZeitpunkt '2023-04-16 19:17'
->
rzeLeistungStatus
   retHeader ...
   hoechsteEnthalteneSequenznummer: 1234567890423
   weitereAenderungenVorhanden: true
   statusUpd[300] ...
```

Der Server vergibt bei jeder Statuszuweisung eine monoton wachsende Sequenznummer, die zusammen mit dem Statuswert im Rezeptsatz abgespeichert wird. Dadurch stehen die aufliegenden Rezepte datenbanktechnisch auch als eine in Reihenfolge der Statusänderungen geordnete Folge zur Verfügung, die von beliebig vielen Gegenstellen unabhängig voneinander durch Angabe der Leseposition schubweise abgerufen werden kann. 

Das Protokoll begrenzt dabei die Anzahl der Datensätze in den Antworten a priori schon auf 300, so daß bei Änderungsabfragen keine Angaben zu Limits oder Obergrenzen mitgegeben werden müssen.

Die Antworten enthalten zusätzlich zum bisherigen Ergebnis jeweils die höchste Änderungsnummer (`hoechsteEnthalteneSequenznummer`). Diese Hochwassermarke kann dann direkt als Parameter `nachSequenznummer` einer anschließenden Änderungsabfrage verwendet werden.

Abspeichern der Sequenznummer zusammen mit dem Statuswert im Rezeptsatz garantiert, daß die dadurch induzierte Folge keine schalen/veralteten Werte enthalten kann. Jedes Rezept tritt höchstens ein Mal in der Folge auf; bei Statusänderungen ändert sich lediglich die Position innerhalb der Folge (i.e. im zugehörigen Index). Damit reflektiert jeder im Ergebnis enthaltene Statuswert jeweils den aktuellsten Stand; das macht im AVS den Abgleich mit den vorhandenen Daten deutlich unkomplizierter, als es ohne diese Garantie der Fall wäre. 

Die Sequenznummern müssen nicht fortlaufend sein; es muß nur garantiert werden, daß sie apothekenseitig in strikt steigender Folge sichtbar werden. Es darf nicht passieren, daß in der Datenbank neu sichtbar werdende Änderungen eine kleinere Nummer haben als die höchste apothekenbekannte Sequenznummer. Diese Änderungen würden verlorengehen.

Das bedeutet insbesondere auch, daß bei Verwendung eines SEQUENCE-Objekts zur Vergabe der Änderungsnummern zusätzliche Sperrmaßnahmen notwendig werden. Ansonsten könnten die höheren Nummern einer später beginnenden kurzen Transaktion früher in der Datenbank sichtbar werden als die niedrigeren Nummern einer lang laufenden Transaktion, die eher begonnenen hat und später endet. Die Notwendigkeit einer Koordination - also für gegenseitigen Ausschluß - beschränkt sich dabei auf Transaktionen für ein und denselben Apothekenzugang; eine Koordination mit Transaktionen anderer Apothekenzugänge ist nicht notwendig.

Strikt genommen wird die Vergabe der konkreten Nummern für neue Änderungen erst dann unumgänglich, wenn diese Änderungen in das Ergebnis einer Änderungsabfrage aufgenommen werden sollen. Das läßt Raum für alternative Implementierungen und Strategien.

Als Wertebereich für die Sequenznummern empfiehlt sich 1 bis 10^14 - 1, entsprechend einem Pattern von '[1-9]\d{0,13}' für Sequenznummern in Antworten und '0|[1-9]\d{0,13}' in Anfragen (hier Hinzunahme des Wertes 0). 

Damit ist eine serverweite - also zugangsübergreifende - Sequenzzählung auch für große RZ uneingeschränkt möglich; es ist mehr als ausreichend, um die Statusänderungen für alle Rezepte bundesweit für Tausende von Jahren zu numerieren. Die Beschränkung auf 14 Stellen erlaubt auch eine verlustfreie Darstellung als Currency (Limit 10^14 - 1 für den ganzzahligen Anteil) oder Double (ULP ≤ 1 bis 2^53, also ca. 9e15), so daß solche Sequenznummern auch Teilsysteme ohne Unterstützung von 64-Bit-Ganzzahlen verlustfrei/unverfälscht durchlaufen können.

RZs können natürlich für sich den Wertebereich problemlos auf 31 oder 32 Bit beschränken, da sie ja die Sequenznummern selber vergeben. Aber das Protokollformat muß auch für große RZ genügend Raum lassen. 

Apothekenseitig muß nur jeweils eine Sequenznummer - also die Hochwassermarke aus der letzten Antwort - abgespeichert werden, nicht eine Sequenznummer je Rezept wie im RZ. Daher ist der Speicherplatzbedarf für Sequenznummern auf Apothekenseite weitgehend irrelevant.

NB: Änderungsabfragen berichten grundsätzlich alle Statusänderungen, ohne Ausnahme. Die bisher üblichen Statusabfragen `perLieferID` sind daher nach Umstellung auf Änderungsabfragen für den Routinebetrieb weitgehend entbehrlich. Andersherum ausgedrückt: an die Stelle der üblichen zehnminütlichen Statusabfragen `perLieferID` für die Lieferungen der letzten 10 Minuten tritt dann 1 Änderungsabfrage, welche dann außer den gleichen Rezepten wie vorstehend auch noch alle anderen zwischenzeitlichen erfolgten Statusänderungen zurückgibt.

---
## DIVERSE DETAILASPEKTE
* [Semantik der Sequenznummern und Garantien](Semantik%20der%20Sequenznummern.md) (in separater Datei)
* [Notwendige Anpassungen im XML-Schema](#notwendige-anpassungen-im-xml-schema)
* [Zusätzliche verbale Protokollfestlegungen](#zusätzliche-verbale-protokollfestlegungen)
* [Signalisierung der Unterstützung von Änderungsabfragen](#signalisierung-der-unterstützung-von-änderungsabfragen)
* [Warum Sequenznummern (Ganzzahlen) anstelle von Zeitstempeln?](#warum-sequenznummern-ganzzahlen-anstelle-von-zeitstempeln)
* [Man kann die Zeit trotzdem ins Spiel bringen, aber ...](#man-kann-die-zeit-trotzdem-ins-spiel-bringen-aber)
* [Warum nur eine Hochwassermarke in der Antwort statt einer Sequenznummer in jedem Statussatz?](#warum-nur-eine-hochwassermarke-in-der-antwort-statt-einer-sequenznummer-in-jedem-statussatz)
* [Praktische Realisierbarkeit von strikt monoton sichtbar werdenden Sequenznummern](#praktische-realisierbarkeit-von-strikt-monoton-sichtbar-werdenden-sequenznummern)
* [Praktische Umsetzung in der Server-Datenbank der ARZsoftware e.G.](#praktische-umsetzung-in-der-server-datenbank-der-arzsoftware-eg)

---

## Notwendige Anpassungen im XML-Schema

```xml
<xs:element name="rzeParamStatus">
	<xs:complexType>
		<xs:sequence>
			<xs:element ref="fiverx:sendHeader"/>
			<xs:choice>
				<xs:element name="perRezeptID">
				...
				<xs:element name="perLieferID">
				...
				<xs:element name="perStatus">
				...
				<!--{ neu für Änderungsabfragen -->
				<xs:element name="geaenderte">
					<xs:complexType>
						<xs:choice>
							<xs:element name="seitZeitpunkt" type="xs:dateTime">
							<xs:element name="nachSequenznummer">
								<xs:simpleType>
									<xs:restriction base="xs:integer">
										<xs:pattern value="0|[1-9]\d{0,13}"/>
									</xs:restriction>
								</xs:simpleType>
							</xs:element>
						</xs:choice>
					</xs:complexType>
				</xs:element>
				<!--} neu für Änderungsabfragen -->
			</xs:choice>
		</xs:sequence>
	</xs:complexType>
</xs:element>
```

```xml
<xs:element name="rzeLeistungStatus">
	<xs:complexType>
		<xs:sequence>
			<xs:element ref="fiverx:retHeader"/>
			<!--{ neu für Änderungsabfragen -->
			<xs:element name="hoechsteEnthalteneSequenznummer" minOccurs="0">
				<xs:simpleType>
					<xs:restriction base="xs:integer">
						<xs:pattern value="[1-9]\d{0,13}"/>
					</xs:restriction>
				</xs:simpleType>
			</xs:element>
			<xs:element name="weitereAenderungenVorhanden" type="xs:boolean" minOccurs="0"/>
			<!--} neu für Änderungsabfragen -->
			<xs:element name="statusUpd" maxOccurs="300">
			   ...
			</xs:element>
		</xs:sequence>
	</xs:complexType>
</xs:element>
```

NB: es sollte erwogen werden, das Element `weitereAenderungenVorhanden` bei Änderungsabfragen zum Pflichtfeld zu machen. Unterm Strich würde dadurch das Protokoll einfacher und robuster, insbesondere aus Sicht des AVS. Ähnliches gilt für die Unterstützung der Abfragevariante `seitZeitstempel`.

## Zusätzliche verbale Protokollfestlegungen

<span style="color:blue">
(1) Der Auswahlbereich von Änderungsabfragen umfaßt genau die über `sendeRezepte` eingereichten Rezepte (alle Rezeptarten), `pruefeRezept` bleibt außen vor.
</span>

<p></p>
Eine Einschränkung auf E-Rezepte würde bewirken, daß AVS einen komplizierten Mix von Änderungsabfragen und klassischen Statusabfragen verwenden müssen.
<p></p>

<span style="color:blue">
(2) In Antworten auf Änderungsabfragen sind `hoechsteEnthalteneSequenznummer` und `rzLieferId` nicht optional, sondern Pflicht.
</span>

<p></p>
Die Angabe von `hoechsteEnthalteneSequenznummer` ist notwendig für Folgeabfragen (sowohl sofortige als auch spätere). Die Angabe von `rzLieferId` ist notwendig, weil sonst die Rezeptidentität ausschließlich an dem für AVS-interne Verwendung gedachten Wert `avsId` hängen würde.
<p></p>

Im klassischen Protokoll ist `rzLieferId` bei allen Rezeptarten außer regulären E-Rezepten optional; außerdem ist der Wert bei Abfrage `perLieferID` a priori gegeben (und so im Ergebnis impliziert). Bei Änderungsabfragen ist dagegen eine explizite Angabe notwendig.

## Signalisierung der Unterstützung von Änderungsabfragen

Die Unterstützung von Änderungsabfragen durch einen Server kann auch ohne Anpassung des Ergebnisses von `ladeRzDienste` (i.e. Struktur `rzeDienste`) problemlos festgestellt werden. Wenn die gewünschte Abfragevariante - also `geaenderte` mit `nachSequenznummer` oder ´seitZeitpunkt` - einfach ausgeführt wird, dann liefern spezifikationskonforme Server je nach Unterstützung der Abfragevariante unterschiedliche, determinierte Antworten.

Die Antwort ist ein SOAP-Fault mit Fehlercode ApoTI:001 ('XML nicht valide') oder ApoTI:003 ('Service nicht unterstützt'), sofern der Server die Variante nicht unterstützt. 

Im positiven Fall kommen entweder Datensätze zurück oder ein SOAP-Fault mit Code ApoTI:004 ('Keine Daten vorhanden'). Bei gezielter Wahl der Parameter - i.e. 10^14 - 1 als Sequenznummer oder ein in der Zukunft liegender Zeitstempel - kann man mit Sicherheit davon ausgehen, daß die Ergebnismenge leer ist bzw. zum Ausdruck bringen, daß die Abfrage zur Sondierung der Unterstützung dient und nicht von einer aus dem Tritt gekommenen Verwaltungslogik verursacht wurde.

## Warum Sequenznummern (Ganzzahlen) anstelle von Zeitstempeln?

Für eine korrekte Funktion des Schemas ist es notwendig, daß jede einzelne Statusänderung eines Rezeptes einen eigenen eindeutigen Schlüsselwert erhält und nicht nur jeder Änderungsvorgang (der u.U. beliebig viele Rezepte auf einen Schlag modifiziert).

Aufgrund der Größenbegrenzung für API-Antworten - max. 300 Datensätze - könnte es sonst passieren, daß bei einer Gruppe von Sätzen mit gleichem Schlüsselwert nur ein Teil am Ende einer API-Antwort Platz findet. Die nächste Iteration würde aber beim nächsthöheren Schlüsselwert fortsetzen, so daß der abgeschnittene Teil der Gruppe komplett unter den Tisch fiele. Insbesondere bei der ein Mal monatlich auftretenden Massenänderung aller abrechenbaren Rezepte auf ABGERECHNET würden große Gruppen mit gleichem Schlüsselwert entstehen, wenn man die Schlüsselwerte je Änderungsvorgang vergeben täte und nicht je Einzeldatensatz.

Bei Ganzzahlen ist eine strikt monotone Vergabe trivial, und die meisten Datenbanken unterstützen eine solche Vergabe direkt, z.B. über SEQUENCE-Objekte. Bei Zeitstempeln ist das nicht so einfach,

Innerhalb ein und desselben Serverprozesses könnte man auch Zeitstempel problemlos strikt monoton vergeben. Man müßte nur zum zuletzt herausgegebenen Zeitstempel beispielsweise eine Mikrosekunde addieren, falls die frisch vom OS geholte Zeit nicht größer sein sollte. Bei einer Auflösung von 1 Mikrosekunde würde so selbst bei 1 Million Schlüsselvergaben je Sekunde die Abweichung des Zeitstempels von der wirklichen Zeit nicht mehr als eine Sekunde betragen. Mit Datenbankmitteln könnte man das sicher auch organisieren, aber dann würde die Performance massiv einbrechen.

Außerdem gibt es keine Garantie, daß hochgenaue Zeitstempel alle Teilelemente der Infrastruktur unbeschadet durchwandern können, sowohl server- als auch klientseitig. Insbesondere laufen bei auf binären Gleitkommazahlen basierenden Darstellungen wie in COM und Delphi die meisten Zeitwerte auf unendliche Binärbrüche hinaus und können nicht exakt repräsentiert werden.

## Man kann die Zeit trotzdem ins Spiel bringen, aber ...

Die Iteration über Statusänderungen basiert zwar strikt auf Sequenznummern, aber das bedeutet nicht, daß man auf zeitbasierte Abfragen vollkommen verzichten muß. Eine Änderungsanfrage mit Parameter `seitZeitpunkt` statt `nachSequenznummer` könnte die ersten 300 Änderungen zusammen mit der üblichen Hochwassermarke zurückgeben, und der Rest der Iteration würde wie gehabt über die Sequenznummern erfolgen.

Allerdings ist dabei zu bedenken, daß die zugrundeliegende Folge der Statusänderungen keine vollständige Historie der Änderungen darstellt. Jedes Rezept tritt in der Folge an genau der Stelle auf, die dem Zeitpunkt der letzten Statusänderung entspricht. So liegt zum Beispiel ein frisch retaxiertes Rezept zwischen den Änderungssätzen für aktuelle Rezepte und nicht in dem Block von Sätzen für das Kippen auf ABGERECHNET am ursprünglichen Abrechnungstag vor vielen Monaten.

Zur Unterstützung solcher Abfragen müßte außer der Sequenznummer auch jeweils ein Zeitstempel im Rezeptsatz abgespeichert werden. Allerdings bestehen für solche Zeitstempel keine besonderen Genauigkeitsanforderungen; selbst eine Auflösung von Minuten ist für diesen Anwendungsfall mehr als ausreichend und würde deutlich weniger Speicherplatz erfordern (i.e. 4 Bytes im Fall von `smalldatetime`).

Der Hauptanwendungsfall für zeitbasierte Änderungsabfragen dürfte im Erstabgleich nach Umstellung eines AVS auf die Verwendung von Änderungsabfragen liegen. 

Das AVS kann allerdings auch ohne zeitbasierte Änderungsabfragen leicht den Anschluß finden, ohne bei 0 beginnend die Statuswerte von ein oder mehreren Jahresladungen von Rezepten abzufragen zu müssen. Über binäre Suche kann das realistisch mit ein bis zwei Dutzend Abfragen erledigt werden. Im ungünstigsten Fall sind es ca. 40. Einmalig. Auf jeden Fall um Größenordnungen weniger als bei einem anderen Schema mit 5000 bis 10000 zusätzlichen Abfragen pro Tag und Apotheke.

Unterm Strich wäre es allerdings wünschenswert, die zeitbasierte Variante der Änderungsabfrage in deren Standardumfang aufzunehmen. 4 (MSSQL) bzw. 8 (Postgres & Co.) zusätzliche Bytes je Rezept und Platz für einen passenden Index sollte sich jedes RZ problemlos leisten können ...

## Warum nur eine Hochwassermarke in der Antwort statt einer Sequenznummer in jedem Statussatz?

Es wäre möglich, jedem einzelnen Statussatz seine Sequenznummer mitzugeben, und eventuell sogar den zugehörigen Zeitstempel. Diese Werte könnten auch bei anderen Formen der Statusabfrage in den Datensätzen enthalten sein.

Das würde eventuell die eine oder andere Neugier befriedigen, aber unterm Strich würde dadurch absolut nichts gewonnen werden. Im Gegenteil. AVS-Programmierer könnten leicht in Versuchung kommen, zum Beispiel die Sequenznummern aus einer Statusabfrage `perLieferID` irgendwie verwerten zu wollen (was aber gar nicht sinnvoll möglich ist). Am Ende nutzen die zusätzlichen Daten niemandem etwas.

Die Abfragelogik braucht nur eine Hochwassermarke je Antwort, und genau das sollte auch im Protokoll wiedergespiegelt werden. Unterm Strich ist das weit weniger invasiv als eine Änderung der Struktur der Statussätze.

## Praktische Realisierbarkeit von strikt monoton sichtbar werdenden Sequenznummern

### **SEQUENCE-Objekte vs. datensatzbasierte Zähler**

SEQUENCE-Objekte ignorieren Transaktionsgrenzen; die von einer Transaktion verursachten Änderungen sind sofort für alle anderen Transaktionen sichtbar. Daher muß durch explizite Sperrmaßnahmen sichergestellt werden, daß bei gleichzeitig laufenden Transaktionen für ein und denselben Apothekenzugang die Ergebnisse nicht in der falschen Reihenfolge sichtbar werden. 

Bei Verwendung von Zähler-Datensätzen macht das DBMS das zwar selbst, aber zugangsübergreifende Massentransaktionen explizite Sperrmaßnahmen zur Vermeidung von Deadlocks erfordern.

### **Serverweite vs zugangsbezogene Sperrung**

Wenn bei der Zählung der Statusänderungen nur eine einzige serverweite Sperre verwendet wird, dann sind Deadlocks a priori ausgeschlossen. Allerdings kann das schnell zu einem Engpaß werden, weil das DBMS dann effektiv *alle* parallel laufenden statusändernden Transaktionen serialisieren muß und nicht nur diejenigen, die den gleichen Zugang betreffen.

Bei zugangsbezogener Sperrung kommen sich nur solche Transaktionen potentiell ins Gehege, die den gleichen Zugang betreffen. Allerdings kann es dann zu Deadlocks kommen, wenn bei parallel laufenden statusändernden Transaktionen mehrere Zugänge involviert sind.  Bei AVS-seitigen Statusänderungen (Neueinreichung/Stornierung) kann das prinzipiell nicht passieren, aber sehr wohl bei vom RZ angestoßenen Änderungen wie Schreiben mehrerer Prüfergebnisse in ein und derselben Transaktion oder monatlicher Zapfenstreich. In solchen Fällen kann nicht ohne weiteres garantiert werden, daß die gleichen Sperren immer in der gleichen Reihenfolge akquiriert werden, und damit sind Deadlocks vorprogrammiert. Dabei ist es unerheblich, ob Sperrungen implizit vom DBMS arrangiert werden oder explizit durch entsprechende Abfrageoptionen oder Funktionsaufrufe.

Bei zugangsbezogener Sperrung ist es daher ein gegenseitiger Ausschluß von *zugangsübergreifenden* statusändernden Transaktionen notwendig.

### **Serverweite vs zugangsbezogene Zählung**

Die in den vorstehenden Abschnitten diskutierte Wahl der Zähler- und Sperrmechanismen ist weitgehend orthogonal zu der Entscheidung, ob Statusänderungen serverweit gezählt werden oder je Zugang. Allerdings haben nicht alle der acht möglichen Kombinationen wirklich Sinn; so müßte man zum Beispiel bei Verwendung eines serverweiten datensatzbasierten Zählers die normale Sperrlogik des DBMS erst künstlich aushebeln, um mit zugangsbasierter Sperrung arbeiten zu können. 

Die simpelste Variante ist ein serverweiter datensatzbasierter Zähler. Hier arrangiert das DBMS selber alles notwendige; es gibt keinen Bedarf für expliziten Sperrmaßnahmen.

Bei der Verwendung von zugangsbezogener datensatzbasierter Zählung erledigt das DBMS automatisch die zugangsbezogene Sperrung. Es muß nur der gegenseitige Ausschluß von zugangsübergreifenden statusändernden Transaktionen explizit arrangiert werden (zwecks Deadlock-Vermeidung). Alle zugangspezifischen Transaktionen können diese globale Sperre komplett ignorieren,

Diese Betrachtungen sind allerdings weitgehend hinfällig, wenn die Zählung in einer Funktion bzw. Stored Procedure gekapselt wird. In diesem Fall sind die drei diskutierten Aspekte weitgehend frei wähl- und veränderbar; lediglich die Akquisition einer eventuell notwendigen globalen Massensperre kann nicht hinter dieser Abstraktion versteckt werden und erfordert explizite Kodierung in jeder zugangsübergreifenden statusändernden Transaktion (bzw. eine weitere Abstraktion in Form einer Stored Procedure für eventuell nötige Massensperren).

Gleiches gilt für Trigger. Die Numerierung der Statusänderungen kann leicht in einen BEFORE-Trigger für INSERT und UPDATE verfrachtet werden, so daß fast alle bestehenden Abfragen komplett unverändert bleiben. Lediglich zugangsübergreifende statusändernde Massentransaktionen benötigen explizite Maßnahmen für den gegenseitigen Ausschluß zwecks Deadlock-Vermeidung, sofern zugangsbezogene Sperrung involviert ist.

Last but not least: bei serverbezogener Numerierung kann jedes Klientsystem über das Drei-Stichproben-Verfahren den täglichen Rezeptdurchsatz eines RZ überraschend genau bestimmen ...

## Praktische Umsetzung in der Server-Datenbank der ARZsoftware e.G.

Konkrete Implementierung in der Server-Datenbank und Test mit Produktionsdaten zeigte, daß Änderungsabfragen einfach und unkompliziert umsetzbar sind, mit gleichem Effizienzniveau wie Statusabfragen `perLieferID`. 

CPU- und I/O-Last waren bei gleicher Zahl der Anfragen und gleicher Größe der Antworten tendenziell sogar etwas geringer.  AVS-abhängig kann die Anzahl der notwendigen Änderungsabfragen auch kleiner sein als bei `perLieferID` (i.e. genau 1 alle 10 Minuten statt 1 oder mehrere alle 10 Minuten), was sich ebenfalls positiv auf die Gesamtlast auswirkt.

Die Aktualisierung der (zugangsbezogenen) Sequenznummern und der zugehörigen Zeitstempel wurde über einen einfachen INSERT-/UPDATE-Trigger realisiert. Dadurch waren keinerlei Eingriffe in existierende Abfragen im ApoTI-Teil des Servers nötig.

 Die einzige Komplikation war die Notwendigkeit, serverinterne Teilbereiche (Prüfung, Zapfenstreich usw.) auf potentiell zugangsübergreifende statusändernde Massentransaktionen durchzusehen, zwecks Hinzufügen des Aufrufs einer Stored Procedure für die Massensperre (gegenseitiger Ausschluß zwecks Deadlock-Vermeidung).

Konkrete Performancewerte stehen nur für meine MSSQL-Implementierung zur Verfügung, wo ich für jede einzelne Abfrage die Instrumentierung über `SET STATISTICS IO` C#-seitig separat schalten kann. 

Eine bei 0 beginnende Abfragekette für alle aufliegenden Rezepte einer guten Apotheke (ca. 1 Jahr) ergab dort folgende Werte:

```
83 ms für 168 Abfragen mit insgesamt 50368 Rezepten -> 2026 Abfragen/s, 607334 Rp/s
STATISTICS IO: 1209 Logical Reads für 168 Abfragen -> 7,20/Abfrage
```

Für die Abfragen über ApoTI sind diese Zahlen natürlich wenig aussagekräftig, da dort noch die Rundreisezeiten im Internet und Zeit für die Verarbeitung im AVS dazukommen, sowie 1-2 ms für den HTTPS-, SOAP- und XML-Overhead im Server. Die Zahlen geben aber gut an, in welchem Maß diese Abfragen zur Serverlast beitragen. 

Abruf der gleichen Information via `perLieferID` würde 12433 ApoTI-Aufrufe erfordern und 51949 Datensätze ergeben. Das sind 1581 Datensätze mehr, weil erneut eingereichte Rezepte nicht nur in ihrer eigenen Lieferung erscheinen, sondern auch in der Lieferung desjenigen Rezepts, das sie ersetzen.

Die Abfrage sieht so aus:

```sql
select top 301
	status_lsn,
	rezepttyp,    -- 'e', 'm' oder 'p' (könnte auch am Wertebereich der Rezept-Id abgelesen werden)
	rezept_id,    -- Einheiz-Id (BigInt) für E-, M- und P-Rezepte
	rezeptstatus,
	avs_id,
	rz_liefer_id
	from sv4.Rezept with (SERIALIZABLE)
	where klient_id = @KlientId and status_lsn > @SeqNr
		and hinfaellig = 0
	order by 1;
```

`SERIALIZABLE` ist notwendig, weil MSSQL bei schwächerer Isolierung grundsätzlich keine korrekten, konsistenten Ergebnisse garantiert. Das Limit ist 301 statt 300, damit das Feld `weitereAenderungenVorhanden` in der Antwort gesetzt werden kann; der 301. Datensatz wird dann jeweils weggeworfen.