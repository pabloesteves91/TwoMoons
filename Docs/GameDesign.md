# TwoMoons – Game Design

> Ausbau-Plan mit Store-Level 1–7, Turnieren, 3D-Druck, Couch-Ecke und Fähigkeiten: siehe [Roadmap.md](Roadmap.md).

## Idee

Ein 2D-Top-Down-Spiel, in dem man den TCG-Laden **TwoMoons** von Grund auf aufbaut:
Ware einkaufen, Regale bestücken, Kunden bedienen, Boosterpacks öffnen und den Laden
Schritt für Schritt vergrößern. Eine Geschichte begleitet den Weg vom leeren Laden
bis zum Ziel.

## Grundentscheidungen

| Thema | Entscheidung |
|---|---|
| Ansicht | 2D, von oben (Top-Down) |
| Grafik | Pixel-Art im 16×16-Raster. Zuerst Platzhalter, Logo und Pixelbilder werden später durch eigene ersetzt |
| Karten | Drei erfundene Kartenspiele mit eigenen Namen und Designs (keine echten Marken) |
| Boosterpacks | Großes Highlight: Öffnen mit Animation, Seltenheitsstufen, Sammelbuch, wertvolle Karten verkaufen |
| Steuerung | Kombiniert: Figur läuft durch den Laden (WASD/Controller), Bau-Modus mit der Maus zum Einrichten |
| Ziel | Story mit Figuren, Aufgaben und einem Ende |

## Spielablauf

1. **Einkaufen** – Boosterpacks und Displays am Computer beim Großhändler bestellen. Die Lieferung kommt sofort an den Eingang.
2. **Einräumen** – Mit der Figur Ware ins Regal stellen.
3. **Preise** – Am Computer per Schieberegler (1–10 €, 10-Cent-Schritte) festlegen. Kunden vergleichen mit dem
   Marktwert: ab Marktwert steigt die Chance, dass sie nicht kaufen (ab +60 % kauft keiner), bis 85 % vom
   Marktwert gilt es als Schnäppchen (mehr Packs, mehr Ruf). Angezeigt werden Einkauf, Marktwert, Preis und Gewinn pro Pack.
4. **Verkaufen** – Kunden laufen herum, nehmen Ware und bezahlen an der Kasse.
5. **Packs öffnen** – Seltene Karten behalten, verkaufen oder ausstellen.
6. **Ausbauen** – Laden vergrößern, Möbel, Spieltische, Turniere, Mitarbeiter.

## Zeit

Das Spiel läuft in Tagen: Morgens öffnet der Laden, abends schließt er. Danach folgen
Abrechnung, Ware bestellen und Schlafen, dann beginnt der nächste Tag.

## Angebot im Laden

- **Boosterpacks und Displays** der drei Kartenspiele
- **Einzelkarten** – wertvolle Karten in einer Vitrine ausstellen und teuer verkaufen
- **Ankauf** – Kunden bieten Karten an, der Preis wird ausgehandelt
- **Spieltische und Turniere** – Kunden spielen im Laden, Turniere bringen Ruf und Einnahmen
- **Zubehör** – Hüllen, Ordner, Spielmatten, Snacks und Getränke

## Pack-Öffnen, Sammelbuch und Vitrine

- **Pack-Tisch** hinter der Theke. Packs kommen aus dem eigenen Lager: ein Pack aus dem Regal nehmen
  (leere Hände, [E] am Regal) oder einen Karton tragen, dann [E] am Pack-Tisch.
- **10 Karten pro Pack**, zum Durchklicken (beste zuletzt): 7× Gewöhnlich, 2× Gewöhnlich/Selten (30 %),
  1× Selten oder Ultra-Selten (33 % – etwa jedes 3. Pack).
- **Ultra-Seltene sind shiny:** wandernder Regenbogen-Schimmer und Funkeln, eigener Sound.
- **30 Monstera-Karten:** 20 Gewöhnlich (Wert 0,50 €), 7 Selten (3,00 €), 3 Ultra-Selten (25,00 €).
  Namen sind Platzhalter. Eigene Kartenbilder (24×32 Pixel): `Assets/Art/Cards/Resources/Cards/monstera_01.png` …
- **Sammelbuch** mit [C]: alle entdeckten Karten, Anzahl im Besitz, Details per Klick.
- **Vitrine** (80 € am Computer, mit [B] aufstellen): bis zu 4 Karten mit eigenem Preis (10–300 % vom Wert).
  30 % der Kunden sind Sammler und kaufen daraus, wenn eine Vitrine gefüllt ist.
