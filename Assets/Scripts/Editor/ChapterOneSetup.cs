using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>Menü „TwoMoons → Kapitel 1 einrichten“: legt Platzhalter-Bilder, SpriteLibrary und Szene an.</summary>
    public static class ChapterOneSetup
    {
        const string SpriteFolder = PixelArtImporter.ArtFolder + "Sprites";
        const string LibraryPath = PixelArtImporter.ArtFolder + "SpriteLibrary.asset";
        const string ScenePath = "Assets/Scenes/Kapitel1.unity";

        [MenuItem("TwoMoons/Kapitel 1 einrichten")]
        public static void Setup()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("TwoMoons", "Die Szene Kapitel1 gibt es schon. Neu anlegen?", "Ja", "Abbrechen"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            int created = CreateMissingSprites();
            var library = CreateOrUpdateLibrary();
            CreateScene(library);

            EditorUtility.DisplayDialog("TwoMoons",
                $"Kapitel 1 ist eingerichtet ({created} neue Platzhalter-Bilder).\n\n" +
                "Drücke jetzt auf Play.\n\n" +
                $"Eigene 16×16-Pixelbilder: einfach die PNGs in {SpriteFolder} mit demselben Namen ersetzen.",
                "OK");
        }

        /// <summary>Schreibt nur Bilder, die es noch nicht gibt – eigene Bilder werden nie überschrieben.</summary>
        static int CreateMissingSprites()
        {
            Directory.CreateDirectory(SpriteFolder);
            int created = 0;
            foreach (var key in SpriteLibrary.Keys)
            {
                string path = $"{SpriteFolder}/{key}.png";
                if (File.Exists(path)) continue;
                var texture = PlaceholderSprites.CreateTexture(key);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                created++;
            }
            AssetDatabase.Refresh();
            return created;
        }

        static SpriteLibrary CreateOrUpdateLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<SpriteLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SpriteLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            foreach (var key in SpriteLibrary.Keys)
            {
                var field = typeof(SpriteLibrary).GetField(key);
                if (field.GetValue(library) as Sprite != null) continue;
                field.SetValue(library, AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/{key}.png"));
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        static void CreateScene(SpriteLibrary library)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();

            var bootstrap = new GameObject("TwoMoons").AddComponent<GameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("sprites").objectReferenceValue = library;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
