using Grey_Goo.State;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Grey_Goo;

public class GreyGooController: IExposable, ILoadReferenceable
{
    /// <summary>
    /// Lifecycle state machine for this controller.
    /// </summary>
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

        // Ensure state machine exists and is initialised to Offline
        _stateMachine ??= new GooControllerStateMachine(this);
        if(!_stateMachine.Initialised) _stateMachine.Initialise(Grey_Goo.State.GooControllerStates.Offline);

        if (!_stateMachine.TryTransitionTo(Grey_Goo.State.GooControllerStates.Initialising, out string reason))
        {
            ModLog.Warn($"Failed to transition to Initialising state: {reason}");
        }
    }

    public void Tick()
    {

    }

    public void LongTick()
    {
        // Advance lifecycle based on time spent in temporary states
        if (_stateMachine.IsInState(Grey_Goo.State.GooControllerStates.Initialising) && _stateMachine.LastTransitionTick < Find.TickManager.TicksAbs - GenDate.TicksPerDay)
        {
            if (!_stateMachine.TryTransitionTo(Grey_Goo.State.GooControllerStates.Online, out string reason))
            {
                Log.Error($"Failed to transition to online state: {reason}");
            }
        }else if(_stateMachine.IsInState(Grey_Goo.State.GooControllerStates.Boosted) && _stateMachine.LastTransitionTick < Find.TickManager.TicksAbs - GenDate.TicksPerDay)
        {
            if (!_stateMachine.TryTransitionTo(Grey_Goo.State.GooControllerStates.Online, out string reason))
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
            // Reinitialise transitions after loading if needed, defaulting to Offline
            if(!_stateMachine.Initialised) _stateMachine.Initialise(Grey_Goo.State.GooControllerStates.Offline);
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
