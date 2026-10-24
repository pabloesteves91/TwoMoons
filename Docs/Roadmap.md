# TwoMoons – Roadmap: Store-Level, Upgrades und neue Bereiche

> Plan für den Ausbau vom leeren Laden bis zum legendären Laden auf **Store-Level 7**.
> Grundidee: **7 Kapitel = 7 Store-Level.** Jedes Kapitel endet mit einem Level-Aufstieg,
> jedes Level schaltet neue Möbel, Deko, Bereiche und eine Fähigkeit frei.

---

## 1. Zeit und Kapitel-Länge

### Spieltag
| | |
|---|---|
| Öffnungszeit | 9–18 Uhr = **7 Minuten Echtzeit** |
| 1 Spielstunde | ca. **47 Sekunden** |
| Turnier (3 Spielstunden) | ca. **2 Minuten 20 Sekunden** |
| Vorbereitung morgens / Abrechnung abends | ohne Zeitdruck |

Mit Vorbereitung und Abrechnung dauert ein ganzer Spieltag etwa **9–10 Minuten**.

### Wie lang ein Kapitel dauert
Kapitel 1–3 dauern je etwa **1 Stunde**, Kapitel 4–7 je etwa **90 Minuten**. Jedes Kapitel besteht aus
**Etappen** mit je 3–5 Aufgaben; zwischen den Etappen gibt es kleine Story-Momente (roter Faden).

| Kapitel | Spieltage | Echtzeit ca. |
|---|---|---|
| 1 Der Traum beginnt | 6–7 | 1 Std. |
| 2 Die ersten Stammkunden | 6–7 | 1 Std. |
| 3 Das erste Turnier | 6–7 | 1 Std. |
| 4 Handeln & Drucken | 9–10 | 1 Std. 30 Min. |
| 5 TwoMoons wächst | 9–10 | 1 Std. 30 Min. |
| 6 Der Ruf eilt voraus | 9–10 | 1 Std. 30 Min. |
| 7 Das große Turnier | 9–10 | 1 Std. 30 Min. |
| **Gesamt** | **ca. 55–60 Tage** | **ca. 9 Stunden** |

**Aufgaben:** Das Kapitel folgt immer dem Store-Level. Erledigte Aufgaben verschwinden aus der Liste;
ist ein Kapitel geschafft, verschwindet die Liste (oder zeigt nur noch die fehlenden XP).
Alle Kapitel-Aufgaben stehen in `Assets/Scripts/Story/ChapterData.cs`. Aufgaben zu noch nicht gebauten Funktionen
(Turniere, Spellforge, 3D-Druck, Grading …) kommen dazu, sobald es die Funktion gibt.

**Kapitel 1 in Etappen:**
1. *Der erste Tag* – putzen, erstes Regal, einräumen, öffnen, 5 Kunden
2. *Ein richtiger Laden* – 30 Packs verkaufen, 2 Regale, 2 Tage mit Gewinn
3. *Gemütlich soll es sein* – 3 Deko, Gemütlichkeit 5, 25 Kunden
4. *Der Laden läuft* – 80 Packs, an einem Tag 20 Packs, 3 weitere Gewinn-Tage, 50 Kunden

Danach kann man frei weiterspielen (Endlos-Modus mit allen Freischaltungen).

---

## 2. Store-Level und Laden-Erfahrung (XP)

Der Laden sammelt **Laden-Erfahrung (XP)**. Ist die XP-Leiste voll **und** sind die Kapitel-Aufgaben
erledigt (entschieden), steigt der Laden ein Level auf → **Level-Up-Bildschirm** mit Fanfare und allen Freischaltungen.

**Ruf** bleibt getrennt davon: Er bestimmt, wie viele Kunden kommen und wie geduldig sie sind.
**Level** bestimmt, was man kaufen und bauen darf.

| Level | XP nötig | Titel |
|---|---|---|
| 1 | 0 | Leerer Laden |
| 2 | 1.200 | Kleiner Kartenladen |
| 3 | 3.500 | Treffpunkt |
| 4 | 7.000 | Sammlerparadies |
| 5 | 12.000 | Spielerheimat |
| 6 | 18.000 | Stadtbekannt |
| 7 | 26.000 | **Legendär** |

