# TwoMoons – Game Design

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

1. **Einkaufen** – Boosterpacks und Displays beim Großhändler bestellen.
2. **Einräumen** – Mit der Figur Ware ins Regal stellen.
3. **Preise** – Preise festlegen: zu teuer → Kunden gehen, zu billig → kein Gewinn.
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

## Seltenheiten

Drei Stufen: **Gewöhnlich**, **Selten**, **Ultra-Selten**.

## Story

- **Hauptfigur:** Der Spieler wählt beim Spielstart aus 2–4 fertigen Figuren und gibt einen Namen ein.
- **Der Name „TwoMoons“:** Es gibt eine sagenumwobene Karte „Two Moons“. Gerüchte über
  sie ziehen sich durch die Story. Gefunden wird sie im Finale (Kapitel 7) beim großen Turnier.
- **Aufbau:** Kapitel mit Aufgaben und Belohnungen bilden den roten Faden. Der Ruf des
  Ladens steigt mit zufriedenen Kunden und zeigt den Fortschritt.
- **Anfang:** Die Hauptfigur erfüllt sich einen Kindheitstraum: Sie kündigt ihren Job
  und eröffnet mit wenig Geld ihren eigenen TCG-Laden TwoMoons.
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

## Grafik-Vorgaben für eigene Bilder

- Raster: 16×16 Pixel pro Bodenfeld (1 Unity-Einheit = 16 Pixel)
- Import: Filter Mode „Point“, keine Kompression, Pixels Per Unit = 16
- Platzhalter haben dieselbe Größe wie die späteren Bilder, damit sie direkt ausgetauscht werden können

## Erster Prototyp: Kapitel 1

- Winziger Laden (10×8 Felder) mit Eingang, Theke, Kasse und Computer
- Figurenauswahl (3 Figuren) und Namenseingabe beim Start
- Ware wird am Computer hinter der Theke bestellt und kommt am nächsten Morgen als Karton
- Kassieren: zur Kasse gehen und mit [E] jeden Kunden selbst bedienen
- Tagesablauf: Vorbereitung → Laden öffnen (Schild an der Tür) → 9–18 Uhr (10 Minuten Echtzeit) → Abrechnung → Schlafen

## Offen

- Kartenwerte und Pack-Wahrscheinlichkeiten je Seltenheit

## Prototyp starten (Kapitel 1)

1. Projekt in Unity 6 öffnen und warten, bis die Skripte kompiliert sind.
2. Im Menü oben **TwoMoons → Kapitel 1 einrichten** wählen. Das legt an:
   - Platzhalter-Bilder in `Assets/Art/Sprites` (16×16 PNG)
   - `Assets/Art/SpriteLibrary.asset` (verknüpft alle Bilder)
   - die Szene `Assets/Scenes/Kapitel1.unity`
3. Auf **Play** drücken.

**Steuerung:** WASD/Pfeiltasten laufen · E benutzen · Q Karton abstellen · B Bau-Modus
(Linksklick aufstellen, Rechtsklick abbrechen) · Esc schließt Menüs. Controller geht auch.

**Eigene Bilder:** Die PNGs in `Assets/Art/Sprites` mit gleichem Dateinamen durch eigene
16×16-Pixelbilder ersetzen. Neue Bilder in `Assets/Art` bekommen automatisch die richtigen
Pixel-Art-Einstellungen. Eigene Bilder werden beim erneuten Einrichten nicht überschrieben.

**Code:** `Assets/Scripts` – der Laden wird komplett aus Code aufgebaut (`GameBootstrap`),
das Raster steht in `ShopGrid`, Preise und Bestellungen in `Catalog`.
