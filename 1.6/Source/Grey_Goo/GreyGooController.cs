using Grey_Goo.State;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public class GreyGooController: IExposable, ILoadReferenceable
{
    private GooControllerStateMachine _stateMachine;
    public WorldObject wo;

    public Tile tile{
        get
        {
            field ??= Find.WorldGrid[wo.Tile];
            return field;
        }
    }

    public GreyGooController()
    {
        ID = GGWorldComponent.instance.GetNextControllerID();
    }

    public GreyGooController(WorldObject wo)
    {
        this.wo = wo;
        ID = GGWorldComponent.instance.GetNextControllerID();
    }

    public GGWorldComponent ggWorldComponent => Find.World.GetComponent<GGWorldComponent>();

    public float GooSpreadMultiplier = 1f;

    public void SetParent(WorldObject parent)
    {
        wo = parent;
    }

    public void Setup()
    {
        Find.LetterStack.ReceiveLetter(
            string.Format("GG_ControllerStartTitle".Translate()),
            string.Format("GG_ControllerStartDesc".Translate(), wo.Label.Colorize(wo.Faction.Color)),
            LetterDefOf.ThreatBig,
            new LookTargets(wo)
        );

        if (wo is Settlement settlement)
        {
            settlement.Name = $"{settlement.Name} GG_ActiveGreyGoo".Translate().Colorize(Color.red);
        }

        _stateMachine ??= new GooControllerStateMachine(this);
        if(!_stateMachine.Initialised) _stateMachine.Initialise("Offline");

        if (!_stateMachine.TryTransitionTo("Initialising", out string reason))
        {
            ModLog.Warn($"Failed to transition to Initialising state: {reason}");
        }
    }

    public void Tick()
    {

    }

    public void LongTick()
    {
        if (_stateMachine.IsInState("Initialising") && _stateMachine.LastTransitionTick < Find.TickManager.TicksAbs - GenDate.TicksPerDay)
        {
            if (!_stateMachine.TryTransitionTo("Online", out string reason))
            {
                Log.Error($"Failed to transition to online state: {reason}");
            }
        }else if(_stateMachine.IsInState("Boosted") && _stateMachine.LastTransitionTick < Find.TickManager.TicksAbs - GenDate.TicksPerDay)
        {
            if (!_stateMachine.TryTransitionTo("Online", out string reason))
            {
                Log.Error($"Failed to transition to Online state: {reason}");
            }
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref _id, "id");
        Scribe_Deep.Look(ref _stateMachine, "stateMachine", this);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if(!_stateMachine.Initialised) _stateMachine.Initialise();
        }
    }

    #region UniqueID
    private int _id;

    public int ID
    {
        get => _id;
        private set => _id = value;
    }

    public string GetUniqueLoadID()
    {
        return "GreyGooController_" + ID;
    }
    #endregion
}
