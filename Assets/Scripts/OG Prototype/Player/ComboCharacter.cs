using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboCharacter : MonoBehaviour
{

    public StateMachine meleeStateMachine;
    public Text stateUI;

    [Header("Character Visuals")]
    public Animator animator;

    [Header("Character Physics")]
     public Rigidbody playerRB;
     //public Rigidbody2D floorRB;

    [Header("Attack Parameters")]
    public float NeutralDuration1;
    public Vector2 NeutralKnockBack1;

    public float NeutralDuration2;
    public Vector2 NeutralKnockBack2;

    public float NeutralDuration3;
    public Vector2 NeutralKnockBack3;

    public Vector2 Knockback;

    [Header("Collision Boxes")]
    public Collider hitbox;
    public GameObject hitBoxx;
    public GameObject Hiteffect;
    public Transform  HitPos;


    // Start is called before the first frame update
    void Start()
    {
        //meleeStateMachine = GetComponent<StateMachine>();
        Debug.Log(meleeStateMachine.CurrentState);
    }

    // Update is called once per frame
    void Update()
    {
        stateUI.text = "current state: "+ meleeStateMachine.CurrentState;

        if (meleeStateMachine.CurrentState.GetType() == typeof(NeutralJab))
        {
            Knockback = new Vector2(NeutralKnockBack1.x, NeutralKnockBack1.y);
        }
         
        if (meleeStateMachine.CurrentState.GetType() == typeof(NeutralFollow))
        {
            Knockback = new Vector2(NeutralKnockBack2.x, NeutralKnockBack2.y);
        }
        
        if (meleeStateMachine.CurrentState.GetType() == typeof(NeutralFinish))
        {
            Knockback = new Vector2(NeutralKnockBack3.x, NeutralKnockBack3.y);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.DrawCube(hitBoxx.transform.position, hitBoxx.transform.localScale);
    }

    public void TakeDamage()
    {
        meleeStateMachine.SetNextState(new OGHurtState());
        Debug.Log(this.name + "take dmg");
    }
}
