using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;

[CreateAssetMenu(menuName = "StateMachine/States/In Air State")]
public class InAirState : BaseState
{
    [SerializeField] private string _animationName;
    private int _animationHash;
    [SerializeField] private List<Transition> _transitions = new List<Transition>();
    
    private void OnValidate()
    {
        _animationHash = Animator.StringToHash(_animationName);
        _transitions.RemoveAll(t => !t);
    }
    
    public override void Execute(BaseStateMachine stateMachine, string inputName){
        if (stateMachine.PlayAnimation(_animationHash))
            stateMachine.StartInAir();
        
        foreach (Transition transition in _transitions)
        {
            transition.Execute(stateMachine, inputName);
        }
    }
    
}
