using Verse;

namespace Grey_Goo.State;


/// <summary>
/// State machine describing the lifecycle of a <see cref="GreyGooController"/>.
/// </summary>
/// <remarks>
/// States are constructed and self-registered in <see cref="BuildTransitions"/>. Transition guards are minimal
/// for now and can be extended to inspect world conditions.
/// </remarks>
public class GooControllerStateMachine(GreyGooController controller) : StateMachine<GooControllerStates, GooControllerState>
{
    /// <summary>Bound controller instance manipulated by state enter/exit actions.</summary>
    public GreyGooController Controller = controller;

    public override void BuildTransitions()
    {
        // Define states and register them with this machine via the self-registering constructor.
        GooControllerState offline = new(this,
            GooControllerStates.Offline,
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 0f;
            });

        GooControllerState initialising = new(this,
            GooControllerStates.Initialising,
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 0.25f;
                controller.ggWorldComponent.GooedTiles.GooTile(Find.World.grid.Surface.Tiles.IndexOf(controller.tile));
            }
        );

        GooControllerState online = new(this,
            GooControllerStates.Online,
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 1f;
            }
        );

        GooControllerState boosted = new(this,
            GooControllerStates.Boosted,
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 3f;
            },
            controller =>
            {
                controller.GooSpreadMultiplier = 1f; // Fallback
            }
        );

        GooControllerState diminished = new(this,
            GooControllerStates.Diminished,
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 0.25f;
            },
            controller =>
            {
                controller.GooSpreadMultiplier = 1f; // Fallback
            }
        );

        // Graph: Offline -> Initialising (guarded; placeholder for future checks)
        AddTransition(offline, initialising, (machine, from, to, out reason) =>
        {
            //TODO: Check if there's any game-states or anything preventing us coming online.
            reason = "";
            return true;
        });

        AddTransition(initialising, online);

        // Online -> Boosted (guarded; placeholder for future checks)
        AddTransition(online, boosted, (machine, from, to, out reason) =>
        {
            //TODO: Check if there's any game-states or anything preventing us boosting.
            reason = "";
            return true;
        });

        AddTransition(initialising, offline);
        AddTransition(online, offline);
        AddTransition(boosted, offline);


        AddTransition(initialising, diminished);
        AddTransition(online, diminished);
        AddTransition(boosted, diminished);

        AddTransition(diminished, initialising);
        AddTransition(diminished, online);
        AddTransition(diminished, boosted);
        AddTransition(diminished, offline);
    }
}
