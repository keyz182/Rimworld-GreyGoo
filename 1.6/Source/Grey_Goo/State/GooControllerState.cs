using System;

namespace Grey_Goo.State;


public class GooControllerState : State<GooControllerStates, GooControllerState>, IIdentifiableState<GooControllerStates>
{
    public GreyGooController controller;
    public Action<GreyGooController> ActionOnEnter;
    public Action<GreyGooController> ActionOnExit;

    /// <summary>
    /// Creates a state with optional enter/exit actions.
    /// </summary>
    /// <param name="id">Enum identifier.</param>
    /// <param name="controller">Target controller.</param>
    /// <param name="actionOnEnter">Action to run on entry.</param>
    /// <param name="actionOnExit">Action to run on exit.</param>
    public GooControllerState(GooControllerStates id, GreyGooController controller, Action<GreyGooController> actionOnEnter = null, Action<GreyGooController> actionOnExit = null): base(id)
    {
        this.controller = controller;
        ActionOnEnter = actionOnEnter;
        ActionOnExit = actionOnExit;
    }

    /// <summary>
    /// Creates a state and immediately registers it with the provided state machine.
    /// </summary>
    /// <param name="sm">The hosting machine to register with.</param>
    /// <param name="id">Enum identifier.</param>
    /// <param name="controller">Target controller.</param>
    /// <param name="actionOnEnter">Action to run on entry.</param>
    /// <param name="actionOnExit">Action to run on exit.</param>
    /// <param name="addSelfTransition">Whether to also register a self-transition for this state.</param>
    public GooControllerState(StateMachine<GooControllerStates, GooControllerState> sm, GooControllerStates id, GreyGooController controller, Action<GreyGooController> actionOnEnter = null, Action<GreyGooController> actionOnExit = null, bool addSelfTransition = true)
        : this(id, controller, actionOnEnter, actionOnExit)
    {
        sm.RegisterState(this, addSelfTransition);
    }

    public override void OnEnter(StateMachine<GooControllerStates, GooControllerState> sm, GooControllerState from)
    {
        ActionOnEnter?.Invoke(controller);
    }

    public override void OnExit(StateMachine<GooControllerStates, GooControllerState> sm, GooControllerState to)
    {
        ActionOnExit?.Invoke(controller);
    }

}
