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

## Story

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

- Name und Aussehen der Hauptfigur
- Namen der drei Kartenspiele
- Bedeutung des Namens „TwoMoons“ in der Story
- Seltenheitsstufen und Kartenwerte
