using UnityEditor;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>
    /// Neue Bilder in Assets/Art bekommen automatisch Pixel-Art-Einstellungen:
    /// Sprite, 16 Pixel pro Einheit, keine Weichzeichnung, keine Kompression.
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        public const string ArtFolder = "Assets/Art/";
        public const string UiFolder = ArtFolder + "UI/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;
            var importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = Art.PixelsPerUnit;
            // Menübilder (Logo) werden verkleinert angezeigt und sollen dabei weich bleiben.
            importer.filterMode = assetPath.StartsWith(UiFolder) ? FilterMode.Bilinear : FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
