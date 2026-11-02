#!/usr/bin/env python3
"""
Erstellt die Kartenbilder für TwoMoons automatisch mit der OpenAI-Bild-API.

GPT malt nur das Motiv (quadratisch). Rahmen in Seltenheitsfarbe, Bildfenster und das leere
Textfeld setzt dieses Skript selbst zusammen – so sehen alle Karten einheitlich aus und passen
genau zum Spiel (Name, Nummer und Holo zeichnet das Spiel darüber).

Einrichten (einmal):
    pip3 install openai pillow
    export OPENAI_API_KEY="sk-..."        # eigener API-Schlüssel von platform.openai.com

Benutzen (im Ordner TwoMoons):
    python3 Vorlagen/karten_generieren.py                  # alle fehlenden Karten
    python3 Vorlagen/karten_generieren.py --spiel monstera # nur ein Spiel
    python3 Vorlagen/karten_generieren.py --nur monstera_03 monstera_04
    python3 Vorlagen/karten_generieren.py --neu monstera_17   # vorhandenes Bild ersetzen
    python3 Vorlagen/karten_generieren.py --test            # zeigt nur, was erstellt würde

Stil-Vorbild: Eine Karte, deren Stil dir gefällt, als Vorlagen/stil_monstera.png (bzw. stil_spellforge.png …)
speichern – dann malt GPT alle Karten dieses Spiels im gleichen Stil (nur mit gpt-image-Modellen).

Vorhandene Bilder werden nie überschrieben (außer mit --neu). Kosten: ca. 4–17 US-Cent pro Bild,
je nach --qualitaet (low / medium / high).
"""
import argparse
import base64
import io
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
LIST = os.path.join(ROOT, "Vorlagen", "Kartenliste_fuer_GPT.md")
OUT = os.path.join(ROOT, "Assets", "Art", "Cards", "Resources", "Cards")

STYLES = {
    "monstera": "cute chibi monster trading card art for all ages: a round, friendly creature with big shiny eyes "
                "and a happy smile, thick dark pixel outlines, bright saturated colors, simple cel shading, "
                "lush detailed background scene (forest, flowers, sky with small clouds) that fills the whole image",
    "spellforge": "fantasy spell card art, medieval and magical, warm colors with glowing magic effects",
    "soulclash": "anime battle card art, dynamic and cool, strong colors, action pose with energy and motion effects",
    "two": "legendary one-of-a-kind trading card art, magical and festive, deep blue night sky, golden accents",
}

# Seltenheitsfarben wie im Spiel (CardDatabase.RarityColor)
FRAMES = {
    "Gewöhnlich": (217, 217, 217),
    "Selten": (89, 166, 255),
    "Ultra-Selten": (255, 204, 64),
    "Legende": (245, 200, 80),
}


def read_cards():
    """Liest die Tabellen aus Kartenliste_fuer_GPT.md."""
    cards = []
    with open(LIST, encoding="utf-8") as f:
        for line in f:
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if not cells or not cells[0].endswith(".png"):
                continue
            if len(cells) == 4:
                file, name, rarity, desc = cells
            elif len(cells) == 3:  # Legende: Datei | Name | Beschreibung
                file, name, desc = cells
                rarity = "Legende"
            else:
                continue
            cards.append({"file": file, "key": file[:-4], "name": name, "rarity": rarity, "desc": desc})
    return cards


def prompt_for(card):
    style = STYLES[card["key"].split("_")[0]]
    extra = ""
    if card["rarity"] in ("Ultra-Selten", "Legende"):
        extra = " Epic and majestic, dramatic lighting, glowing aura."
    elif card["rarity"] == "Selten":
        extra = " Slightly more impressive than a common card, some glow."
    return (
        f"Pixel art illustration (16-bit style, crisp clean pixels, no blur, no anti-aliasing), square image. "
        f"{style}.{extra} "
        f"Subject: {card['name']} – {card['desc']}. "
        f"The subject is centered and fully visible with a simple fitting background scene filling the whole image. "
        f"No card frame, no border, no text, no letters, no numbers, no logo, no watermark."
    )


# Farbstreifen über dem Textfeld (wie CardArt im Spiel): grün Monstera, lila Spellforge, rot Soul Clash.
BANDS = {"monstera": (60, 160, 80), "spellforge": (110, 70, 190), "soulclash": (200, 50, 40), "two": (245, 200, 80)}


