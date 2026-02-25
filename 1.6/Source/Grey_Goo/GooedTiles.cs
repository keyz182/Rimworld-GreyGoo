using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Grey_Goo;

public class GooedTiles: IExposable
{
    public List<TileGooTracker> gooedTiles;

    private Dictionary<int,TileGooTracker> ungooedTiles;

    public Dictionary<int,TileGooTracker> UngooedTiles
    {
        get
        {
            ungooedTiles ??= gooedTiles.Where(gt => gt.status == GooedStatus.NotGooed).ToDictionary(gt => gt.tileId, gt => gt);
            return ungooedTiles;
        }
    }

    private Dictionary<int,TileGooTracker> activeTiles;

    public Dictionary<int,TileGooTracker> ActiveTiles
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
        gooedTiles = layer.Tiles.Select(tile => new TileGooTracker(tile.tile.tileId, layer.LayerID, null, 0)).ToList();
    }

    public void GooTile(int tileId, float? amount = null)
    {
        if(tileId < 0 || tileId >= Length) return;

        amount ??= Grey_GooMod.settings.SpreadPercentagePerIncrease;
        gooedTiles[tileId].Goo(amount.Value);
    }

    public int Length => gooedTiles.Count;

    public TileGooTracker this[int tileId] => gooedTiles[tileId];

    public void Tick()
    {
        GenThreading.ParallelForEach(ActiveTiles.Values.TakeRandomDistinct(Grey_GooMod.settings.TilesToProcessPerTick), tile =>
        {
            tile.Tick(this);
        });

        int gooedCount = 0;
        while(_tilesToGoo.TryDequeue(out int tileId))
        {
            gooedCount++;
            gooedTiles[tileId].Goo(0.001f);
            if(gooedCount > Grey_GooMod.settings.TilesToProcessPerTick) break;
        }
    }

    private ConcurrentQueue<int> _tilesToGoo = new();

    public void QueueTileForGoo(int tileId)
    {
        _tilesToGoo.Enqueue(tileId);
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref gooedTiles, "gooedTiles", LookMode.Deep);
    }
}
