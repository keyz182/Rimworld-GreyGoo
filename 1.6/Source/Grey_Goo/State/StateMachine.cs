using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Grey_Goo.State;
public class StateMachine<T> : IExposable where T : State
{
    public bool Initialised = false;

    private Dictionary<string, T> _states = new();
    private string _state;
    private T _prevState;
    private int _lastTransitionTick;

    public T State
    {
        get => _states[_state];
        set => _state = value.ToString();
    }

    public bool IsInState(string stateName) => _states.TryGetValue(stateName, out T state) && State == state;

    // Transitions are grouped by source state: From -> (To -> Transition)
    protected readonly Dictionary<T, StateTransitions<T>> _transitions = new();
    public T PrevState { get => _prevState; private set => _prevState = value; }
    public int LastTransitionTick { get => _lastTransitionTick; private set => _lastTransitionTick = value; }
    public int TicksInState => Math.Max(0, Find.TickManager.TicksGame - _lastTransitionTick);

    public void ExposeData()
    {
        Scribe_Values.Look(ref _state, "state");
        Scribe_Values.Look(ref _prevState, "prevState");
        Scribe_Values.Look(ref _lastTransitionTick, "lastTransitionTick");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            BuildTransitions();
    }

    public virtual void RegisterState(T state)
    {
        _states[state.ToString()] = state;
    }

    public virtual void BuildTransitions() { }

    public virtual void Initialise([CanBeNull] string initial = null)
    {
        BuildTransitions();
        initial ??= _state;
        State = _states[ initial];
        PrevState = _states[ initial];
        LastTransitionTick = Find.TickManager.TicksGame;
        Initialised = true;
    }

    protected void ClearTransitions() => _transitions.Clear();

    protected void AddTransition(Transition<T> t)
    {
        if (!_transitions.TryGetValue(t.From, out StateTransitions<T> bucket))
        {
            bucket = new StateTransitions<T>();
            _transitions[t.From] = bucket;
        }
        bucket.Add(t);
    }

    public bool TryTransitionTo(string newStateName, out string reason)
    {
        reason = string.Empty;

        if (!_states.TryGetValue(newStateName, out T newState))
        {
            reason = "state '" + newStateName + "' not found. Valid states:" + string.Join(",", _states.Keys);
            return false;
        }

        if (!_transitions.TryGetValue(State, out StateTransitions<T> tf) || !tf.TryGet(newState, out Transition<T> tr) || !tr.CanApply(this, out reason))
            return false;
        PrevState = State;
        tr.Apply(this);
        LastTransitionTick = Find.TickManager.TicksGame;
        return true;
    }

    public bool CanTransitionTo(T newState, out string reason)
    {
        reason = string.Empty;
        return _transitions.TryGetValue(State, out StateTransitions<T> tf) && tf.TryGet(newState, out Transition<T> tr) && tr.CanApply(this, out reason);
    }
}
