using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public class GGWorldComponent(World world) : WorldComponent(world)
{
    public static GGWorldComponent instance => Find.World.GetComponent<GGWorldComponent>();

    public static Lazy<MethodInfo> GetNextID = new(()=>AccessTools.Method(typeof(UniqueIDsManager), "GetNextID"));
    private int nextControllerId;
    public int GetNextControllerID()
    {
        object[] args = { nextControllerId }; // value goes in
        int result = (int)GetNextID.Value.Invoke(Find.UniqueIDsManager, args);
        nextControllerId = (int)args[0];      // updated ref value comes out
        return result;
    }

    public List<GreyGooController> controllers = new();

    private GooedTiles gooedTiles;

    public GooedTiles GooedTiles
    {
        get
        {
            gooedTiles ??= new GooedTiles(world.grid.Surface);
            return gooedTiles;
        }
    }

    public bool HasAlreadyStarted = false;

    public List<Tile> Tiles => Find.World.grid.Surface.Tiles;

    public bool TrySpawnController(IncidentParms parms, int locationTile = -1, bool isDebug = false)
    {
        if (!CanCreateNewController(isDebug))
        {
            return false;
        }

        if (locationTile < 0)
        {
            for (int i = 0; i < 100; i++)
            {
                Tile tile = Tiles.RandomElement();
                locationTile = Tiles.IndexOf(tile);
                if (tile.WaterCovered) continue;
                if (Find.World.worldObjects.AnyWorldObjectAt(locationTile)) continue;

                break;
            }
        }

        Site wo = (Site) WorldObjectMaker.MakeWorldObject(Grey_GooDefOf.GG_GooControllerWorldDef);
        Faction fac = Find.FactionManager.FirstFactionOfDef(Grey_GooDefOf.GG_GreyGoo);
        wo.SetFaction(fac);
        wo.Tile = locationTile;
        wo.customLabel = "GG_ActiveGreyGoo".Translate().Colorize(Color.red);
        wo.AddPart(new SitePart(wo, Grey_GooDefOf.GG_GooControllerSitePart, new SitePartParams()));
        Find.WorldObjects.Add(wo);

        GreyGooController controller = new(wo);
        controller.Setup();

        controllers.Add(controller);

        return true;
    }

    public override void WorldComponentTick()
    {
        if (Find.TickManager.TicksGame % 60 == 0)
        {
            foreach (GreyGooController greyGooController in controllers)
            {
                greyGooController.Tick();
            }
        }

        if (Find.TickManager.TicksGame % 300 == 0)
        {
            foreach (GreyGooController greyGooController in controllers)
            {
                greyGooController.LongTick();
            }
        }

        GooedTiles.Tick();
    }

    [CanBeNull]
    public GreyGooController ClosestController(int tile)
    {
        float distance = float.MaxValue;
        GreyGooController closest = null;

        foreach (GreyGooController greyGooController in controllers)
        {
            float dist = Find.World.grid.ApproxDistanceInTiles(Tiles.IndexOf(greyGooController.tile), tile);

            if (dist < distance)
            {
                distance = dist;
                closest = greyGooController;
            }
        }

        return closest;
    }

    public Direction8Way GetDirection8WayToNearestController(int tile)
    {
        if (controllers.Count == 0) return Direction8Way.Invalid;
        GreyGooController closest = ClosestController(tile);
        if (closest == null)
        {
            return Direction8Way.Invalid;
        }

        int controllerIdx = Tiles.IndexOf(closest.tile);

        return Find.World.grid.GetDirection8WayFromTo(tile, controllerIdx);
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref HasAlreadyStarted, "HasAlreadyStarted");
        Scribe_Values.Look(ref nextControllerId, "nextControllerId");
        Scribe_Collections.Look(ref controllers, "controllers", LookMode.Deep);
        Scribe_Deep.Look(ref gooedTiles, "gooedTiles", [world.grid.Surface]);
    }

    public float GetTileGooLevelAt(int tile)
    {
        if(tile < 0 || tile >= GooedTiles.Length) return 0f;
        return GooedTiles[tile].spread;
    }

    public bool CanCreateNewController(bool isDebug = false)
    {
        if (isDebug) return true;
        return controllers.Count < 3;
    }

    public virtual void Notify_ControllerDestroyed(Map map)
    {
        GreyGooController controller = controllers.FirstOrDefault(ggc => ggc.wo == map.Parent);
        controllers.Remove(controller);

        Find.LetterStack.ReceiveLetter(
            string.Format("GG_ControllerDestroyTitle".Translate()),
            string.Format("GG_ControllerDestroyDesc".Translate(), controller.wo.Label.Colorize(controller.wo.Faction.Color)),
            LetterDefOf.ThreatBig,
            new LookTargets(controller.wo)
        );
    }
}
