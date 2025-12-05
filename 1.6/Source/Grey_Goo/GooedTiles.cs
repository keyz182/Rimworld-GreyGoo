using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Grey_Goo;

public class GooedTiles: IExposable
{
    public List<GooedTile> gooedTiles;

    private Dictionary<int,GooedTile> ungooedTiles;

    public Dictionary<int,GooedTile> UngooedTiles
    {
        get
        {
            ungooedTiles ??= gooedTiles.Where(gt => gt.status == GooedStatus.NotGooed).ToDictionary(gt => gt.tileId, gt => gt);
            return ungooedTiles;
        }
    }

    private Dictionary<int,GooedTile> activeTiles;

    public Dictionary<int,GooedTile> ActiveTiles
    {
        get
        {
            activeTiles ??= gooedTiles.Where(gt => gt.status != GooedStatus.NotGooed).ToDictionary(gt => gt.tileId, gt => gt);
            return activeTiles;
        }
    }

    public void ResetCache()
    {
        ungooedTiles = null;
        activeTiles = null;
    }

    public GooedTiles()
    {
    }

    public GooedTiles(PlanetLayer layer)
    {
        gooedTiles = layer.Tiles.Select(tile => new GooedTile(tile.tile.tileId, layer.LayerID, null, 0)).ToList();
    }

    public void GooTile(int tileId, float? amount = null)
    {
        amount ??= Grey_GooMod.settings.WorldMapGooIncrementPercentPerTick;
        gooedTiles[tileId].Goo(amount.Value);
    }

    public int Length => gooedTiles.Count;

    public GooedTile this[int tileId] => gooedTiles[tileId];

    public void Tick()
    {
        foreach (GooedTile tile in ActiveTiles.Values.TakeRandomDistinct(Grey_GooMod.settings.TilesToProcessPerTick))
        {
            tile.Tick();
        }
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref gooedTiles, "gooedTiles", LookMode.Deep);
    }
}
