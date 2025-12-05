using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public enum GooedStatus: int
{
    FullyGooed,
    PartiallyGooed,
    NotGooed
}

public class GooedTile: IExposable
{
    public int tileId;
    public int planetLayerId;
    public GreyGooController controller;
    public float spread;
    public bool neighboursGooed = false;
    public GooedStatus status = GooedStatus.NotGooed;
    public int lastUpdateTick = -1;

    public List<PlanetTile> Neighbours
    {
        get
        {
            if (!field.NullOrEmpty())
            {
                return field;
            }

            field = [];
            Find.WorldGrid.GetTileNeighbors(Find.WorldGrid.Surface.PlanetTileForID(tileId), field);

            return field;
        }
        set;
    }

    public GooedTile(){}

    public GooedTile(int tileId, int planetLayerId, GreyGooController controller, float spread)
    {
        this.tileId = tileId;
        this.planetLayerId = planetLayerId;
        this.controller = controller;
        this.spread = spread;
    }

    public void Goo(float amount)
    {
        if(status == GooedStatus.FullyGooed) return;
        if(status == GooedStatus.NotGooed) status = GooedStatus.PartiallyGooed;

        controller = GGWorldComponent.instance.ClosestController(tileId);

        spread = Mathf.Clamp01(spread + amount);
        GGUtils.NotifyGooChanged(tileId);
        GGWorldComponent.instance.GooedTiles.UngooedTiles.Remove(tileId);
        GGWorldComponent.instance.GooedTiles.ActiveTiles.Add(tileId, this);
    }

    public void Tick()
    {
        if(neighboursGooed) return;

        int ticks = Find.TickManager.TicksGame - lastUpdateTick;
        lastUpdateTick = Find.TickManager.TicksGame;

        if (status == GooedStatus.FullyGooed)
        {
            // Do spread
            List<PlanetTile> neighbours = Neighbours.Where(tile => GGWorldComponent.instance.GooedTiles.UngooedTiles.ContainsKey(tile.tileId)).ToList();
            if (neighbours.Count == 0)
            {
                neighboursGooed = true;
                return;
            }

            if (Rand.Chance(Grey_GooMod.settings.GooSpreadChance * ticks))
            {
                GGWorldComponent.instance.GooifyTileAt(neighbours.RandomElement(), spread);
            }
        }
        else
        {
            spread = Mathf.Clamp01(spread + Grey_GooMod.settings.WorldMapGooIncrementPercentPerTick* ticks);
            controller ??= GGWorldComponent.instance.ClosestController(tileId);
        }

        if(spread >= 0) status = GooedStatus.PartiallyGooed;
        if(spread >= 1) status = GooedStatus.FullyGooed;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Values.Look(ref planetLayerId, "planetLayerId");
        Scribe_References.Look(ref controller, "controller");
        Scribe_Values.Look(ref spread, "spread");
        Scribe_Values.Look(ref neighboursGooed, "neighboursGooed");
        Scribe_Values.Look(ref status, "status");
        Scribe_Values.Look(ref lastUpdateTick, "lastUpdateTick");
    }
}