**XP gibt es für:**
- 1 XP pro 1 € Umsatz
- 5 XP pro zufriedenem Kunden
- 20 XP pro erledigter Aufgabe
- 60 XP pro Turnier (+ Bonus bei vollem Turnier)
- 10 XP pro verkauftem 3D-Druck
- Erstes Mal X (erste Ultra-Seltene, erste Vitrinen-Karte verkauft, …): 25 XP

---

## 3. Freischaltungen pro Level

| Level | Kapitel | Neu freigeschaltet |
|---|---|---|
| **1** | Der Traum beginnt | Regal, Kasse, Computer, Pack-Tisch · Monstera · Deko: Topfpflanze, Poster |
| **2** | Die ersten Stammkunden | **Spellforge** · Vitrine · Zubehör-Ständer (Hüllen, Ordner, Spielmatten) · Deko: Teppich, Wanduhr, Lichterkette |
| **3** | Das erste Turnier | **Turniere** · Spieltisch · Turnier-Counter (Prizepool) · Turnier-Tafel · **Soul Clash** · **Couch-&-TV-Ecke mit Konsole** · Getränkekühlschrank · **1 Mitarbeiter** · Deko: Wimpel, Pokalregal |
| **4** | Handeln & Drucken | **Ankauf** (Kunden bringen Karten) · **3D-Drucker** + Filament · Druck-Regal · Deko: Neon-Schild |
| **5** | TwoMoons wächst | **Karten-Grading** (Grading-Station) · **Ladenerweiterung** (zweiter Raum) |
| **6** | Der Ruf eilt voraus | Premium-Vitrine (8 Karten, beleuchtet) · Großturniere (8 statt 4 Spieler pro Tisch-Runde) · schnellerer 3D-Drucker · Deko: Arcade-Automat, Kunstrasen-Ecke |
| **7** | Das große Turnier | Stadtturnier · Trophäenwand · Two-Moons-Statue · alle Deko-Varianten |

---

## 4. Möbel & Deko (Computer → neuer Tab „Einrichtung“)

Alles wird am Computer gekauft und mit **[B]** im Bau-Modus aufgestellt (wie Regale heute).
Gesperrtes ist sichtbar, aber grau mit „ab Level X“ – so weiß man, worauf man hinarbeitet.

**Gemütlichkeit:** Jede Deko gibt Gemütlichkeits-Punkte. Mehr Gemütlichkeit =
Kunden sind geduldiger (bis +30 %) und es gibt jeden Abend etwas Ruf-Bonus.
Flecken ziehen Gemütlichkeit ab.

| Gegenstand | Level | Preis | Wirkung |
|---|---|---|---|
| Topfpflanze | 1 | 15 € | +2 Gemütlichkeit |
| Poster (3 Motive) | 1 | 10 € | +1 Gemütlichkeit |
| Teppich | 2 | 40 € | +3 Gemütlichkeit |
| Wanduhr | 2 | 20 € | +1, zeigt die Uhrzeit im Laden |
| Lichterkette | 2 | 25 € | +3, leuchtet abends |
| Zubehör-Ständer | 2 | 50 € | verkauft Hüllen, Ordner, Matten |
| Spieltisch (4 Plätze) | 3 | 120 € | Turniere, freies Spielen |
| Turnier-Counter | 3 | 150 € | Prizepool vorbereiten |
| Turnier-Tafel | 3 | 40 € | zeigt nächstes Turnier & Spiel |
| Pokalregal | 3 | 60 € | +4, füllt sich mit gewonnenen Pokalen |
| 3D-Drucker | 4 | 300 € | druckt Figuren |
| Druck-Regal | 4 | 70 € | verkauft 3D-Drucke |
| Neon-Schild „TwoMoons“ | 4 | 90 € | +5 Gemütlichkeit |
| Couch | 3 | 200 € | Couch-Ecke |
| TV + Konsole | 3 | 250 € + 180 € | Couch-Ecke |
| Getränkekühlschrank | 3 | 160 € | verkauft Getränke |
| Grading-Station | 5 | 400 € | Karten bewerten und versiegeln |
| Ladenerweiterung | 5 | 1.000 € | zweiter Raum (+10 Spalten) |
| Premium-Vitrine | 6 | 250 € | 8 Karten, Sammler zahlen +10 % |
| Arcade-Automat | 6 | 400 € | +6, Kunden spielen 1 € pro Runde |
| Trophäenwand | 7 | 300 € | +8, zeigt alle Turniersiege |

---

## 5. Turniere (ab Level 3)

