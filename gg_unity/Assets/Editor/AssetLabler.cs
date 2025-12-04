using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetLabeler
{
    // The folder that holds your mod textures.
    private static readonly string assetsFolder = "Assets/Data";

    private static readonly HashSet<string> ExtensionsToProcess = new HashSet<string>(new[]
    {
        ".shader",
        ".png",
        ".jpeg",
        ".jpg",
        ".psd",
        ".wav",
        ".mp3",
        ".ogg"
    });

    /// <summary>
    ///     Converts a texture asset from Sprite to Default to prevent Unity from generating sprite sub-assets.
    /// </summary>
    /// <param name="assetPath">The path to the texture asset.</param>
    private static void ConvertSpriteToDefault(string assetPath)
    {
        if (AssetImporter.GetAtPath(assetPath) is not TextureImporter
            {
                textureType: TextureImporterType.Sprite
            } importer)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.SaveAndReimport();
        Debug.Log($"Converted {assetPath} from Sprite to Default.");
    }

    /// <summary>
    ///     Labels all assets  with a single common asset bundle name.
    /// </summary>
    /// <returns>The number of textures labeled.</returns>
    public static List<string> LabelAllAssetsWithCommonName(string assetFileName)
    {
        if (!Directory.Exists(assetsFolder))
        {
            Debug.LogError($"Folder not found: {assetsFolder}");
            return new List<string>();
        }

        // Get all the files under modTexturesFolder (and its subdirectories).
        string[] filePaths = Directory.GetFiles(assetsFolder, "*.*", SearchOption.AllDirectories);
        List<string> assetsLabeled = new();

        foreach (string filePath in filePaths)
        {
            // Normalize the path format.
            string assetPath = filePath.Replace("\\", "/");
            string extension = Path.GetExtension(assetPath).ToLower();

            if (!ExtensionsToProcess.Contains(extension))
            {
                continue;
            }

            bool isTexture = extension is ".png" or ".jpeg" or ".jpg" or ".psd";
            bool isShaderOrMat = extension is ".shader";

            // Confirm that the asset is located under the Assets folder.
            if (!assetPath.StartsWith("Assets"))
            {
                continue;
            }

            if (isTexture)
            {
                // Convert Sprite textures to Default to avoid additional sprite sub-assets.
                ConvertSpriteToDefault(assetPath);

                // Set a common asset bundle name for every texture.
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                if (importer is null)
                {
                    Debug.LogWarning($"Could not get importer for: {assetPath}");
                    continue;
                }

                importer.assetBundleName = assetFileName;
                importer.alphaIsTransparency = true;
                // Check if the path has terrain in it, if so, set the wrap mode to Repeat.
                importer.wrapMode = assetPath.ToLower().Contains("/terrain/")
                    ? TextureWrapMode.Repeat
                    : TextureWrapMode.Clamp;

                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Trilinear;
                importer.mipmapEnabled = true;
                importer.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
                importer.SaveAndReimport();
            }else if (isShaderOrMat)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                importer.assetBundleName = assetFileName;
                importer.SaveAndReimport();
            }
            else
            {
                AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(assetPath);
                if (importer is null)
                {
                    Debug.LogWarning($"Could not get importer for: {assetPath}");
                    continue;
                }

                importer.assetBundleName = assetFileName;
                AudioImporterSampleSettings sampleSettings = new AudioImporterSampleSettings
                {
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate,
                    loadType = AudioClipLoadType.CompressedInMemory,
                    quality = 0.25f,
                    preloadAudioData = true
                };
                importer.defaultSampleSettings = sampleSettings;
                importer.SaveAndReimport();
            }

            assetsLabeled.Add(assetPath);
            Debug.Log($"Labeled asset: {assetPath} as {assetFileName}");
        }

        Debug.Log($"Labeling complete: {assetsLabeled} assets labeled with \"{assetFileName}\".");
        return assetsLabeled;
    }

    // For manual testing from the Editor.
    [MenuItem("Assets/Label All Assets")]
    public static void Menu_LabelAllTexturesWithCommonName()
    {
        LabelAllAssetsWithCommonName("GreyGoo");
    }
}
