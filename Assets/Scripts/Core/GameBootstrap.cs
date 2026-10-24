using System.Collections;
using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Startpunkt des Spiels: baut den Laden aus Code auf – fürs Hauptmenü, ein neues Spiel oder einen Spielstand.
    /// In der Szene braucht es nur dieses Objekt (und eine Kamera).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] SpriteLibrary sprites = null;
        [SerializeField] AudioLibrary sounds = null;

        static readonly Vector2Int[] StartDirt =
            { new Vector2Int(2, 3), new Vector2Int(7, 4), new Vector2Int(17, 5), new Vector2Int(12, 8), new Vector2Int(19, 7) };

        /// <summary>Volle Regale für den Laden im Hintergrund des Hauptmenüs.</summary>
        static readonly Vector2Int[] DemoShelves =
            { new Vector2Int(8, 6), new Vector2Int(12, 6), new Vector2Int(16, 6), new Vector2Int(10, 3), new Vector2Int(18, 3) };

        static GameBootstrap instance;
        GameObject world;

        void Awake()
        {
            instance = this;
            Art.Init(sprites);
            SoundManager.Init(sounds);
            SetUpCamera();
            gameObject.AddComponent<SoundManager>();
            Build(game => game.InitMainMenu());
        }

        public static void ShowMainMenu() => instance.StartCoroutine(instance.Rebuild(game => game.InitMainMenu()));

        public static void NewGame(int slot) => instance.StartCoroutine(instance.Rebuild(game => game.InitNewGame(slot)));

        public static void LoadGame(int slot)
        {
            var data = SaveSystem.Load(slot);
            if (data == null) return;
            instance.StartCoroutine(instance.Rebuild(game => game.InitFromSave(slot, data)));
        }

        /// <summary>Alte Welt abbauen, einen Frame warten (damit alles wirklich weg ist), neue aufbauen.</summary>
        IEnumerator Rebuild(System.Action<GameManager> init)
        {
            Time.timeScale = 1f;
            if (world != null) Destroy(world);
            yield return null;
            Build(init);
        }

        void Build(System.Action<GameManager> init)
        {
            world = new GameObject("Welt");
            WorldFactory.Root = world.transform;

            var game = new GameObject("GameManager").AddComponent<GameManager>();
            game.transform.SetParent(world.transform);
            game.Grid = new GameObject("ShopGrid").AddComponent<ShopGrid>();
            game.Grid.transform.SetParent(world.transform);
            game.Grid.Init();

            BuildShop(game);
            game.Player = WorldFactory.SpawnPlayer(ShopGrid.PlayerStartCell);

            game.gameObject.AddComponent<CustomerSpawner>();
            game.gameObject.AddComponent<BuildController>();
            game.gameObject.AddComponent<Chapters>();
            game.gameObject.AddComponent<GameHUD>();
            game.gameObject.AddComponent<MainMenuUI>();
            game.gameObject.AddComponent<CardScreensUI>();
            game.gameObject.AddComponent<CutscenePlayer>();

            init(game);
        }

        /// <summary>Startzustand von Kapitel 1: ein Regal zum Aufstellen, ein Karton, fünf Flecken.</summary>
        public static void SetUpNewGameWorld(GameManager game)
        {
            game.AddToPlace(Furniture.Shelf);
            WorldFactory.SpawnBox(ShopGrid.DeliveryCells[0], Catalog.MonsteraPack, 12);
            foreach (var cell in StartDirt) WorldFactory.SpawnDirt(cell);
        }

        public static void SetUpDemoWorld(GameManager game)
        {
            foreach (var cell in DemoShelves)
                WorldFactory.SpawnShelf(cell).SetStock(Catalog.MonsteraPack, Shelf.Capacity);
            game.Player.transform.position = ShopGrid.CellToWorld(ShopGrid.StaffCells[0]);
        }

        static void SetUpCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                camera = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.08f, 0.06f);
            if (camera.GetComponent<CameraFit>() == null) camera.gameObject.AddComponent<CameraFit>();
        }

        static void BuildShop(GameManager game)
        {
            var root = new GameObject("Laden").transform;
            root.SetParent(WorldFactory.Root);

            for (int x = 0; x < ShopGrid.Width; x++)
            for (int y = 0; y < ShopGrid.Height; y++)
            {
                var cell = new Vector2Int(x, y);
                var position = ShopGrid.CellToWorld(cell);
                bool wall = x == 0 || x == ShopGrid.Width - 1 || y == 1 || y == ShopGrid.Height - 1;

                if (y == 0)
                {
                    WorldFactory.CreateSprite("Straße", "street", position, WorldFactory.FloorOrder, root);
                }
                else if (cell == ShopGrid.DoorCell)
                {
                    WorldFactory.CreateSprite("Tür", "door", position, WorldFactory.FloorOrder, root);
                }
                else if (cell == ShopGrid.SignCell)
                {
                    WorldFactory.CreateSolid("Ladenschild", "sign", cell, root).AddComponent<ShopSign>();
                }
                else if (wall)
                {
                    WorldFactory.CreateSolid("Wand", "wall", cell, root);
                }
                else
                {
                    WorldFactory.CreateSprite("Boden", "floor", position, WorldFactory.FloorOrder, root);
                }
            }

            // Unsichtbare Grenzen, damit die Figur nicht über die Straße davonläuft.
            for (int x = -1; x <= ShopGrid.Width; x++) WorldFactory.CreateBlocker(new Vector2Int(x, -1), root);
            WorldFactory.CreateBlocker(new Vector2Int(-1, 0), root);
            WorldFactory.CreateBlocker(new Vector2Int(ShopGrid.Width, 0), root);

            foreach (var cell in ShopGrid.CounterCells)
            {
                if (cell == ShopGrid.RegisterCell)
                    game.Register = WorldFactory.CreateSolid("Kasse", "register", cell, root).AddComponent<CashRegister>();
                else
                    WorldFactory.CreateSolid("Theke", "counter", cell, root);
            }

            WorldFactory.CreateSolid("Computer", "computer", ShopGrid.ComputerCell, root).AddComponent<Computer>();
            WorldFactory.CreateSolid("Pack-Tisch", "packTable", ShopGrid.PackTableCell, root).AddComponent<PackTable>();

            CreateStoreSign(root);
        }

        /// <summary>Großes TwoMoons-Schild außen an der Ladenfront, rechts neben der Tür.</summary>
        static void CreateStoreSign(Transform root)
        {
            // Bleibt in den beiden untersten Reihen (Straße und Außenwand), damit es nichts im Laden verdeckt.
            const float width = 3.8f;
            var center = new Vector3(ShopGrid.DoorCell.x + 4f, 1f, 0f);

            var board = WorldFactory.CreateSprite("Ladenschild außen", "signBoard", center, -140, root);
            board.transform.localScale = new Vector3(width + 0.4f, 2f, 1f);

            var logo = Art.Logo;
            if (logo == null)
            {
                var texture = UiKit.LoadLogoTexture();
                if (texture == null) return;
                logo = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
            var logoRenderer = WorldFactory.CreateSprite("TwoMoons-Logo", "signBoard", center, -139, root);
            logoRenderer.sprite = logo;
            float logoWidth = logo.bounds.size.x;
            logoRenderer.transform.localScale = Vector3.one * (width / logoWidth);
        }
    }
}
