# Eigene Bilder für TwoMoons

## So geht's

1. In Unity einmal **TwoMoons → Bilder und Sounds aktualisieren** klicken.
   Danach liegt für jedes Bild ein Platzhalter in `Assets/Art/Sprites/` (z. B. `shelf.png`).
2. Bild malen (z. B. coddy.tech Pixel-Editor, **16×16**), als **PNG mit transparentem Hintergrund** exportieren.
   Die Export-Größe ist egal (16×16 oder 256×256) – das Spiel macht jedes Bild genau ein Feld groß.
3. Die Datei **genau so nennen wie den Platzhalter** und diesen damit ersetzen (im Finder/Explorer überschreiben).
4. Zurück in Unity: das Bild wird automatisch übernommen. Play drücken.

Tipp: Unten im Bild „stehen“ – Figuren und Möbel sollten die unterste Pixelreihe berühren, sonst schweben sie.

## Bildernamen

| Datei | Was | Hinweis |
|---|---|---|
| floor.png / floor2.png | Boden / Boden im zweiten Raum | Kachel, nahtlos |
| wall.png, street.png, door.png | Wand, Straße, Tür | Kachel |
| sign.png, signBoard.png | Türschild, Hintergrund vom Ladenschild außen | |
| counter.png, register.png, computer.png, packTable.png | Theke, Kasse, Computer, Pack-Tisch | |
| shelf.png, accessoryStand.png, fridge.png, printShelf.png | Regale | |
| showcase.png, premiumShowcase.png | Vitrinen | |
| box.png, package.png, dirt.png | Karton, Möbel-Paket, Fleck | |
| plant.png, poster.png, rug.png, wallClock.png, fairyLights.png, neonSign.png, beanBag.png, turf.png, trophyWall.png, trophy.png, moonStatue.png | Deko | Poster/Uhr/Lichterkette/Neon/Trophäenwand hängen an der Wand |
| gameTable.png, prizeCounter.png, tournamentBoard.png | Turnier | |
| couch.png, tv.png, console.png, arcade.png | Spielecke | |
| printer3d.png, gradingStation.png | Werkstatt | |
| packMonstera.png, packSpellforge.png, packSoulClash.png | Booster | |
| sleeves.png, binder.png, playmat.png, soda.png, energyDrink.png, filament.png | Ware | |
| printSmall.png, printMedium.png, printLarge.png | 3D-Druck-Figuren | |
| cardIcon.png, slabIcon.png | Karte / bewertete Karte (klein in der Vitrine) | |
| moodHappy.png, moodNeutral.png, moodAngry.png | Kunden-Laune | |
| player1–3.png, customer1–3.png, employee.png, papa.png | Figuren | Figur schaut nach rechts |
| white.png | einfarbig weiß | nicht ändern |

## Andere Bilder

| Was | Wo | Größe |
|---|---|---|
| Logo | `Assets/Art/UI/Resources/` (Name enthält „logo“ oder „twomoons“) | beliebig |
| Kartenbilder | `Assets/Art/Cards/Resources/Cards/monstera_01.png` … `soulclash_30.png`, `two_moons.png` | 24×32 (oder ein Vielfaches) |
| Intro-Szenen | `Assets/Art/Intro/Resources/Intro/` (bedroom, kitchen, shopFront, shopEmpty, father …) | **genau** 160×90 (Hintergründe) – nicht vergrößert exportieren |
