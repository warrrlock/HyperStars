using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;

[CreateAssetMenu(menuName = "StateMachine/States/In Air State")]
public class InAirState : BaseState
{
    [FsmList(typeof(Transition))] [SerializeField] private List<Transition> _transitions = new List<Transition>();
    [SerializeField] private bool _alwaysExecute;
    
    [SerializeField] private AttackInfo _attackInfo;
    

    private void OnEnable()
    {
        _transitions.RemoveAll(t => !t);
    }
    

    public override bool Execute(BaseStateMachine stateMachine, string inputName){
        if (stateMachine.PlayAnimation(_animationHash))
        {
            stateMachine.StartInAir(stateMachine.CheckRequeueJump);
            CheckSpecialMeter(stateMachine);
        }
        
        foreach (Transition transition in _transitions)
        {
            if (_alwaysExecute)
            {
                if (transition.Execute(stateMachine, inputName)) return true;
            }
            else if (transition.Execute(stateMachine, inputName, stateMachine.CanCombo())) return true;
        }

        return false;
    }

    public override void QueueExecute(BaseStateMachine stateMachine, string inputName){
        foreach (Transition transition in _transitions)
            transition.Execute(stateMachine, inputName, action: null, queueAtEndOfAnim: true);
    }

    public override AttackInfo GetAttackInfo()
    {
        return _attackInfo;
    }
    
    
#if UNITY_EDITOR
    public override void AddTransition(Transition t)
    {
        _transitions.Add(t);
        SaveChanges();
    }

    public override void DeleteTransition(Transition t)
    {
        _transitions.Remove(t);
        SaveChanges();
    }

    public override bool HasTransitions()
    {
        return true;
    }

    public override IReadOnlyList<Transition> GetTransitions()
    {
        return _transitions;
    }
#endif
}
