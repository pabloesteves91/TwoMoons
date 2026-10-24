using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>Menü „TwoMoons → Kapitel 1 einrichten“: legt Platzhalter-Bilder und -Sounds, die Bibliotheken und die Szene an.</summary>
    public static class ChapterOneSetup
    {
        const string SpriteFolder = PixelArtImporter.ArtFolder + "Sprites";
        const string LibraryPath = PixelArtImporter.ArtFolder + "SpriteLibrary.asset";
        const string AudioFolder = PixelArtImporter.ArtFolder + "Audio";
        const string AudioLibraryPath = PixelArtImporter.ArtFolder + "AudioLibrary.asset";
        const string ScenePath = "Assets/Scenes/Kapitel1.unity";

        [MenuItem("TwoMoons/Kapitel 1 einrichten")]
        public static void Setup()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("TwoMoons", "Die Szene Kapitel1 gibt es schon. Neu anlegen?", "Ja", "Abbrechen"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            int createdSprites = CreateMissingSprites();
            int createdSounds = CreateMissingSounds();
            var library = CreateOrUpdateLibrary();
            var audioLibrary = CreateOrUpdateAudioLibrary();
            CreateScene(library, audioLibrary);

            EditorUtility.DisplayDialog("TwoMoons",
                $"Kapitel 1 ist eingerichtet ({createdSprites} neue Platzhalter-Bilder, {createdSounds} neue Sounds).\n\n" +
                "Drücke jetzt auf Play.\n\n" +
                $"Eigene 16×16-Pixelbilder: die PNGs in {SpriteFolder} mit demselben Namen ersetzen.\n" +
                $"Eigene Sounds: die WAVs in {AudioFolder} mit demselben Namen ersetzen.",
                "OK");
        }

        /// <summary>
        /// Menü „TwoMoons → Bilder und Sounds aktualisieren“: legt Platzhalter für neue Bilder/Sounds an und
        /// trägt alles in die Bibliotheken ein. Eigene Bilder bleiben, die Szene wird nicht angefasst.
        /// </summary>
        [MenuItem("TwoMoons/Bilder und Sounds aktualisieren")]
        public static void RefreshArt()
        {
            // Ersetzte Bilder sicher neu einlesen (Größe/Pixel pro Einheit passen sich an).
            AssetDatabase.ImportAsset(SpriteFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            int createdSprites = CreateMissingSprites();
            int createdSounds = CreateMissingSounds();
            var library = CreateOrUpdateLibrary();
            var audioLibrary = CreateOrUpdateAudioLibrary();
            int linked = LinkLibraries(library, audioLibrary);

            string sceneHint = linked > 0
                ? "Die Bibliotheken sind mit der Szene verbunden."
                : $"Achtung: In der offenen Szene gibt es kein TwoMoons-Objekt. Öffne {ScenePath} und klick den Menüpunkt nochmal.";
            EditorUtility.DisplayDialog("TwoMoons",
                $"Fertig: {createdSprites} neue Platzhalter-Bilder, {createdSounds} neue Sounds.\n{sceneHint}\n\n" +
                $"Eigene Bilder: PNG mit genau demselben Namen in {SpriteFolder} ersetzen (z. B. shelf.png).",
                "OK");
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

            if (library.logo == null)
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

            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
