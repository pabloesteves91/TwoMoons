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

        /// <summary>Nach dem ersten Turnier: Papa war heimlich da.</summary>
        public static readonly Cutscene FirstTournament = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopNight(t, phoneCall: false),
                Lines = new[]
                {
                    ((string)null, "Als die letzten Turnierspieler gegangen sind, liegt ein Zettel auf dem Turnier-Counter."),
                    (Papa, "„Ich stand ganz hinten und habe zugeschaut. Die Kinder haben gejubelt – genau wie früher bei mir.“"),
                    (Papa, "„Ich bin so stolz auf dich.“"),
                    (Kid, "Er war hier … und ich hab ihn nicht mal gesehen."),
                },
            },
        });

        /// <summary>Ende von Kapitel 2: ein Brief von Papa mit alten Fotos.</summary>
        public static readonly Cutscene Chapter2End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawLetter(t, moons: false),
                Lines = new[]
                {
                    ((string)null, "Am Morgen liegt ein dicker Umschlag vor der Ladentür. Papas Handschrift."),
                    (Papa, "„Ich hab beim Aufräumen alte Fotos gefunden. So sah der Laden aus, als du drei warst.“"),
                    (null, "Auf einem Foto: Papa hinter der Theke, ein kleines Kind auf dem Arm, das eine Karte hochhält."),
                    (Papa, "„Du hast damals schon jedem Kunden deine Karten gezeigt. Jetzt verkaufst du sie selbst.“"),
                    (Kid, "Das Foto kommt an die Wand. Direkt neben die Kasse."),
                },
            },
        });

        /// <summary>Ende von Kapitel 3: Papa ruft nach dem Turnier-Kapitel an.</summary>
        public static readonly Cutscene Chapter3End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopNight(t, phoneCall: true),
                Lines = new[]
                {
                    ((string)null, "Spät am Abend. Der Laden riecht nach Pizza und Kartenhüllen."),
                    (null, "Das Handy klingelt."),
                    (Papa, "Ganz TwoMoons redet von deinen Turnieren. Sogar meine Nachbarin!"),
                    (Kid, "Kommst du mal wieder vorbei, Papa? Wir haben jetzt eine Couch."),
                    (Papa, "Eine Couch, soso … Wenn die Beine mitmachen, komme ich. Versprochen."),
                    (null, "Papa hustet. Dann lacht er leise, als wäre nichts gewesen."),
                },
            },
        });

        /// <summary>Ende von Kapitel 4: Papas Notizbuch und eine Spur zur Two-Moons-Karte.</summary>
        public static readonly Cutscene Chapter4End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawLetter(t, moons: true),
                Lines = new[]
                {
                    ((string)null, "Ganz unten in einer angekauften Kartenkiste liegt ein altes Notizbuch."),
                    (null, "Es ist Papas Notizbuch. Skizzen für Figuren, Preislisten, Turnierpläne – von vor zwanzig Jahren."),
                    (null, "Auf der letzten Seite: zwei gezeichnete Monde. Darunter, in Papas Schrift:"),
                    (Papa, "„Die Karte wurde nie verkauft. Der Hersteller hebt sie auf – für den besten Laden der Stadt.“"),
                    (Kid, "Der beste Laden der Stadt … Papa, das schaffe ich."),
                },
            },
        });

        /// <summary>Ende von Kapitel 5: Papa geht es schlechter.</summary>
        public static readonly Cutscene Chapter5End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopNight(t, phoneCall: true),
                Lines = new[]
                {
                    ((string)null, "Nach Ladenschluss. Der große, neue Laden ist still, nur die Neonschrift summt."),
                    (null, "{0} ruft Papa an. Es dauert lange, bis er abnimmt."),
                    (Papa, "Hallo, mein Schatz … entschuldige. Ich war beim Arzt. Es war ein langer Tag."),
                    (Kid, "Papa, was hat er gesagt?"),
                    (Papa, "Dass ich mich schonen soll. Aber weißt du was? Ich hab Fotos von deinem vergrößerten Laden gesehen."),
                    (Papa, "Die Leute stehen bis auf die Straße. Ich bin so stolz, dass ich es kaum aushalte."),
                    (Kid, "Ruh dich aus, Papa. Und dann zeig ich dir alles. Persönlich."),
                },
            },
        });

        /// <summary>Kapitel 6: Papa hilft einen Tag lang an der Kasse.</summary>
        public static readonly Cutscene PapaHelps = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawShopBusy(t, papaAtRegister: true),
                Lines = new[]
                {
                    ((string)null, "Am Morgen steht plötzlich jemand hinter der Theke und poliert die Kasse."),
                    (Papa, "Überraschung! Heute helfe ich dir an der Kasse. So lange ich kann."),
                    (Kid, "Papa! Aber der Arzt …"),
                    (Papa, "… hat gesagt, ich soll Dinge tun, die mich glücklich machen. Also los, schließ auf!"),
                },
            },
        });

        /// <summary>Ende von Kapitel 6: die Einladung zum Stadtturnier.</summary>
        public static readonly Cutscene Chapter6End = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawLetter(t, moons: true),
                Lines = new[]
                {
                    ((string)null, "Ein Brief mit goldenem Siegel. Zwei Monde, eingeprägt ins Wachs."),
                    (null, "„Sehr geehrtes TwoMoons-Team, wir möchten, dass Ihr Laden das große Stadtturnier ausrichtet.“"),
                    (null, "„Im Prizepool: ein ganz besonderer Preis, den es nur ein einziges Mal gibt.“"),
                    (Kid, "Papa … ich glaube, die Legende ist wahr."),
                    (Papa, "Dann sitze ich in der ersten Reihe. Und wenn sie mich tragen müssen."),
                },
            },
        });

        /// <summary>Finale nach dem Stadtturnier: die Two-Moons-Karte.</summary>
        public static readonly Cutscene Finale = new Cutscene(new[]
        {
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawCityTournament(t),
                Lines = new[]
                {
                    ((string)null, "Das Stadtturnier ist vorbei. Der Laden ist so voll wie noch nie."),
                    (null, "Die Siegerin hält ihren Pokal hoch. Alle jubeln – am lautesten Papa in der ersten Reihe."),
                    (null, "Dann tritt eine Frau vom Hersteller nach vorn. Sie hält ein kleines, goldenes Kästchen in der Hand."),
                    (null, "„Diese Karte wurde nie verkauft. Sie gehört dem besten Laden der Stadt.“"),
                    (null, "„Und der heißt TwoMoons.“"),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawLegendCard(t),
                Lines = new[]
                {
                    ((string)null, "Das Kästchen öffnet sich. Zwei Monde schimmern auf der Karte – ein roter und ein goldener."),
                    (Kid, "Two Moons …"),
                    (null, "{0} dreht sich um und legt die Karte in Papas Hände."),
                    (Papa, "Sie findet ihren Weg zu dem, der seinen Traum nie aufgibt. Ich hab's dir immer gesagt."),
                },
            },
            new CutsceneScene
            {
                Draw = (c, t) => c.DrawFinaleNight(t),
                Lines = new[]
                {
                    ((string)null, "Später am Abend stehen die beiden vor dem Laden. Über der Straße gehen zwei Monde auf."),
                    (Papa, "Weißt du, was das Schönste ist? Nicht die Karte."),
                    (Papa, "Sondern dass hier drin wieder Licht brennt. Jeden Tag."),
                    (Kid, "Und es bleibt an, Papa. Versprochen."),
                    (null, "Ende. Danke fürs Spielen! TwoMoons bleibt geöffnet – spiel einfach weiter."),
                },
            },
        });

        /// <summary>Story-Moment am Ende eines Kapitels (Kapitel 7: das Finale).</summary>
        public static Cutscene ChapterEnd(int chapter)
        {
            switch (chapter)
            {
                case 1: return Chapter1End;
                case 2: return Chapter2End;
                case 3: return Chapter3End;
                case 4: return Chapter4End;
                case 5: return Chapter5End;
                case 6: return Chapter6End;
                case 7: return Finale;
                default: return null;
            }
        }

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
