using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>
    /// Menü „TwoMoons → Aktualisieren“: ein Knopf für alles. Legt fehlende Platzhalter-Bilder und -Sounds an,
    /// liest geänderte Bilder neu ein, aktualisiert die Bibliotheken, legt die Szene Kapitel1 an bzw. öffnet sie
    /// und verbindet alles. Eigene Bilder und Sounds werden nie überschrieben.
    /// </summary>
    public static class ChapterOneSetup
    {
        const string SpriteFolder = PixelArtImporter.ArtFolder + "Sprites";
        const string LibraryPath = PixelArtImporter.ArtFolder + "SpriteLibrary.asset";
        const string AudioFolder = PixelArtImporter.ArtFolder + "Audio";
        const string AudioLibraryPath = PixelArtImporter.ArtFolder + "AudioLibrary.asset";
        const string ScenePath = "Assets/Scenes/Kapitel1.unity";

        [MenuItem("TwoMoons/Aktualisieren", priority = 0)]
        public static void UpdateMenu() => PrepareProject(true);

        /// <summary>Bringt Bilder, Sounds und Szene auf den neuesten Stand. False, wenn abgebrochen wurde.</summary>
        public static bool PrepareProject(bool showDialog)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            AssetDatabase.Refresh();
            // Ersetzte Bilder sicher neu einlesen (Größe/Pixel pro Einheit passen sich an).
            if (AssetDatabase.IsValidFolder(SpriteFolder))
                AssetDatabase.ImportAsset(SpriteFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            int createdSprites = CreateMissingSprites();
            int createdSounds = CreateMissingSounds();
            var library = CreateOrUpdateLibrary();
            var audioLibrary = CreateOrUpdateAudioLibrary();

            // Szene anlegen, falls es sie noch nicht gibt, sonst öffnen.
            bool newScene = !File.Exists(ScenePath);
            if (newScene) CreateScene(library, audioLibrary);
            else if (EditorSceneManager.GetActiveScene().path != ScenePath) EditorSceneManager.OpenScene(ScenePath);
            LinkLibraries(library, audioLibrary);
            AddSceneToBuild();

            if (showDialog)
                EditorUtility.DisplayDialog("TwoMoons",
                    "Alles aktuell!" + (newScene ? " Die Szene Kapitel1 wurde neu angelegt." : "") + "\n\n" +
                    $"{createdSprites} neue Platzhalter-Bilder, {createdSounds} neue Sounds.\n\n" +
                    $"Eigene Bilder: PNG mit genau demselben Namen in {SpriteFolder} ersetzen (z. B. shelf.png), " +
                    "dann nochmal „TwoMoons → Aktualisieren“ und Play.",
                    "OK");
            return true;
        }

        static void AddSceneToBuild()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        /// <summary>Trägt die Bibliotheken bei jedem TwoMoons-Objekt der offenen Szene ein und speichert sie.</summary>
        static int LinkLibraries(SpriteLibrary library, AudioLibrary audioLibrary)
        {
            var bootstraps = Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None);
            foreach (var bootstrap in bootstraps)
            {
                var serialized = new SerializedObject(bootstrap);
                serialized.FindProperty("sprites").objectReferenceValue = library;
                serialized.FindProperty("sounds").objectReferenceValue = audioLibrary;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
            }
            if (bootstraps.Length > 0) EditorSceneManager.SaveOpenScenes();
            return bootstraps.Length;
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

        /// <summary>Schreibt nur Sounds, die es noch nicht gibt – eigene Sounds werden nie überschrieben.</summary>
        static int CreateMissingSounds()
        {
            Directory.CreateDirectory(AudioFolder);
            int created = 0;
            foreach (var key in AudioLibrary.Keys)
            {
                string path = $"{AudioFolder}/{key}.wav";
                if (File.Exists(path)) continue;
                WriteWav(path, PlaceholderSounds.CreateSamples(key), PlaceholderSounds.SampleRate);
                created++;
            }
            AssetDatabase.Refresh();
            return created;
        }

        /// <summary>16-Bit-Mono-WAV.</summary>
        static void WriteWav(string path, float[] samples, int sampleRate)
        {
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                int dataSize = samples.Length * 2;
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);
                foreach (float sample in samples)
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }
        }

        static AudioLibrary CreateOrUpdateAudioLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AudioLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, AudioLibraryPath);
            }

            foreach (var key in AudioLibrary.Keys)
            {
                var field = typeof(AudioLibrary).GetField(key);
                if (field.GetValue(library) as AudioClip != null) continue;
                field.SetValue(library, AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/{key}.wav"));
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
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

            // Ein hochkantiges Bild im Logo-Feld ist keins (z. B. versehentlich eine Karte) – dann logo.png eintragen.
            if (library.logo == null || !UiKit.IsLogoShaped(library.logo.rect.width, library.logo.rect.height))
                library.logo = AssetDatabase.LoadAssetAtPath<Sprite>(PixelArtImporter.UiFolder + "Resources/logo.png");

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        static void CreateScene(SpriteLibrary library, AudioLibrary audioLibrary)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();

            var bootstrap = new GameObject("TwoMoons").AddComponent<GameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("sprites").objectReferenceValue = library;
            serialized.FindProperty("sounds").objectReferenceValue = audioLibrary;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
