using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OGHitBox : MonoBehaviour
{
    public ComboCharacter character;
    public Controller charCtrl;
    public Collider triggerCollider;
    public Collider playerHurtBox;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreCollision(triggerCollider, playerHurtBox);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider hitCollider)
    {
        Debug.Log(hitCollider.name);
        AttackPhysics playerAtkPhys = GetComponentInParent<AttackPhysics>();
        ComboCharacter hitController = hitCollider.GetComponentInParent<ComboCharacter>();
        AttackPhysics atkPhys = hitCollider.GetComponentInParent<AttackPhysics>();
        atkPhys.Knockback(character.Knockback.x * charCtrl.faceDir, character.Knockback.y);
        playerAtkPhys.attackPushback(10 * -charCtrl.faceDir, 0);
        hitController.TakeDamage();
        
    }
}
