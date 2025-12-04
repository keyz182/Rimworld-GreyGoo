using Verse;

namespace Grey_Goo.Buildings.Comps;

public class CompProperties_GooMortar: CompProperties
{
    public IntRange SpitIntervalRangeTicks = new(5000, 7500);
    public CompProperties_GooMortar()
    {
        compClass = typeof(CompGooMortar);
    }
}
