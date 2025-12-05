using System.Text;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo.HarmonyPatches;


[HarmonyPatch(typeof(WorldInspectPane))]
public static class WorldInspectPane_Patch
{
    [HarmonyPatch("TileInspectString", MethodType.Getter)]
    [HarmonyPostfix]
    public static void TileInspectString(WorldInspectPane __instance, ref string __result)
    {
        Tile tile = Find.WorldGrid[Find.WorldSelector.SelectedTile];
        GGWorldComponent ggWorldComponent = Find.World.GetComponent<GGWorldComponent>();

        if (tile is not null && ggWorldComponent is not null)
        {
            float gooLevelAt = ggWorldComponent.GetTileGooLevelAt(Find.World.grid.Surface.Tiles.IndexOf(tile));

            if (!Mathf.Approximately(gooLevelAt, 0))
            {
                StringBuilder res = new StringBuilder(__result);
                res.Append("\n");
                res.Append("GG_TileInspectString".Translate((100 * gooLevelAt).ToString("0.00")));

                __result = res.ToString();
            }
        }
    }
}
