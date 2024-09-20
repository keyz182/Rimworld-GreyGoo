using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public class GGWorldComponent(World world) : WorldComponent(world)
{
    public Dictionary<int, float> TileGooLevel = new Dictionary<int, float>();

    public List<GreyGooController> controllers = new List<GreyGooController>();

    public bool HasAlreadyStarted = false;

    public void Setup()
    {
        for (int i = 0; i < Find.World.grid.tiles.Count; i++)
        {
            TileGooLevel[i] = 0;
        }

        List<int> controllerLocations = new List<int>();

        for (int i = 0; i < 100 && controllerLocations.Count < 3; i++)
        {
            Tile tile = Find.World.grid.tiles.RandomElement();
            int idx = Find.World.grid.tiles.IndexOf(tile);

            if(controllerLocations.Contains(idx)) continue;

            if(tile.WaterCovered) continue;
            if (Find.World.worldObjects.AnyWorldObjectAt(idx))
            {
                continue;
            }

            controllerLocations.Add(idx);
        }

        foreach (int tile in controllerLocations)
        {
            Settlement wo = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            Faction fac = Find.FactionManager.FirstFactionOfDef(Grey_GooDefOf.GG_GreyGoo);
            wo.SetFaction(fac);
            wo.Tile = tile;
            wo.Name = SettlementNameGenerator.GenerateSettlementName(wo);
            Find.WorldObjects.Add(wo);

            GreyGooController controller = new GreyGooController(wo);
            controller.Setup();

            controllers.Add(controller);
        }
    }

    public override void WorldComponentTick()
    {
        if (!HasAlreadyStarted && Current.ProgramState == ProgramState.Playing)
        {
            HasAlreadyStarted = true;
            Setup();
        }

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
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref HasAlreadyStarted, "HasAlreadyStarted");
        Scribe_Collections.Look(ref TileGooLevel, "pollutedTiles", LookMode.Value);
        Scribe_Collections.Look(ref controllers, "controllers", LookMode.Value);
    }

    public void GooifyTileAt(int tile, float level = 0.1f)
    {
        if (!TileGooLevel.ContainsKey(tile))
            TileGooLevel[tile] = 0;
        TileGooLevel[tile] = Mathf.Clamp01(TileGooLevel[tile]+ level);

        GGUtils.NotifyGooChanged(tile);
    }

    public float GetTileGooLevelAt(int tile)
    {
        return TileGooLevel[tile];
    }
}
