using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;

[CreateAssetMenu(menuName = "StateMachine/States/In Air State")]
public class InAirState : BaseState
{
    [SerializeField] private string _animationName;
    [FsmList(typeof(Transition))] [SerializeField] private List<Transition> _transitions = new List<Transition>();
    [SerializeField] private bool _alwaysExecute;
    
    [HideInInspector]
    [SerializeField] private int _animationHash;
    
    [SerializeField] private AttackInfo _attackInfo;
    
    [Header("Special")]
    [SerializeField] private bool _isSpecial;
    [Tooltip("number of bars the special costs. 1 means 1 bar.")]
    [SerializeField] private int _specialBarCost;

    
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
            stateMachine.StartInAir();
            if (_isSpecial) stateMachine.Fighter.SpecialMeterManager?.DecrementBar(_specialBarCost);
        }
        
        foreach (Transition transition in _transitions)
        {
            if (_alwaysExecute)
                transition.Execute(stateMachine, inputName);
            else transition.Execute(stateMachine, inputName, stateMachine.CanCombo);
        }
    }
    
    public override AttackInfo GetAttackInfo()
    {
        return _attackInfo;
    }
    
    public override int GetSpecialBarCost()
    {
        return _isSpecial ? _specialBarCost : -1;
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
