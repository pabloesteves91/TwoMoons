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

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;
            var importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = Art.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
