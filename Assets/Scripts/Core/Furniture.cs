namespace TwoMoons
{
    /// <summary>Neue Einträge immer hinten anhängen – die Nummern stehen in Spielständen.</summary>
    public enum Furniture { Shelf, Showcase, Plant, Poster, Rug, WallClock, FairyLights, AccessoryStand, GameTable, PrizeCounter, TournamentBoard,
        Fridge, Couch, Tv, Console, Printer3D, PrintShelf, NeonSign, GradingStation, Lounge }

    /// <summary>Wo ein Möbelstück hinkommt.</summary>
    public enum Placement
    {
        /// <summary>Steht im Raum und blockiert das Feld (Regal, Pflanze …).</summary>
        Floor,
        /// <summary>Hängt an der hinteren Wand.</summary>
        Wall,
        /// <summary>Liegt auf dem Boden, man kann darüber laufen.</summary>
        Rug,
    }

    public class FurnitureDef
    {
        public Furniture Id;
        public string Name;
        public string SpriteKey;
        public Placement Placement;
        /// <summary>Gemütlichkeit: macht Kunden geduldiger und bringt abends etwas Ruf.</summary>
        public int Cozy;
    }

    public static class FurnitureCatalog
    {
        static readonly FurnitureDef[] All =
        {
            new FurnitureDef { Id = Furniture.Shelf, Name = "Regal", SpriteKey = "shelf", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.Showcase, Name = "Vitrine", SpriteKey = "showcase", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.Plant, Name = "Topfpflanze", SpriteKey = "plant", Placement = Placement.Floor, Cozy = 2 },
            new FurnitureDef { Id = Furniture.Poster, Name = "Poster", SpriteKey = "poster", Placement = Placement.Wall, Cozy = 1 },
            new FurnitureDef { Id = Furniture.Rug, Name = "Teppich", SpriteKey = "rug", Placement = Placement.Rug, Cozy = 3 },
            new FurnitureDef { Id = Furniture.WallClock, Name = "Wanduhr", SpriteKey = "wallClock", Placement = Placement.Wall, Cozy = 1 },
            new FurnitureDef { Id = Furniture.FairyLights, Name = "Lichterkette", SpriteKey = "fairyLights", Placement = Placement.Wall, Cozy = 3 },
            new FurnitureDef { Id = Furniture.AccessoryStand, Name = "Zubehör-Ständer", SpriteKey = "accessoryStand", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.GameTable, Name = "Spieltisch", SpriteKey = "gameTable", Placement = Placement.Floor, Cozy = 1 },
            new FurnitureDef { Id = Furniture.PrizeCounter, Name = "Turnier-Counter", SpriteKey = "prizeCounter", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.TournamentBoard, Name = "Turnier-Tafel", SpriteKey = "tournamentBoard", Placement = Placement.Wall, Cozy = 1 },
            new FurnitureDef { Id = Furniture.Fridge, Name = "Getränkekühlschrank", SpriteKey = "fridge", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.Couch, Name = "Couch", SpriteKey = "couch", Placement = Placement.Floor, Cozy = 3 },
            new FurnitureDef { Id = Furniture.Tv, Name = "Fernseher", SpriteKey = "tv", Placement = Placement.Wall, Cozy = 1 },
            new FurnitureDef { Id = Furniture.Console, Name = "Spielkonsole", SpriteKey = "console", Placement = Placement.Floor, Cozy = 1 },
            new FurnitureDef { Id = Furniture.Printer3D, Name = "3D-Drucker", SpriteKey = "printer3d", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.PrintShelf, Name = "Druck-Regal", SpriteKey = "printShelf", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.NeonSign, Name = "Neon-Schild", SpriteKey = "neonSign", Placement = Placement.Wall, Cozy = 5 },
            new FurnitureDef { Id = Furniture.GradingStation, Name = "Grading-Station", SpriteKey = "gradingStation", Placement = Placement.Floor },
            new FurnitureDef { Id = Furniture.Lounge, Name = "Sitzsack", SpriteKey = "beanBag", Placement = Placement.Floor, Cozy = 3 },
        };

        public static FurnitureDef Get(Furniture id) => All[(int)id];
    }
}
