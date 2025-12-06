namespace Grey_Goo.State;

public delegate bool TransitionValidator<T>(StateMachine<T> stateMachine, T from, T to, out string reason)
    where T : State;
