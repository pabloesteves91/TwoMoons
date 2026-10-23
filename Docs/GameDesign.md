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

- **Hauptfigur:** Den Namen wählt der Spieler beim Spielstart.
- **Aufbau:** Kapitel mit Aufgaben und Belohnungen bilden den roten Faden. Der Ruf des
  Ladens steigt mit zufriedenen Kunden und zeigt den Fortschritt.
- **Anfang:** Die Hauptfigur erfüllt sich einen Kindheitstraum: Sie kündigt ihren Job
  und eröffnet mit wenig Geld ihren eigenen TCG-Laden TwoMoons.
- **Ziel/Ende:** TwoMoons wird zum besten Laden der Stadt – Höhepunkt ist ein großes
  Turnier/Event, das der Laden ausrichtet.

## Kartenspiele

Drei erfundene Spiele, jeweils mit eigenem Stil:

1. **Monster sammeln** – süße und coole Kreaturen zum Sammeln
2. **Fantasy & Magie** – Zauberer, Drachen, Ritter
3. **Anime & Duelle** – Duellanten, Krieger, spektakuläre Monster

## Grafik-Vorgaben für eigene Bilder

- Raster: 16×16 Pixel pro Bodenfeld (1 Unity-Einheit = 16 Pixel)
- Import: Filter Mode „Point“, keine Kompression, Pixels Per Unit = 16
- Platzhalter haben dieselbe Größe wie die späteren Bilder, damit sie direkt ausgetauscht werden können

## Erster Prototyp

- Kleiner Laden mit Platzhaltergrafik
- Spielerfigur, die sich bewegt und mit Objekten interagiert
- Ein Regal zum Auffüllen
- Kunden, die Ware nehmen und an der Kasse bezahlen
- Geldanzeige

## Offen

- Aussehen der Hauptfigur
- Inhalt der einzelnen Story-Kapitel
- Namen der drei Kartenspiele
- Bedeutung des Namens „TwoMoons“ in der Story
- Seltenheitsstufen und Kartenwerte
