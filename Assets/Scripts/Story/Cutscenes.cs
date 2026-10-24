using UnityEngine;

namespace TwoMoons
{
    /// <summary>Eine Szene: was gezeichnet wird und welche Sätze dazu kommen.</summary>
    public class CutsceneScene
    {
        public System.Action<CutscenePlayer, float> Draw;
        /// <summary>Sprecher (null = Erzähler) und Text. {0} wird durch den Namen ersetzt.</summary>
        public (string speaker, string text)[] Lines;
    }

    public class Cutscene
    {
        public readonly CutsceneScene[] Scenes;

        public Cutscene(CutsceneScene[] scenes) => Scenes = scenes;
    }

    /// <summary>
    /// Alle Story-Szenen: das Intro und die Papa-Momente zwischen und nach den Kapiteln (roter Faden).
    /// </summary>
    public static class Cutscenes
    {
        const string Papa = "Papa";
        const string Kid = "{0}";

        /// <summary>Kapitelnamen für die Titelkarte „Kapitel X“.</summary>
        public static readonly string[] ChapterTitles =
        {
            "", "Der Traum beginnt", "Die ersten Stammkunden", "Das erste Turnier", "Handeln & Drucken",
            "TwoMoons wächst", "Der Ruf eilt voraus", "Das große Turnier",
        };

        /// <summary>Gibt es dieses Kapitel schon im Spiel?</summary>
        public static bool ChapterExists(int chapter) => chapter >= 1 && chapter < ChapterTitles.Length;

        public static readonly Cutscene Intro = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawBedroom(t, thought: true, father: false, doorOpen: 0f, dark: 0f),
                Lines = new[]
                {
                    ((string)null, "Es ist spät in der Nacht. Vor dem Fenster gehen zwei Monde auf – ein roter und ein goldener."),
                    (null, "{0} kann nicht schlafen. Wie fast jede Nacht träumt {0} von einem eigenen Laden …"),
                    (null, "… mit Regalen voller Sammelkarten und Kunden, die strahlend mit ihren Packs nach Hause gehen."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawBedroom(t + 20f, thought: false, father: true, doorOpen: Mathf.Clamp01(t / 1.2f), dark: 0f, walkTime: t - 1f),
                Lines = new[]
                {
                    ((string)null, "Da geht leise die Tür auf."),
                    (Papa, "Na, mein Schatz? Wieder zu aufgeregt zum Schlafen?"),
                    (Kid, "Papa … erzählst du mir noch einmal die Geschichte von den zwei Monden?"),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawBedroom(t + 40f, thought: false, father: true, doorOpen: 1f, dark: c.IsLastLine ? Mathf.Clamp01(c.LineTime / 3f) * 0.6f : 0f, walkTime: 99f),
                Lines = new[]
                {
                    (Papa, "Es heißt, in den Nächten, in denen beide Monde zugleich am Himmel stehen, wurde eine ganz besondere Karte gedruckt."),
                    (Papa, "Man nennt sie „Two Moons“. Niemand weiß, wo sie heute ist."),
                    (Papa, "Aber man sagt, sie findet ihren Weg zu dem, der seinen Traum nie aufgibt."),
                    (Kid, "Ich geb meinen Traum nie auf, Papa. Versprochen."),
                    (Papa, "Das weiß ich. Und jetzt schlaf. Träume brauchen Kraft."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawYearsPass(t),
                Lines = new[]
                {
                    ((string)null, "Die Jahre vergehen. Viele Monde ziehen über den Himmel …"),
                    (null, "… aber der Traum bleibt."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawKitchen(t),
                Lines = new[]
                {
                    ((string)null, "Am Morgen des 18. Geburtstags."),
                    (Papa, "Alles Gute, {0}! Ich hab da etwas für dich."),
                    (null, "Ein alter Schlüssel. Am Anhänger baumeln zwei kleine Monde – ein roter und ein goldener."),
                    (null, "Papa lächelt. Aber {0} sieht, wie müde er in letzter Zeit aussieht. Und die vielen Tablettendosen auf dem Küchenschrank."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopFront(t),
                Lines = new[]
                {
                    (Papa, "Das hier war einmal mein Laden. Als du klein warst, musste ich ihn schließen."),
                    (Papa, "Ich wollte ihn immer wieder aufmachen. Aber die Ärzte sagen, ich muss jetzt kürzertreten."),
                    (Kid, "Papa …"),
                    (Papa, "Schon gut. Ich möchte nur noch erleben, wie hier drin wieder Licht brennt. Ab heute gehört er dir."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopEmpty(t),
                Lines = new[]
                {
                    ((string)null, "Drinnen: Staub, Spinnweben und ein schiefes, altes Regal. Sonst nichts."),
                    (Papa, "Er ist leer, ich weiß. Einen Karton Monstera-Packs habe ich dir noch besorgt. Den Rest schaffst du."),
                    (Kid, "Er ist perfekt, Papa. Und du wirst sehen, wie er sich füllt. Versprochen."),
                    (Kid, "Ich weiß auch schon, wie er heißen wird … TwoMoons."),
                },
            },
        });

        /// <summary>Kapitel 1, nach dem ersten Tag: Papa ruft an.</summary>
        public static readonly Cutscene FirstEvening = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopNight(t, phoneCall: true),
                Lines = new[]
                {
                    ((string)null, "Nach dem ersten Tag. Der Laden ist dunkel, nur die Lampe über der Kasse brennt noch."),
                    (null, "Da klingelt das Handy."),
                    (Papa, "Na? Wie war dein erster Tag?"),
                    (Kid, "Anstrengend. Aber … schön. Die Leute haben gelächelt, Papa."),
                    (Papa, "Genau so hat es bei mir damals auch angefangen."),
                    (null, "Papa hustet. Länger als sonst."),
                    (Kid, "Papa? Alles okay?"),
                    (Papa, "Alles gut, alles gut. Schlaf jetzt. Morgen kommen noch mehr Kunden."),
                },
            },
        });

        /// <summary>Ende von Kapitel 1: Licht im Fenster.</summary>
        public static readonly Cutscene Chapter1End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopFrontNight(t, papaClose: false),
                Lines = new[]
                {
                    ((string)null, "Am Abend, nach Ladenschluss."),
                    (null, "Zum ersten Mal seit vielen Jahren brennt im alten Laden wieder Licht."),
                    (null, "Auf der anderen Straßenseite steht jemand und schaut lange hinüber."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopFrontNight(t, papaClose: true),
                Lines = new[]
                {
                    (Papa, "Da ist es. Das Licht."),
                    ((string)null, "Papa lächelt. Dann zieht er den Mantel enger und geht langsam nach Hause."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopNight(t, phoneCall: false),
                Lines = new[]
                {
                    ((string)null, "Später summt das Handy. Eine Nachricht von Papa:"),
                    (Papa, "„Ich hab das Licht gesehen. Danke.“"),
                    (Kid, "Das war erst der Anfang, Papa."),
                },
            },
        });
    }
}