- Pack-Öffnen ist ab Tag 1 verfügbar (wird später an die Story angepasst).

## Kunden-Laune und Schwierigkeit

- In der Schlange zeigt jeder Kunde seine Laune: 🙂 → 😐 → 😠, jeweils ein Drittel der Geduld.
  Nach 😠 geht er. Wer vorher bedient wird, zahlt den vollen Preis.
- Geduld wächst mit dem Ruf: 15 s (Unbekannt) bis 35 s (Legendär). Jeder Fleck im Laden kostet 8 % Geduld (max. −50 %).
- Startgeld 60 €, Display 36 € (3,00 € pro Pack), jeden Tag 1–3 neue Flecken, Kunden nehmen meist 1–2 Packs.

## Seltenheiten

Drei Stufen: **Gewöhnlich**, **Selten**, **Ultra-Selten** (shiny).

## Story

- **Hauptfigur:** Der Spieler wählt beim Spielstart aus 2–4 fertigen Figuren und gibt einen Namen ein.
- **Der Name „TwoMoons“:** Es gibt eine sagenumwobene Karte „Two Moons“. Gerüchte über
  sie ziehen sich durch die Story. Gefunden wird sie im Finale (Kapitel 7) beim großen Turnier.
- **Aufbau:** Kapitel mit Aufgaben und Belohnungen bilden den roten Faden. Der Ruf des
  Ladens steigt mit zufriedenen Kunden und zeigt den Fortschritt.
- **Anfang (Intro-Geschichte, nur zum Durchklicken, [Esc] überspringt):**
  1. *Sternennacht* – Ein Kind liegt nachts wach im Bett, vor dem Fenster gehen ein roter und ein goldener
     Mond auf. In einer Traumblase: ein eigener Laden voller Sammelkarten.
  2. *Papa kommt herein* – Die Tür geht auf, warmes Licht fällt ins Zimmer, Papa setzt sich ans Bett.
  3. *Die Legende* – Papa erzählt von der Karte „Two Moons“, die nur gedruckt wurde, wenn beide Monde am
     Himmel standen. Sie findet ihren Weg zu dem, der seinen Traum nie aufgibt. Licht aus.
  4. *Jahre vergehen* – Monde ziehen über den Himmel, das Kind wächst heran.
  5. *18. Geburtstag* – Kuchen mit flackernden Kerzen. Papa schenkt einen alten Schlüssel mit
     Zwei-Monde-Anhänger. Er sieht müde aus, auf dem Küchenschrank stehen viele Tablettendosen.
  6. *Die Ladenfront* – Ein verbretterter alter Laden: früher Papas Laden. Papa ist krank und muss kürzertreten –
     er möchte nur noch erleben, wie dort wieder Licht brennt. „Ab heute gehört er dir.“
  7. *Der leere Laden* – Staub, Spinnweben, ein schiefes Regal und ein Karton Monstera-Packs.
     „Er ist perfekt. Ich nenne ihn … TwoMoons.“ Dann beginnt Kapitel 1.
  Eigene Szenenbilder (160×90 Pixel) können als `Assets/Art/Intro/Resources/Intro/<name>.png` die Platzhalter ersetzen
  (bedroom, nightSky, kitchen, shopFront, shopEmpty, father, door, thought, cake, key, moonRed, moonGold).
- **Ton:** leise und hoffnungsvoll – ein berührender Moment, aber das Ende bleibt warm und motivierend.
- **Roter Faden: Papa** – Papas Krankheit begleitet die Story leise weiter, z. B. durch Besuche im Laden,
  Briefe oder alte Fotos und Notizen aus seiner Ladenzeit, die man in späteren Kapiteln findet.
  Im Finale schließt sich der Kreis: Papa erlebt den vollen Laden beim großen Turnier – und die Two-Moons-Karte.
- **Story-Rhythmus:**
  - Jedes Kapitel beginnt mit derselben Titelkarte: Nachthimmel, die zwei Monde schweben herein, „Kapitel X“ + Name.
  - Mitten im Kapitel gibt es kleine Papa-Momente (roter Faden).
  - Nach jedem geschafften Kapitel kommt eine kurze Story-Szene mit Papa, dann der Level-Aufstieg.
  - Kapitel 1: *„Der erste Abend“* (nach Tag 1: Papa ruft an und hustet länger als sonst) und
    *„Licht im Fenster“* (Kapitelende: Papa sieht von der anderen Straßenseite, dass im Laden wieder Licht brennt,
    und schreibt: „Ich hab das Licht gesehen. Danke.“).
  - Jede Szene kommt pro Spielstand nur einmal; [Esc] überspringt.
