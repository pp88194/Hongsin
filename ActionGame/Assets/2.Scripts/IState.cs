using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState<T> where T : MonoBehaviour
{
    T Instance { get; set; }
    public void OnEnter(T instance);
    public void OnUpdate();
    public void OnExit();
}