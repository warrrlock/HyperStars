using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;

[CreateAssetMenu(menuName = "StateMachine/States/In Air State")]
public class InAirState : BaseState
{
    [SerializeField] private string _animationName;
    [SerializeField] private List<Transition> _transitions = new List<Transition>();
    
    [HideInInspector]
    [SerializeField] private int _animationHash;
    
    [SerializeField] private AttackInfo _attackInfo;
    
    private void OnValidate()
    {
        _animationHash = Animator.StringToHash(_animationName);
    }

    private void OnEnable()
    {
        _transitions.RemoveAll(t => !t);
    }

    public override void Execute(BaseStateMachine stateMachine, string inputName){
        if (stateMachine.PlayAnimation(_animationHash))
        {
            // stateMachine.SetReturnState("jump");
            stateMachine.StartInAir();
        }
            
        
        foreach (Transition transition in _transitions)
        {
            transition.Execute(stateMachine, inputName);
        }
    }
    
    public override AttackInfo GetAttackInfo()
    {
        return _attackInfo;
    }
    
}
