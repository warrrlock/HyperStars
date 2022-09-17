using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBaseState : State
{
    // How long this state should be active for before moving on
    public float duration;
    // Cached animator component
    protected Animator animator;
    // bool to check whether or not the next attack in the sequence should be played or not
    protected bool shouldCombo;
    // The attack index in the sequence of attacks
    protected int attackIndex;

    // The hit collider of this attack
    protected Collider hitCollider;
    protected Transform hitTransform;

    protected Vector3 hitLocation;
    // Cached already struck objects of said attack to avoid overlapping attacks on same target
    private List<Collider> collidersDamaged;
    // The Hit Effect to Spawn on the afflicted Enemy
    private GameObject HitEffectPrefab;

    protected CharacterMovement player;

    protected Rigidbody playerRB;
    //protected Rigidbody2D floorRB;
    protected Controller charCtrl;

    // Input buffer Timer
    private float AttackPressedTimer = 0;

    protected float NeutralDuration1, NeutralDuration2, NeutralDuration3;

    protected Vector2 NeutralKnockBack1, NeutralKnockBack2, NeutralKnockBack3;

    protected Vector2 atkKnockback;

    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);
        collidersDamaged = new List<Collider>();
        hitCollider = GetComponent<ComboCharacter>().hitbox;
        HitEffectPrefab = GetComponent<ComboCharacter>().Hiteffect;
        hitTransform = GetComponent<ComboCharacter>().HitPos;
        player = GetComponent<CharacterMovement>();
        //floorRB = GetComponent<Rigidbody2D>();
        playerRB = GetComponent<ComboCharacter>().playerRB;
        animator = GetComponent<ComboCharacter>().animator;
        charCtrl = GetComponent<Controller>();
        NeutralDuration1 = GetComponent<ComboCharacter>().NeutralDuration1;
        NeutralDuration2 = GetComponent<ComboCharacter>().NeutralDuration2;
        NeutralDuration3 = GetComponent<ComboCharacter>().NeutralDuration3;
        NeutralKnockBack1   = GetComponent<ComboCharacter>().NeutralKnockBack1;
        NeutralKnockBack2 = GetComponent<ComboCharacter>().NeutralKnockBack2;
        NeutralKnockBack3 = GetComponent<ComboCharacter>().NeutralKnockBack3;
        atkKnockback = GetComponent<ComboCharacter>().Knockback;
        //player.canMove = false;
        charCtrl.attacking = true;
        
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        AttackPressedTimer -= Time.deltaTime;

        if (animator.GetFloat("Weapon.Active") > 0f)
        {
            //Attack(knockBackX, knockBackY);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            AttackPressedTimer = 5;
        }

        if (animator.GetFloat("AttackWindow.Open") > 0f && AttackPressedTimer > 0)
        {
            shouldCombo = true;
        }

    }

    public override void OnExit()
    {
        base.OnExit();
        charCtrl.attacking = false;

    }
    protected void Attack(float x, float y)
    {

        /*Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter();
        filter.useTriggers = true;
        filter.layerMask = 7;

        int colliderCount = Physics2D.OverlapCollider(hitCollider, filter, collidersToDamage);

        for (int i = 0; i < colliderCount; i++)
        {

            if (!collidersDamaged.Contains(collidersToDamage[i]))
            {
                TeamComponent hitTeamComponent = collidersToDamage[i].GetComponent<TeamComponent>();
                //DummyControls dumCtrl = collidersToDamage[i].GetComponent<DummyControls>();
                //CharacterMovement charMove = collidersToDamage[i].GetComponent<CharacterMovement>();
                hitPhys = collidersToDamage[i].GetComponentInChildren<AttackPhysics>();

                // Only check colliders with a valid Team Componnent attached
                if (hitTeamComponent && hitTeamComponent.teamIndex == TeamIndex.Enemy) //if "enemy" is hit //only ever need this if there's "non-enemy" objects in scene. worth keeping but not too needed
                {
                    GameObject.Instantiate(HitEffectPrefab, hitTransform.position, hitTransform.rotation);            //spawn hit effect
                                                                                                                      //dumCtrl.TakeDmg();                                                          //enemy takes damage
                                                                                                                      //hitPhys.charMove.TakeDmg();
                    hitPhys.Knockback(player.faceDir * knockBackX, knockBackY);                 //add knockback to enemy
                    Debug.Log("hurtbox hit");                                                    //debug

                    collidersDamaged.Add(collidersToDamage[i]);                                 //add to array to avoid damaging twice
                                                                                                //}
                }
            }*/
/*
        Collider[] hitColliders = Physics.OverlapBox(hitCollider.transform.position, hitCollider.transform.localScale / 2, Quaternion.identity, LayerMask.GetMask("Hurtbox"));
        
        foreach (Collider c in hitColliders)
        {

            //hitPhys.Knockback(player.faceDir * knockBackX, knockBackY);
            Debug.Log("hit: " + c.gameObject.name + " of: " + c.transform.parent.name);
            //collidersDamaged.Add(hitColliders[i]);
        }
*/
    }

}
