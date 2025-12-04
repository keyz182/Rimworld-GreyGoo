using RimWorld;
using Verse;

namespace Grey_Goo;

[DefOf]
public static class Grey_GooDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;

    public static GG_ShaderTypeDef GG_LiquidMetal;
    public static GG_ShaderTypeDef GG_LiquidMetalSimplex;

    public static GGTerrainDef GG_Goo;

    public static FactionDef GG_GreyGoo;

    public static readonly TerrainDef GG_Goo_Inactive;
    public static readonly DamageDef GG_Goo_Burn;
    public static readonly DamageDef GG_GooMortarBurn;
    public static readonly DamageDef GG_Goo_GooShieldBurn;
    public static readonly SitePartDef GG_GooControllerSitePart;
    public static readonly WorldObjectDef GG_GooControllerWorldDef;
    public static readonly ThingDef GG_Goo_Mortar;
    public static readonly ThingDef GG_ArchotechPowerNode;
    public static readonly ThingDef GG_GooWaders;
    public static readonly GameConditionDef GG_GooBoosted;
    public static readonly ThingDef GG_Frogge;
    public static readonly ThingDef GG_Turret_EMPMiniTurret;
    public static readonly ThingDef GG_Gun_Improvised_EmpLauncher;
    public static readonly ThingDef Goo_Scarab_Database;

    static Grey_GooDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(Grey_GooDefOf));
}
