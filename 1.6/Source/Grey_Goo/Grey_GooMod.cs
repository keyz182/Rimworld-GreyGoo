using System.IO;
using System.Runtime.InteropServices;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace Grey_Goo;

public class Grey_GooMod : Mod
{
    public static Settings settings;

    public static Grey_GooMod mod;

    public Grey_GooMod(ModContentPack content) : base(content)
    {
        Log.Message("Hello world from Grey Goo");
        mod = this;

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new("keyz182.rimworld.Grey_Goo.main");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Grey Goo_SettingsCategory".Translate();
    }

    public AssetBundle MainBundle
    {
        get
        {
            string text = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                text = "StandaloneOSX";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                text = "StandaloneWindows64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                text = "StandaloneLinux64";
            }

            string bundlePath = Path.Combine(Content.RootDir, $@"Materials\Bundles\{text}\liquidmetal");
            Log.Message("Bundle Path: " + bundlePath);

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Log.Error("Failed to load bundle at path: " + bundlePath);
            }

            foreach (string allAssetName in bundle.GetAllAssetNames())
            {
                Log.Message($"[{allAssetName}]");
            }

            return bundle;
        }
    }
}
