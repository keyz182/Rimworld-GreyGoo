using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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

public class TileGooTracker: IExposable
{
    public int tileId;
    public int planetLayerId;
    private GreyGooController _controller;
    public GreyGooController controller => _controller ??= GGWorldComponent.instance.ClosestController(tileId);

    public float spread;
    public bool neighboursGooed = false;
    public GooedStatus status = GooedStatus.NotGooed;
    public int lastUpdateTick = -1;
    private float spreadPerTick = -1;

    public float SpreadPerTick
    {
        get
        {
            if (Mathf.Approximately(spreadPerTick, 0) || spreadPerTick < 0)
            {
                SpreadPerTick = Grey_GooMod.settings.DaysToFullyGooTile.RandomInRange / GenDate.TicksPerDay;
            }
            return spreadPerTick;
        }
        set => spreadPerTick = value;
    }

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

    public TileGooTracker(){}

    public TileGooTracker(int tileId, int planetLayerId, GreyGooController controller, float spread)
    {
        this.tileId = tileId;
        this.planetLayerId = planetLayerId;
        _controller = controller;
        this.spread = spread;
    }

    public void Goo(float amount)
    {
        switch (status)
        {
            case GooedStatus.FullyGooed:
            case GooedStatus.PartiallyGooed:
                return;
            case GooedStatus.NotGooed:
                status = GooedStatus.PartiallyGooed;
                break;
            default:
                status = GooedStatus.NotGooed;
                return;
        }

        spread = Mathf.Clamp01(spread + amount);
        GGUtils.NotifyGooChanged(tileId);
        GGWorldComponent.instance.GooedTiles.UngooedTiles.Remove(tileId);
        if(!GGWorldComponent.instance.GooedTiles.ActiveTiles.ContainsKey(tileId)) GGWorldComponent.instance.GooedTiles.ActiveTiles.Add(tileId, this);
    }

    public void Tick(GooedTiles tiles)
    {
        if(neighboursGooed && status == GooedStatus.FullyGooed) return;

        if (lastUpdateTick < 0)
        {
            // Establish baseline on first tick to avoid huge delta
            lastUpdateTick = Find.TickManager.TicksGame;
            return;
        }
        int ticks = Find.TickManager.TicksGame - lastUpdateTick;
        lastUpdateTick = Find.TickManager.TicksGame;
        List<PlanetTile> neighbours = Neighbours.Where(tile => GGWorldComponent.instance.GooedTiles.UngooedTiles.ContainsKey(tile.tileId)).ToList();

        if (status == GooedStatus.FullyGooed)
        {
            // Do spread
            if (neighbours.Count == 0)
            {
                neighboursGooed = true;
            }
            else
            {
                if (Rand.Chance((Grey_GooMod.settings.ChancePerHourToSpreadToNewTileWhenFullyGooed / GenDate.TicksPerHour) * ticks))
                {
                    tiles.QueueTileForGoo(neighbours.RandomElement().tileId);
                }
            }
        }
        else
        {
            float mul = controller?.GooSpreadMultiplier ?? 1f;
            spread = Mathf.Clamp01(spread + (SpreadPerTick * ticks * mul));

            if (neighbours.Count > 0)
            {
                if (Rand.Chance((Grey_GooMod.settings.ChancePerDayToSpreadToNewTileWhenNotFullyGooed / GenDate.TicksPerDay) * ticks))
                {
                    tiles.QueueTileForGoo(neighbours.RandomElement().tileId);
                }
            }
        }

        if (spread >= 1f) status = GooedStatus.FullyGooed;
        else if (spread > 0f) status = GooedStatus.PartiallyGooed;
        else status = GooedStatus.NotGooed;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Values.Look(ref planetLayerId, "planetLayerId");
        Scribe_References.Look(ref _controller, "controller");
        Scribe_Values.Look(ref spread, "spread");
        Scribe_Values.Look(ref neighboursGooed, "neighboursGooed");
        Scribe_Values.Look(ref status, "status");
        Scribe_Values.Look(ref lastUpdateTick, "lastUpdateTick");
        Scribe_Values.Look(ref spreadPerTick, "spreadPerTick");
    }
}
