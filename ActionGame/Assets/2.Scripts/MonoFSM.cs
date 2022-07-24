using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoFSM<T> : MonoBehaviour where T : MonoBehaviour
{
    protected IState<T> curState;
    public virtual void SetState(IState<T> state)
    {
        curState?.OnExit();
        curState = state;
        curState?.OnEnter(this as T);
    }
    protected virtual void Update()
    {
        curState?.OnUpdate();
    }
}