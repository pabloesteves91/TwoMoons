using UnityEngine;

namespace TwoMoons
{
    /// <summary>
    /// Startpunkt von Kapitel 1: baut den Laden aus Code auf.
    /// In der Szene braucht es nur dieses Objekt (und eine Kamera).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] SpriteLibrary sprites = null;
        [SerializeField] AudioLibrary sounds = null;

        static readonly Vector2Int[] StartDirt =
            { new Vector2Int(2, 3), new Vector2Int(7, 4), new Vector2Int(17, 5), new Vector2Int(12, 8), new Vector2Int(19, 7) };

        void Awake()
        {
            Art.Init(sprites);
            SoundManager.Init(sounds);
            SetUpCamera();

            var game = new GameObject("GameManager").AddComponent<GameManager>();
            game.Grid = new GameObject("ShopGrid").AddComponent<ShopGrid>();
            game.Grid.Init();

            BuildShop(game);

            game.Player = WorldFactory.SpawnPlayer(ShopGrid.PlayerStartCell);
            game.ShelvesToPlace = 1;
            WorldFactory.SpawnBox(ShopGrid.DeliveryCells[0], Catalog.MonsteraPack, 12);
            foreach (var cell in StartDirt) WorldFactory.SpawnDirt(cell);

            game.gameObject.AddComponent<SoundManager>();
            game.gameObject.AddComponent<CustomerSpawner>();
            game.gameObject.AddComponent<BuildController>();
            game.gameObject.AddComponent<ChapterOne>();
            game.gameObject.AddComponent<GameHUD>();
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
            camera.backgroundColor = new Color(0.12f, 0.1f, 0.18f);
            if (camera.GetComponent<CameraFit>() == null) camera.gameObject.AddComponent<CameraFit>();
        }

        static void BuildShop(GameManager game)
        {
            var root = new GameObject("Laden").transform;

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
        }
    }
}
