using System.Collections.Generic;

namespace TwoMoons
{
    /// <summary>Neue Fähigkeiten immer hinten anhängen – die Nummern stehen in Spielständen.</summary>
    public enum Skill { QuickHands, Charmer, StockPro, Negotiator, Organizer, Tinkerer, Tidy, Lucky }

    public class SkillDef
    {
        public Skill Id;
        public string Name;
        public string Description;
    }

    /// <summary>Fähigkeiten: mit jedem Level-Aufstieg gibt es einen Punkt (insgesamt 6 von 8 Fähigkeiten).</summary>
    public class Skills
    {
        public static readonly SkillDef[] All =
        {
            new SkillDef { Id = Skill.QuickHands, Name = "Flinke Füße", Description = "Du läufst 40 % schneller durch den Laden." },
            new SkillDef { Id = Skill.Charmer, Name = "Charmeur", Description = "Kunden warten 20 % geduldiger an der Kasse." },
            new SkillDef { Id = Skill.StockPro, Name = "Lagerprofi", Description = "Ware beim Großhändler 10 % günstiger." },
            new SkillDef { Id = Skill.Negotiator, Name = "Verhandlungsgeschick", Description = "Ankauf 15 % günstiger, Sammler zahlen bis zu 10 % mehr." },
            new SkillDef { Id = Skill.Organizer, Name = "Turnier-Organisator", Description = "Mehr Turnierspieler, der Hersteller schickt 4 Packs mehr." },
            new SkillDef { Id = Skill.Tinkerer, Name = "Tüftler", Description = "3D-Drucke und Grading 25 % schneller, halb so viele Druckfehler." },
            new SkillDef { Id = Skill.Tidy, Name = "Ordnungsfanatiker", Description = "Höchstens ein neuer Fleck am Morgen, Kunden kleckern halb so oft." },
            new SkillDef { Id = Skill.Lucky, Name = "Glückspilz", Description = "Beim Pack-Öffnen öfter Karten, die noch im Sammelbuch fehlen." },
        };

        readonly HashSet<Skill> learned = new HashSet<Skill>();

        public bool Has(Skill skill) => learned.Contains(skill);

        public int Count => learned.Count;

        /// <summary>Freie Punkte: einer pro Level-Aufstieg.</summary>
        public int FreePoints(int storeLevel) => System.Math.Max(0, storeLevel - 1 - learned.Count);

        public bool Learn(Skill skill, int storeLevel)
        {
            if (Has(skill) || FreePoints(storeLevel) <= 0) return false;
            learned.Add(skill);
            return true;
        }

        public void SaveTo(SaveData data)
        {
            data.skills.Clear();
            foreach (var skill in learned) data.skills.Add((int)skill);
        }

        public void LoadFrom(SaveData data)
        {
            learned.Clear();
            if (data.skills == null) return;
            foreach (int skill in data.skills) learned.Add((Skill)skill);
        }
    }
}
