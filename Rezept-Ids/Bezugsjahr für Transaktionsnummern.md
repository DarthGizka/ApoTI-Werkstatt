# Bezugsjahr für Transaktionsnummern

Die Transaktionsnummern von Papierrezepten mit Zusatzdaten dienen zusammen mit dem Apotheken-IK zur eindeutigen Zuordnung eines Zusatzdatensatzes zu einem gegebenen Rezept.

<sup>*NB: der auf dem Papierrezept aufgedruckt Hash verbindet das Rezept effektiv unlösbar mit einem ganz bestimmten Datensatz(-inhalt). In vielen Situationen ist dieser Hash wesentlich besser als Schlüssel für einen Zusatzdatensatz geeignet als das Tripel Apotheken-IK × Bezugsjahr × Transaktionsnummer. Für die in diesem Artikel diskutierte Problematik ist das jedoch unerheblich.*</sup>

Die technischen Anlagen fordern, daß *'mindestens ein Jahr lang keine doppelte Transaktionsnummer auftritt'* (TA1), und zwar *'innerhalb der in ZDR-03 angegebenen Apotheke'* (TA3). ZDR-03 ist dabei das auf dem Rezept angegebene Apotheken-IK, also das IK der Filiale. Der Bezug für das Jahr wird aber nicht klargestellt. Die ApoTI-Spezifikation hilft hier auch nicht weiter.

Bei Auslassen der weniger wahrscheinlichen Möglichkeiten verbleiben zwei:

1) Jahr des Abgabedatums
2) Jahr des Erstellungszeitpunktes des Datensatzes

Auf dem Papierrezept steht nur das Abgabedatum zur Verfügung, nicht aber der Erstellungszeitpunkt des Datensatzes. Logisch gesehen kommt also eigentlich nur das Abgabedatum als Quelle des Bezugsjahres in in Frage.

**TODO** Was ist der VDARZ-Konsens hier?

---
## Praktische Konsequenzen

Beim Bezugsjahr aus dem Abgabedatum gibt es zwei kleinere Komplikationen:

1) Das tatsächliche Abgabedatum auf dem Rezept weicht gelegentlich von dem über ApoTI übermittelten ab.
2) Fremdhersteller von Rezepturen u.ä. kennen nur den Erstellungszeitpunkt des Datensatzes; das Abgabedatum können sie bestenfalls raten.

Das führt über Jahreswechsel hinweg immer wieder zu einer gewissen Menge an Problemen, wenn nämlich das ApoTI-Abgabedatum in ein anderes Jahr fällt als das gedruckte (welches für Abrechnung und Datenlieferung maßgeblich ist).

Für die Arbeit im Rechenzentrum läßt sich das Problem durch Hinzuziehen des Hashwertes weitgehend automatisch lösen, aber bei der Rezeptsuche/Statusabfrage über ApoTI hilft das nichts.

Man könnte zunächst meinen, das Abfrage über die Liefer-Id gegen das Problem mit dem Bezugsjahr gefeit seit sollte. Dem ist aber nicht so. Per definitionem soll bei Abfrage über die Liefer-Id jeweils die neueste Inkarnation jedes Rezepts zurückgegeben werden, dessen Rezept-Id in der ursprünglichen Lieferung enthalten war (siehe [Highlander-Regel][HlR]). Und die Rezept-Id eines P-Rezeptes ist halt Bezugsjahr × Transaktionsnummer ...

Abrechnungsrelevant wird die Frage des Bezugsjahrs durch die Tatsache, daß Emmendingen schon Datenlieferungen wegen Auftretens bereits gelieferter Transaktionsnummern abgelehnt hat.

[HlR]: Highlander-Regel.md

---
Stand 2023-02-18