using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>
    /// Menü „TwoMoons → Web-Build erstellen“: setzt die Web-Einstellungen, baut das Spiel für den Browser
    /// und packt alles als ZIP zum Hochladen auf itch.io.
    /// </summary>
    public static class WebBuild
    {
        const string OutputFolder = "Builds/Web";
        const string ZipPath = "Builds/TwoMoons-Web.zip";

        [MenuItem("TwoMoons/Web-Build erstellen")]
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                EditorUtility.DisplayDialog("TwoMoons – Web-Build",
                    "Das Web-Modul fehlt noch.\n\n" +
                    "Unity Hub öffnen → Installs → bei deiner Unity-Version auf das Zahnrad → Add modules → " +
                    "„Web Build Support“ anhaken → Install. Danach Unity neu starten.",
                    "OK");
                return;
            }

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("TwoMoons – Web-Build",
                    "Es ist keine Szene zum Bauen eingetragen. Bitte zuerst „TwoMoons → Kapitel 1 einrichten“ ausführen.", "OK");
                return;
            }

            // Einstellungen, mit denen der Build auf itch.io, GitHub Pages und Netlify ohne Zusatzaufwand läuft.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.runInBackground = true;
            if (string.IsNullOrEmpty(PlayerSettings.productName) || PlayerSettings.productName == "TwoMoons-Temp")
                PlayerSettings.productName = "TwoMoons";

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputFolder,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("TwoMoons – Web-Build",
                    $"Der Build hat nicht geklappt ({report.summary.result}). Details stehen in der Console.", "OK");
                return;
            }

            if (File.Exists(ZipPath)) File.Delete(ZipPath);
            ZipFile.CreateFromDirectory(OutputFolder, ZipPath);
            EditorUtility.RevealInFinder(ZipPath);

            EditorUtility.DisplayDialog("TwoMoons – Web-Build",
                "Fertig!\n\n" +
                $"Hochladen auf itch.io: die Datei {ZipPath} nehmen, als „HTML“-Projekt hochladen und " +
                "„This file will be played in the browser“ anhaken.\n\n" +
                "Hinweis: Die index.html funktioniert nicht per Doppelklick – sie braucht einen Webserver wie itch.io.",
                "OK");
        }
    }
}
