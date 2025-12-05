using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Grey_Goo;

public class GooedTile: IExposable
{
    public int tileId;
    public int planetLayerId;
    public GreyGooController controller;
    public float spread;

    private List<PlanetTile> neighbours;

    public List<PlanetTile> Neighbours
    {
        get
        {
            if (!neighbours.NullOrEmpty())
            {
                return neighbours;
            }

            neighbours = [];
            Find.WorldGrid.GetTileNeighbors(Find.WorldGrid.Surface.PlanetTileForID(tileId), neighbours);

            return neighbours;
        }
        set => neighbours = value;
    }

    public GooedTile(){}

    public GooedTile(int tileId, int planetLayerId, GreyGooController controller, float spread)
    {
        this.tileId = tileId;
        this.planetLayerId = planetLayerId;
        this.controller = controller;
        this.spread = spread;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Values.Look(ref planetLayerId, "planetLayerId");
        Scribe_References.Look(ref controller, "controller");
        Scribe_Values.Look(ref spread, "spread");
    }
}