def compose(art, rarity, game="monstera"):
    """Baut die Karte im Aufbau deiner eigenen Karten (monstera_01–03) zusammen, 768×1024."""
    from PIL import Image, ImageDraw

    w, h = 768, 1024
    card = Image.new("RGBA", (w, h))
    d = ImageDraw.Draw(card)

    def box(x0, y0, x1, y1, color):  # Anteile von Breite/Höhe, von oben gemessen
        d.rectangle([round(x0 * w), round(y0 * h), round(x1 * w) - 1, round(y1 * h) - 1], fill=color)

    frame = FRAMES.get(rarity, FRAMES["Gewöhnlich"])
    band = BANDS.get(game, BANDS["monstera"])
    box(0, 0, 1, 1, (30, 22, 18))                     # dunkler Außenrand
    box(0.021, 0.016, 0.979, 0.984, frame)            # Rahmen in Seltenheitsfarbe
    box(0.10, 0.690, 0.90, 0.696, band)               # dünner Farbstreifen pro Spiel
    box(0.10, 0.712, 0.90, 0.930, (242, 232, 208))    # Textfeld: bleibt leer, das Spiel schreibt hinein

    # Bildfenster: 10–90 % der Breite, 8,1–66,4 % der Höhe
    x0, y0, x1, y1 = round(0.10 * w), round(0.081 * h), round(0.90 * w), round(0.664 * h)
    bw, bh = x1 - x0, y1 - y0
    art = art.convert("RGBA")
    # mittig auf das Seitenverhältnis des Fensters zuschneiden
    aw, ah = art.size
    target = bw / bh
    if aw / ah > target:
        nw = int(ah * target)
        art = art.crop(((aw - nw) // 2, 0, (aw - nw) // 2 + nw, ah))
    else:
        nh = int(aw / target)
        art = art.crop((0, (ah - nh) // 2, aw, (ah - nh) // 2 + nh))
    card.paste(art.resize((bw, bh), Image.LANCZOS), (x0, y0))
    return card


# Falls die Modellliste nichts Brauchbares liefert, werden diese der Reihe nach probiert.
FALLBACK_MODELS = ["gpt-image-2", "gpt-image-1.5", "chatgpt-image-latest", "gpt-image-1", "gpt-image-1-mini", "dall-e-3"]


def candidate_models(client, wanted):
    """Bildmodelle zum Ausprobieren: gewünschtes, sonst die vom Konto angebotenen, dann bekannte Namen."""
    if wanted:
        return [wanted]
    listed = []
    try:
        ids = sorted((m.id for m in client.models.list()), reverse=True)
        listed = [i for i in ids if ("image" in i or i.startswith("dall-e")) and "mini" not in i]
        listed += [i for i in ids if ("image" in i or i.startswith("dall-e")) and "mini" in i]
        print("Bildmodelle deines Kontos: " + (", ".join(listed) if listed else "keine gefunden"))
    except Exception as e:
        print(f"Modellliste nicht abrufbar ({e}).")
    return list(dict.fromkeys(listed + FALLBACK_MODELS))


def model_missing(error):
    text = str(error).lower()
    return "does not exist" in text or "model_not_found" in text or "invalid model" in text or "unknown model" in text


def style_image(card):
    """Stil-Vorbild: Vorlagen/stil_<spiel>.png, sonst die erste fertige Karte des Spiels."""
    game = card['key'].split('_')[0]
    path = os.path.join(ROOT, "Vorlagen", f"stil_{game}.png")
    if os.path.exists(path):
        return path
    # Sonst die erste schon fertige Karte dieses Spiels (z. B. deine eigene monstera_01.png).
    for number in range(1, 31):
        existing = os.path.join(OUT, f"{game}_{number:02d}.png")
        if os.path.exists(existing):
            return existing
    return None


def generate(client, card, quality, model):
    from PIL import Image
    reference = style_image(card)
    if reference and model.startswith("gpt-image"):
        # Mit Vorbild: gleicher Zeichenstil, aber neues Motiv.
        prompt = ("Use the attached image ONLY as a style reference (pixel size, outlines, colors, shading, "
                  "type of background). Do not copy its creature, card frame or layout. " + prompt_for(card))
        with open(reference, "rb") as f:
            result = client.images.edit(model=model, image=[f], prompt=prompt, size="1024x1024", quality=quality, n=1)
    elif model.startswith("dall-e"):
        result = client.images.generate(
            model=model, prompt=prompt_for(card), size="1024x1024",
            quality="hd" if quality == "high" else "standard", response_format="b64_json", n=1,
        )
    else:
        result = client.images.generate(model=model, prompt=prompt_for(card), size="1024x1024", quality=quality, n=1)
    data = base64.b64decode(result.data[0].b64_json)
    return Image.open(io.BytesIO(data))


def main():
    parser = argparse.ArgumentParser(description="TwoMoons-Kartenbilder mit GPT erstellen")
    parser.add_argument("--spiel", choices=["monstera", "spellforge", "soulclash", "two"])
    parser.add_argument("--nur", nargs="+", metavar="KARTE", help="z. B. monstera_03 monstera_04")
    parser.add_argument("--neu", nargs="+", metavar="KARTE", help="vorhandene Bilder neu erstellen")
    parser.add_argument("--qualitaet", default="medium", choices=["low", "medium", "high"])
    parser.add_argument("--test", action="store_true", help="nichts erstellen, nur anzeigen")
    parser.add_argument("--rahmen", action="store_true",
                        help="keine neuen Bilder: schon erstellte Motive (Vorlagen/Motive) nur neu einrahmen – kostet nichts")
    parser.add_argument("--modell", help="Bildmodell erzwingen, z. B. gpt-image-1 oder dall-e-3 (sonst automatisch)")
    args = parser.parse_args()

    cards = read_cards()
    if args.rahmen:
        from PIL import Image
        raw_dir = os.path.join(ROOT, "Vorlagen", "Motive")
        done = 0
        for c in cards:
            if (args.nur or args.neu) and c["key"] not in set(args.nur or []) | set(args.neu or []):
                continue
            if args.spiel and not c["key"].startswith(args.spiel):
                continue
            raw = os.path.join(raw_dir, c["file"])
            if not os.path.exists(raw):
                continue
            compose(Image.open(raw), c["rarity"], c["key"].split("_")[0]).save(os.path.join(OUT, c["file"]))
            print(f"neu eingerahmt: {c['file']}")
            done += 1
        print(f"{done} Karte(n) neu eingerahmt." if done else "Keine Motive in Vorlagen/Motive gefunden.")
        return
    wanted = set(args.nur or []) | set(args.neu or [])
    if wanted:
        cards = [c for c in cards if c["key"] in wanted]
    if args.spiel:
        cards = [c for c in cards if c["key"].startswith(args.spiel)]
    redo = set(args.neu or [])
    todo = [c for c in cards if c["key"] in redo or not os.path.exists(os.path.join(OUT, c["file"]))]

    if not todo:
        print("Nichts zu tun – alle gewählten Karten haben schon ein Bild.")
        return
    print(f"{len(todo)} Karte(n) werden erstellt:")
    for c in todo:
        print(f"  {c['file']:<24} {c['name']} ({c['rarity']})")
    if args.test:
        return

    if not os.environ.get("OPENAI_API_KEY"):
        sys.exit("Kein OPENAI_API_KEY gesetzt. Erst:  export OPENAI_API_KEY=\"sk-...\"")
    from openai import OpenAI
    client = OpenAI()
    models = candidate_models(client, args.modell)
    for game in sorted({c["key"].split("_")[0] for c in todo}):
        ref = style_image({"key": game})
        print(f"Stil-Vorbild {game}: {os.path.basename(ref) if ref else 'keins (Vorlagen/stil_' + game + '.png anlegen)'}")
    os.makedirs(OUT, exist_ok=True)

    for i, c in enumerate(todo, 1):
        print(f"[{i}/{len(todo)}] {c['name']} …", end=" ", flush=True)
        try:
            art = None
            while art is None:
                if not models:
                    sys.exit("\nKein Bildmodell funktioniert. Auf platform.openai.com unter Settings → Organization "
                             "→ Verification die Organisation verifizieren, dann nochmal versuchen.")
                try:
                    art = generate(client, c, args.qualitaet, models[0])
                except Exception as e:
                    if not model_missing(e):
                        raise
                    print(f"({models[0]} gibt es nicht, nächstes …)", end=" ", flush=True)
                    models.pop(0)
            # Rohbild zum Nachbessern aufheben (liegt außerhalb von Resources, kommt nicht ins Spiel)
            raw_dir = os.path.join(ROOT, "Vorlagen", "Motive")
            os.makedirs(raw_dir, exist_ok=True)
            art.save(os.path.join(raw_dir, c["file"]))
            compose(art, c["rarity"], c["key"].split("_")[0]).save(os.path.join(OUT, c["file"]))
            print(f"fertig ({models[0]})")
        except Exception as e:  # weiter mit der nächsten Karte
            print(f"Fehler: {e}")

    print("\nJetzt in Unity: TwoMoons → Aktualisieren, dann Play.")


if __name__ == "__main__":
    main()