### Ablauf
1. **Ankündigung:** Jeden **2. Tag** ist Turniertag. Die Turnier-Tafel zeigt das Spiel – die drei Spiele
   wechseln sich ab: **Monstera → Spellforge → Soul Clash**.
2. **Hersteller-Packs:** Am Morgen des Turniertags liefert der Spielehersteller einen Karton
   **Gratis-Turnierpacks** (Anzahl wächst mit dem Level, z. B. 8 / 12 / 16 Packs). Diese Packs sind nur
   für den Prizepool gedacht.
3. **Prizepool vorbereiten:** Den Karton zum **Turnier-Counter** tragen und dort die Preise einteilen:
   1. Platz, 2. Platz, 3. Platz, Teilnahmepreis. Optional eigene Karten aus der Sammlung dazulegen
   → größerer Prizepool lockt mehr Spieler und gibt mehr Ruf.
4. **Anmeldung:** Über den Tag kommen Turnierspieler und zahlen **5 € Startgeld** an der Kasse (entschieden).
   Wie viele kommen, hängt ab von Ruf, Prizepool und Anzahl der Spieltisch-Plätze.
5. **Turnier (3 Spielstunden, z. B. 14–17 Uhr):** Die Spieler sitzen an den Spieltischen und spielen
   (Karten-Animationen, kleine Jubel- und Ärger-Symbole). Eine Fortschrittsanzeige zeigt Runde 1/3, 2/3, 3/3.
   Der normale Laden läuft nebenbei weiter – Turnierspieler kaufen in den Pausen Packs und Zubehör.
6. **Siegerehrung:** Zum Schluss versammeln sich alle am Turnier-Counter, **jubeln**, und man übergibt
   per [E] die Preise in Reihenfolge der Platzierung: 3. → 2. → 1. Platz, mit Fanfare und Konfetti.
7. **Ergebnis:** XP, Ruf, Pokal fürs Pokalregal. **Nicht vorbereiteter Prizepool** → enttäuschte Spieler,
   Ruf sinkt.

### Größere Turniere
- Level 3: 1 Spieltisch, bis 4 Spieler
- Level 4–5: mehrere Tische, bis 8 Spieler
- Level 6: Großturnier bis 16 Spieler
- Level 7: **Stadtturnier** (Story-Finale)

---

## 6. 3D-Drucker (ab Level 4)

1. **Drucker kaufen** (300 €) und aufstellen.
2. **Filament kaufen** am Computer: Spulen in Rot, Gold, Grün, Blau (je 15 €, reicht für mehrere Drucke).
3. **Filament einlegen:** Spule tragen und [E] am Drucker.
4. **Modell wählen:** Figuren aus den drei Spielen, z. B.
   | Modell | Druckzeit | Filament | Verkaufswert |
   |---|---|---|---|
   | Mooskröte (klein) | 1 Std. | 1 | 8 € |
   | Flammfalke (mittel) | 2 Std. | 2 | 18 € |
   | Zwielichtdrache (groß) | 3 Std. | 4 | 45 € |
5. **Drucken:** Fortschrittsbalken über dem Drucker, kleine Animation (Druckkopf fährt hin und her).
6. **Fertig:** Figur abholen und ins **Druck-Regal** stellen → Kunden kaufen sie.
7. Gelegentlich **Druckfehler** (Filament verheddert) → [E] zum Beheben, sonst bleibt der Drucker stehen.
8. Level 6: schnellerer Drucker, mehrfarbige Figuren (teurer verkaufbar).

---

## 7. Couch-&-TV-Ecke (ab Level 3)

- **Couch + TV + Konsole** aufstellen.
- Kunden setzen sich und **zocken** (Bildschirm flackert, Controller-Animation). Sie bleiben länger im Laden,
  sind entspannter und kaufen eher Getränke und Packs.
- **Zocken ist gratis** – dafür sind die Kunden danach glücklicher (mehr Ruf) und kaufen deutlich mehr
  Getränke und Packs.
- Mit **Getränkekühlschrank** daneben: Getränke als neue Ware.
- Ab und zu **Konsolen-Abend**: kleine Bonusaufgabe, die Ecke voll zu bekommen.

---

## 7b. Mitarbeiter (ab Level 3)

