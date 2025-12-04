using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModAssetBundleBuilder
{
    private const string outputDirectoryRoot = "../AssetBundles";

    [MenuItem("Assets/Build Compressed Asset Bundle (LZ4)")]
    public static void BuildBundles()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        string assetBundleName = "GreyGoo";
        foreach (string arg in arguments)
        {
            if (!arg.StartsWith("--assetBundleName="))
            {
                continue;
            }

            assetBundleName = arg.Substring("--assetBundleName=".Length);
            Debug.Log($"Using asset bundle name: {assetBundleName}");
        }

        // Ensure textures are labeled correctly before proceeding.
        string[] assetPaths = AssetLabeler.LabelAllAssetsWithCommonName(assetBundleName).ToArray();
        if (assetPaths.Length == 0)
        {
            Debug.LogError("No assets were labeled; aborting asset bundle build.");
            return;
        }

        // Since the bundle only includes generic assets like textures or sounds
        // and not platform-specific assets, we can build for all platforms.
        Debug.Log("Building asset bundle.");
        foreach (string assetPath in assetPaths)
        {
            Debug.Log($"Adding asset: {assetPath}");
        }

        AssetBundleBuild[] bundles = { new() { assetBundleName = "GreyGoo_linux", assetNames = assetPaths } };
        BuildPipeline.BuildAssetBundles(outputDirectoryRoot, bundles, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);

        bundles[0] =
            new() { assetBundleName = "GreyGoo_mac", assetNames = assetPaths };
        BuildPipeline.BuildAssetBundles(outputDirectoryRoot, bundles, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX);


        bundles[0] =
            new() { assetBundleName = "GreyGoo_windows", assetNames = assetPaths };
        BuildPipeline.BuildAssetBundles(outputDirectoryRoot, bundles, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        Debug.Log("Asset bundles built successfully.");
    }
}
