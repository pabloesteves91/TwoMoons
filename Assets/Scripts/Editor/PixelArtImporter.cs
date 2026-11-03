using UnityEditor;
using UnityEngine;

namespace TwoMoons.EditorTools
{
    /// <summary>
    /// Neue Bilder in Assets/Art bekommen automatisch Pixel-Art-Einstellungen:
    /// Sprite, 16 Pixel pro Einheit, keine Weichzeichnung, keine Kompression.
    /// Bilder in Assets/Art/Sprites dürfen auch vergrößert exportiert sein (z. B. 256×256 statt 16×16):
    /// sie werden immer so eingestellt, dass sie genau ein Feld groß sind.
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        public const string ArtFolder = "Assets/Art/";
        public const string UiFolder = ArtFolder + "UI/";
        public const string SpriteFolder = ArtFolder + "Sprites/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;
            var importer = (TextureImporter)assetImporter;

            if (assetPath.StartsWith(SpriteFolder))
            {
                // Ein Feld = ein Bild, egal ob 16×16 oder 256×256 gemalt bzw. exportiert.
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                int size = Mathf.Max(width, height);
                if (size > 0) importer.spritePixelsPerUnit = Mathf.Max(1, size * Art.PixelsPerUnit / 16f);
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = Mathf.Max(importer.maxTextureSize, Mathf.NextPowerOfTwo(Mathf.Max(size, 32)));
            }
            if (!importer.importSettingsMissing) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            if (!assetPath.StartsWith(SpriteFolder)) importer.spritePixelsPerUnit = Art.PixelsPerUnit;
            // Menübilder (Logo) werden verkleinert angezeigt und sollen dabei weich bleiben.
            importer.filterMode = assetPath.StartsWith(UiFolder) ? FilterMode.Bilinear : FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
