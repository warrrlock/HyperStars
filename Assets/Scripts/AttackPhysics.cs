using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AttackPhysics : MonoBehaviour
{
    //public Rigidbody2D floorRB;
    public Rigidbody characterRB;
    public Controller charMove;

/*    public Rigidbody2D floorRB;
    public Rigidbody2D characterRB;
    public CharacterMovement charMove;*/

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Knockback(float x, float y)
    {
        //floorRB.AddForce(new Vector2(x, 0), ForceMode2D.Impulse);
        characterRB.AddForce(new Vector2(x, y), ForceMode.Impulse);
        Debug.Log("Knockback: "+x+y);
    }

    public void attackPushback(float x, float y)
    {
        //floorRB.AddForce(new Vector2(x, y), ForceMode2D.Impulse);
        characterRB.AddForce(new Vector2(x, y), ForceMode.Impulse);
        Debug.Log("attack pushback");
    }

    public void attackForce(float x)
    {
        characterRB.AddForce(new Vector2(x * charMove.faceDir, 0) , ForceMode.Impulse);
        //floorRB.MovePosition(transform.position * x * Time.deltaTime);

        Debug.Log("Attack force");
    }

    public void moveStop(float x)
    {
        characterRB.velocity = Vector3.zero;
        //characterRB.velocity = Vector3.zero;
    }
}