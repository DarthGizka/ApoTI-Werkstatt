# Semantik der Sequenznummern und Garantien

Dem auf Protokollebene sichtbaren Verhalten des Systems liegt folgendes Modell zugrunde:

<span style="color:blue">
(S1) Alle zu einem gegebenen Zeitpunkt t im Server vorliegenden Rezepte eines Zugangs haben eindeutige Sequenznummern zwischen s0 und s1 (nicht notwendigerweise fortlaufend).
</span><p></p>

<span style="color:blue">
(S2) Bei einer nach Zeitpunkt t stattfindenden Statusänderung erhält das betroffene Rezept eine neue eindeutige Sequenznummer > s1
</span><p></p>

<span style="color:blue">
(S3) Eine Antwort auf eine Änderungsabfrage enthält von allen die jeweilige Bedingung erfüllenden Datensätzen grundsätzlich die mit den kleinsten Sequenznummern.
</span><p></p>

Diese Garantien erscheinen auf den ersten Blick vielleicht trivial und wenig nützlich, sind aber hinreichend für eine einfache und zuverlässige Umsetzung von Änderungsabfragen.

S1 garantiert i.V.m. S3, daß ein AVS alle zum einem gegebenen Zeitpunkt auf dem Server liegenden Statuswerte durch wiederholte Änderungsabfragen schubweise vollständig abrufen kann, sofern es in der Zwischenzeit nicht zu Statusänderungen kommt. Die letzte nichtleere Antwort enthält dann s1 im Feld `hoechsteEnthalteneSequenznummer` und ggf. weniger als 300 Datensätze.

S2 garantiert, daß schubweiser Abruf auch dann vollständig möglich ist, wenn während der Abrufkette Statusänderungen auftreten. Je nach Art der Überlappung zwischen den einzelnen Abrufen und den Statusänderungen können die geänderten Rezepte dann u.U. in mehreren Antworten auftreten, aber es werden keine Rezepte übersprungen, und es können auch keine Statusänderungen verlorengehen.

Dieses Modell bezieht sich auf das beobachtbare Verhalten des Systems. Das bedeutet insbesondere, daß interne Vergabe der Sequenznummern in strikt wachsender Folge allein nicht ausreichend ist, wenn die so vergebenen Nummern nicht auch in strikt wachsender Folge für Änderungsabfragen *sichtbar* werden. In Bezug auf DBMS ist also der COMMIT-Zeitpunkt entscheidend, nicht der Zeitpunkt des Abholens einer Sequenznummer von einem SEQUENCE-Objekt oder Zähler-Datensatz.

<span style="color:blue">
(S4) Die Garantien gelten nur in Bezug auf die Sequenznummern ein und desselben Zugangs.
</span><p></p>

Es nicht spezifiziert, wie sich Sequenznummern *unterschiedlicher* Zugänge zueinander verhalten, auch bei offensichtlich zugangsübergreifender Vergabe der Sequenznummern durch das RZ. Zum Garantieren des spezifizierten Verhaltens ist eine Koordination parallel laufender statusändernder Transaktionen nur für solche Transaktionen notwendig, die Rezepte ein und desselben Zugangs betreffen.

## Mehrfaches Auftreten von Rezepten innerhalb ein und derselben Antwort

Bei Statusänderungen während einer Kette von Statusänderungsabfragen können bereits übermittelte Rezepte in späteren Antworten der gleichen Serie nochmals auftreten. Das ist unvermeidlich¹ und Ausdruck des korrekten Funktionierens im Sinne dieser Spezifikation.

Mehrfaches Auftreten eines Rezepts innerhalb ein und derselben Antwort wäre an und für sich unschädlich, sofern die Datensätze strikt nach Sequenznummer sortiert sind und das AVS die Sätze in dieser Reihenfolge verarbeitet. Die Situation ist effektiv auch nicht anders als beim Auftreten ein und desselben Rezepts in verschiedenen Antworten.

Es ist allerdings möglich, eine stärkere Garantie zu geben, ohne daß sich daraus zusätzlicher Aufwand auf Implementierungsseite ergibt.

<span style="color:blue">
(S5) Analog zu `sendeRezepte` darf ein und dasselbe Rezept in ein und derselben Antwort nicht mehrfach enthalten sein. Das betrifft sowohl Datensätze für ein und dieselbe Einreichung eines Rezepts (gleiche RZLieferId) als auch mehrfache Einreichungen von Rezepten mit gleicher Rezept-Id (RZLieferId unterschiedlich).
</span><p></p>

Hintergrund: die Maßnahmen zum Garantieren korrekter Abfrageergebnisse für Änderungsabfragen (i.e. Isolierungsniveau `SERIALIZABLE` bei MS SQL Server) verhindern auch das Auftreten verschiedener Versionen ein und desselben Datensatzes in einer Antwort. Bei einer Neueinreichung eines Rezepts wird gemäß der klassischen Protokollsemantik der alte Datensatz entweder ersetzt oder hinfällig gemacht, so daß alte Datensatz auch hier auf jeden Fall eine Änderung erfährt. Damit wird auch das mehrfache Auftreten von Rezept-Ids (also verschiedener Einreichungen ein und desselben Rezepts) in ein und derselben Antwort effektiv verhindert.

---
<sup>*1) 'unvermeidlich' in dem Sinne, daß zur Vermeidung dieses harmlosen Phänomens im Server ein immenser Aufwand notwendig wäre: der Server müßte zu Beginn einer Abfragekette einen Schnappschuß des momentanen Zustands aller Rezepte ab der angegebenen Schwelle (Sequenznummer oder Änderungszeitpunkt) anfertigen und dem AVS ein Token für diesen Schnappschuß zurückgeben; das AVS müßte das Schnappschuß-Token dann bei jeder Folgeabfrage mit angeben*; last but not least wäre auch die Vorhaltedauer der Schnappschußdaten zu klären ...</sup>

---
Stand 2023-05-01
