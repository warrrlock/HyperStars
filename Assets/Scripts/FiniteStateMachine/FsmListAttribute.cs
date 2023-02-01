using System;
using FiniteStateMachine;
using UnityEngine;

public class FsmListAttribute : PropertyAttribute
{
    public readonly Type type;

    public FsmListAttribute(Type type)
    {
        this.type = type;
    }
}