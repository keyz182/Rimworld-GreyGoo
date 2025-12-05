using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Grey_Goo;

public static class GGUtils
{
    public static void NotifyGooChanged(int tile)
    {
        foreach (WorldLayer_GreyGoo worldLayerGreyGoo in Find.World.renderer.AllDrawLayers.OfType<WorldLayer_GreyGoo>())
        {
            worldLayerGreyGoo.Notify_TileGooChanged(tile);
        }
    }

}