- **1 Mitarbeiter** ab Level 3, kostet Lohn pro Tag (z. B. 30 €, wird bei der Abrechnung abgezogen).
- Man legt seine Aufgabe fest: **Kasse** (kassiert selbst) oder **Regale** (füllt aus Kartons nach).
- Er hat einen Namen und kleine Sprüche – und gehört später zur Story (er lernt Papa kennen).

## 7c. Karten-Grading (ab Level 5)

1. **Grading-Station** kaufen (400 €) und aufstellen.
2. Eine Karte aus der Sammlung einlegen, Grading kostet **5 € pro Karte**.
3. Prüfen dauert **1 Spielstunde** – Fortschrittsbalken über der Station (Lupe, Licht, Messung).
4. Ergebnis: **Note 1–10**, die Karte wird in einer Schutzhülle versiegelt („Slab“).
5. Bewertete Karten sind in der Vitrine mehr wert:
   | Note | Wert |
   |---|---|
   | 10 (makellos) | × 5 |
   | 9 | × 3 |
   | 8 | × 2 |
   | 7 | × 1,5 |
   | 6 oder weniger | × 1 |
6. Sammler kaufen bevorzugt bewertete Karten; eine **10er Ultra-Seltene** ist ein Highlight mit eigenem Sound.
7. Im Sammelbuch sieht man die beste Note jeder Karte.

## 8. Fähigkeiten

Mit jedem Level-Aufstieg gibt es **1 Fähigkeitspunkt** (insgesamt 6). Man wählt aus:

| Fähigkeit | Wirkung |
|---|---|
| Flinke Finger | Kassieren und Einräumen doppelt so schnell |
| Charmeur | Kunden +20 % Geduld |
| Lagerprofi | 2 Kartons gleichzeitig tragen |
| Verhandlungsgeschick | Ankauf 15 % günstiger, Sammler zahlen 10 % mehr |
| Turnier-Organisator | +2 Plätze pro Turnier, Hersteller schickt mehr Packs |
| Tüftler | 3D-Drucke und Grading 25 % schneller, weniger Druckfehler |
| Ordnungsfanatiker | Weniger neue Flecken pro Tag |
| Glückspilz | Etwas mehr „NEU!“-Karten beim Pack-Öffnen |

---

## 9. Papa als roter Faden

| Kapitel | Papa-Moment |
|---|---|
| 2 | Papa schickt einen Brief: alte Fotos vom Laden, wie er früher aussah |
| 3 | Papa kommt zum **ersten Turnier** und schaut vom Rand aus zu |
| 4 | Man findet Papas altes Notizbuch mit Druck-Ideen und einer Spur zur Two-Moons-Karte |
| 5 | Papa geht es schlechter – man ruft ihn nach Ladenschluss an |
| 6 | Papa hilft einen Tag lang an der Kasse mit, so lange er kann |
| 7 | Beim **Stadtturnier** sitzt Papa in der ersten Reihe. Im Prizepool des Herstellers: **die Two-Moons-Karte**. |

---

## 10. Umsetzungs-Reihenfolge

| Schritt | Inhalt | Ergebnis |
|---|---|---|
| **A** ✅ | Store-Level, XP-Leiste, Level-Up-Bildschirm, Freischalt-System, Tab „Einrichtung“, Deko & Gemütlichkeit | Grundgerüst für alles Weitere (umgesetzt: Pflanze, Poster, Teppich, Wanduhr, Lichterkette) |
| **B** | Kapitel 2 (Level 2): Spellforge, Zubehör-Ständer, erste Papa-Szene | Level 1 → 2 spielbar |
| **C** | Kapitel 3 (Level 3): Spieltische, Turnier-Counter, Hersteller-Packs, Turnierablauf, Siegerehrung, Soul Clash, Couch-&-TV-Ecke, Getränke, 1 Mitarbeiter | Turniere und Couch-Ecke spielbar |
| **D** | Kapitel 4 (Level 4): 3D-Drucker, Filament, Druck-Regal, Ankauf | Drucken & Handeln |
| **E** | Kapitel 5 (Level 5): Karten-Grading, Ladenerweiterung | Laden wird groß |
| **F** | Kapitel 6–7: Großturniere, Premium-Deko, Finale mit Papa und der Two-Moons-Karte | Story fertig |
| **G** | Fähigkeiten-Menü, Feinschliff, Balance | Rund und fertig |

Jeder Schritt ist für sich spielbar und wird einzeln getestet, bevor der nächste beginnt.