- **Ziel/Ende:** TwoMoons wird zum besten Laden der Stadt – Höhepunkt ist ein großes
  Turnier/Event, das der Laden ausrichtet.

## Kartenspiele

Drei erfundene Spiele, jeweils mit eigenem Stil:

1. **Monstera** – Monster sammeln: süße und coole Kreaturen
2. **Spellforge** – Fantasy & Magie: Zauberer, Drachen, Ritter
3. **Soul Clash** – Anime & Duelle: Duellanten, Krieger, spektakuläre Monster

## Story-Kapitel

Jedes Kapitel hat Aufgaben, schaltet neue Funktionen frei und endet mit einer kleinen Szene.

| # | Kapitel | Aufgaben (Beispiele) | Schaltet frei |
|---|---|---|---|
| 1 | **Der Traum beginnt** | Laden putzen, erstes Regal aufstellen, Monstera-Packs einräumen, erste 5 Kunden bedienen | Grundverkauf, Kasse, Tagesablauf |
| 2 | **Die ersten Stammkunden** | Preise anpassen, 3 Tage mit Gewinn abschließen, Zubehör ins Sortiment nehmen | Spellforge, Zubehör, Preisgestaltung |
| 3 | **Schätze aus dem Pack** | 10 Packs selbst öffnen, erste Ultra-Seltene finden, Vitrine aufstellen | Pack-Öffnen, Sammelbuch, Vitrine & Einzelkarten |
| 4 | **Handeln wie ein Profi** | Erste Sammlung eines Kunden ankaufen, Karte mit Gewinn weiterverkaufen | Ankauf & Verhandeln, Soul Clash |
| 5 | **Der erste Spieltisch** | Spieltisch kaufen, erstes kleines Turnier ausrichten | Spieltische, kleine Turniere |
| 6 | **TwoMoons wächst** | Laden erweitern, ersten Mitarbeiter einstellen, Ruf-Stufe erreichen | Ladenerweiterung, Mitarbeiter |
| 7 | **Das große Turnier** | Großes Stadtturnier vorbereiten und ausrichten | Finale: TwoMoons ist der beste Laden der Stadt |

**Ruf-Stufen:** Unbekannt → Geheimtipp → Beliebt → Stadtbekannt → Legendär.
Zufriedene Kunden, faire Preise und gelungene Turniere erhöhen den Ruf. Manche
Kapitel brauchen eine bestimmte Ruf-Stufe, bevor sie beginnen.

Nach dem Finale kann man weiterspielen.

## Sound

- Stil: 8-Bit / Chiptune
- Soundeffekte: Kasse, Türglocke, Karton aufheben/abstellen, Einräumen, Putzen, Bestellen, Aufstellen,
  Fehler, Aufgabe erledigt, Kapitel-Fanfare, Ladenschluss
- Menü-Klänge: Knöpfe und Dialoge
- Hintergrundmusik: gemütliche Chiptune-Schleife
- Lautstärke: Musik und Effekte getrennt einstellbar (Menü über Esc), wird gespeichert
- Zuerst erzeugte Platzhalter (WAV in `Assets/Art/Audio`), eigene Sounds mit gleichem Dateinamen ersetzen

## Startmenü und Speichern

- **Hintergrund:** der Laden leicht abgedunkelt, Kunden laufen herum
- **Logo:** Pixel-Version des TwoMoons-Logos im Startmenü (`Assets/Art/UI/Resources/logo.png`, wird automatisch gefunden);
  Original-Logo zusätzlich möglich, Pixel-Version auch im Spiel (z. B. Ladenschild)
- **Menüpunkte:** Fortsetzen (nur wenn ein Spielstand existiert) · Neues Spiel · Laden · Einstellungen · Credits · Beenden
- **Speichern:** automatisch jeden Morgen nach dem Schlafen + jederzeit manuell über das Esc-Menü.
  Spielstände liegen als JSON in `Application.persistentDataPath` (`spielstand_1.json` …).
  Gespeichert werden auch Tagesphase, Uhrzeit, Aufgaben und Tageszahlen; nach dem Laden geht es genau dort weiter.
  Kunden, die gerade im Laden waren, werden nicht gespeichert.
