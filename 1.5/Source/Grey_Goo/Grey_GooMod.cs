using Verse;
using UnityEngine;
using HarmonyLib;

namespace Grey_Goo;

public class Grey_GooMod : Mod
{
    public static Settings settings;

    public Grey_GooMod(ModContentPack content) : base(content)
    {
        Log.Message("Hello world from Grey Goo");

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("keyz182.rimworld.Grey_Goo.main");	
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
}
