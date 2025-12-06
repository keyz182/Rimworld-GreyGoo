using System;
using JetBrains.Annotations;
using Verse;

namespace Grey_Goo.State;

public class GooControllerState(string name, GreyGooController controller, Action<GreyGooController> actionOnEnter = null, Action<GreyGooController> actionOnExit = null) : State
{
    public string name = name;
    public GreyGooController controller = controller;
    public Action<GreyGooController> ActionOnEnter = actionOnEnter;
    public Action<GreyGooController> ActionOnExit = actionOnExit;

    public override void OnEnter<T>(StateMachine<T> sm, T from)
    {
        ActionOnEnter?.Invoke(controller);
    }

    public override void OnExit<T>(StateMachine<T> sm, T to)
    {
        ActionOnExit?.Invoke(controller);
    }

    public override string ToString() => name;
}

public class GooControllerStateMachine(GreyGooController controller) : StateMachine<GooControllerState>
{
    public GreyGooController Controller = controller;

    public override void Initialise(string initial = null)
    {
        if (initial == null && State == null)
        {
            base.Initialise("Offline");
        }
        else
        {
            base.Initialise(initial);
        }

}

    public override void BuildTransitions()
    {
        GooControllerState offline = new(
            "Offline",
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 0f;
            });

        RegisterState(offline);

        GooControllerState initialising = new(
            "Initialising",
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 0.25f;
                controller.ggWorldComponent.GooedTiles.GooTile(Find.World.grid.Surface.Tiles.IndexOf(controller.tile));
            }
        );

        RegisterState(initialising);

        GooControllerState online = new(
            "Online",
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 1f;
            }
        );

        RegisterState(online);

        GooControllerState boosted = new(
            "Boosted",
            Controller,
            controller =>
            {
                controller.GooSpreadMultiplier = 3f;
            },
            controller =>
            {
                controller.GooSpreadMultiplier = 1f;
            }
        );

        RegisterState(boosted);

        AddTransition(new Transition<GooControllerState>(offline, initialising, (machine, from, to, out reason) =>
        {
            //TODO: Check if there's any game-states or anything preventing us coming online.
            reason = "";
            return true;
        }));

        AddTransition(new Transition<GooControllerState>(initialising, online));

        AddTransition(new Transition<GooControllerState>(online, boosted, (machine, from, to, out reason) =>
        {
            //TODO: Check if there's any game-states or anything preventing us boosting.
            reason = "";
            return true;
        }));

        AddTransition(new Transition<GooControllerState>(initialising, offline));

        AddTransition(new Transition<GooControllerState>(online, offline));

        AddTransition(new Transition<GooControllerState>(boosted, offline));
    }
}
