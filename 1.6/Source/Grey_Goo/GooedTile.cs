using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public class GooedTile: IExposable
{
    public int tileId;
    public int planetLayerId;
    public GreyGooController controller;
    public float spread;
    public bool neighboursGooed = false;

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

    public void Tick(int ticks = 1)
    {
        if (spread >= 1f)
        {
            // Do spread
            List<PlanetTile> neighbours = Neighbours.Where(tile => !GGWorldComponent.instance.GooedTiles.Any(gt => gt.tileId == tile.tileId)).ToList();
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
            spread += Grey_GooMod.settings.WorldMapGooIncrementPercentPerTick* ticks;
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Values.Look(ref planetLayerId, "planetLayerId");
        Scribe_References.Look(ref controller, "controller");
        Scribe_Values.Look(ref spread, "spread");
        Scribe_Values.Look(ref neighboursGooed, "neighboursGooed");
    }
}