- **Esc-Menü im Spiel:** Weiter · Speichern · Einstellungen · Zum Hauptmenü · Spiel beenden (mit Nachfrage, ob gespeichert werden soll)
- **Neues Spiel:** Speicherplatz wählen (belegt → Nachfrage „Überschreiben?“), dann Name und Figur
- **Laden:** Spielstand wählen oder mit Nachfrage löschen
- **3 Speicherplätze**, jeder zeigt Name und Figur, Tag und Kapitel, Datum der letzten Speicherung
- **Credits:** vorerst Platzhalter „TwoMoons – ein Spiel von …“
- **Farben:** alle Menüs in den Logo-Farben – Dunkelbraun als Hintergrund, Rot-Orange und Gold für Knöpfe und Akzente

## Lieferungen

- **Ware** (Displays) kommt als Karton an den Eingang und wird getragen und eingeräumt.
- **Möbel und Deko** kommen gesammelt in **einem Paket** an den Eingang. [E] am Paket packt alles aus und
  startet direkt den Bau-Modus – nichts muss getragen werden. [B] wechselt zwischen den Teilen.
- Außen an der Ladenfront hängt ein großes **TwoMoons-Schild** mit dem Pixel-Logo.

## Grafik-Vorgaben für eigene Bilder

- Raster: 16×16 Pixel pro Bodenfeld (1 Unity-Einheit = 16 Pixel)
- Import: Filter Mode „Point“, keine Kompression, Pixels Per Unit = 16
- Platzhalter haben dieselbe Größe wie die späteren Bilder, damit sie direkt ausgetauscht werden können

## Erster Prototyp: Kapitel 1

- Laden im Breitformat (20×8 Felder) mit Eingang, Theke, Kasse und Computer – füllt den Bildschirm
- Figurenauswahl (3 Figuren) und Namenseingabe beim Start
- Ware wird am Computer hinter der Theke bestellt und kommt sofort als Karton an den Eingang
- Kassieren: zur Kasse gehen und mit [E] jeden Kunden selbst bedienen
- Tagesablauf: Vorbereitung → Laden öffnen (Schild an der Tür) → 9–18 Uhr (Level 1–2: 5 Minuten, ab Level 3: 7 Minuten Echtzeit) → Abrechnung → Schlafen

## Offen


## Prototyp starten (Kapitel 1)

1. Projekt in Unity 6 öffnen und warten, bis die Skripte kompiliert sind.
2. Im Menü oben **TwoMoons → Aktualisieren** wählen. Das legt an:
   - Platzhalter-Bilder in `Assets/Art/Sprites` (16×16 PNG)
   - Platzhalter-Sounds in `Assets/Art/Audio` (WAV)
   - `Assets/Art/SpriteLibrary.asset` und `Assets/Art/AudioLibrary.asset` (verknüpfen alle Bilder und Sounds)
   - die Szene `Assets/Scenes/Kapitel1.unity`
3. Auf **Play** drücken.

**Steuerung:** WASD/Pfeiltasten laufen · E benutzen · Q Karton abstellen · B Bau-Modus
(Linksklick aufstellen, Rechtsklick abbrechen) · Esc öffnet die Einstellungen bzw. schließt Menüs
· Tab klappt die Aufgabenliste auf/zu · C Sammelbuch · F11 Vollbild (im fertigen Spiel). Controller geht auch.

**Eigene Bilder:** Die PNGs in `Assets/Art/Sprites` mit gleichem Dateinamen durch eigene
16×16-Pixelbilder ersetzen. Neue Bilder in `Assets/Art` bekommen automatisch die richtigen
Pixel-Art-Einstellungen. Eigene Bilder werden beim erneuten Einrichten nicht überschrieben.

**Web-Version:** Menü **TwoMoons → Web-Build erstellen** baut das Spiel für den Browser nach `Builds/Web`
und legt `Builds/TwoMoons-Web.zip` zum Hochladen auf itch.io an (als HTML-Projekt, „played in the browser“).
Voraussetzung: „Web Build Support“ im Unity Hub installiert. Im Browser werden Spielstände über PlayerPrefs gespeichert.

**Code:** `Assets/Scripts` – der Laden wird komplett aus Code aufgebaut (`GameBootstrap`),
das Raster steht in `ShopGrid`, Preise und Bestellungen in `Catalog`.
