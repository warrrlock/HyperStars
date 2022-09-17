using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    private Vector3 movement;

    [SerializeField]private float inputX, inputY, MS, jumpForce, dashForce, grav;

    private Rigidbody rb;

    private bool jump;
    private bool facingRight = true;

    public Transform shadowSprite;

    public int faceDir;

    public bool isPlayer, attacking, dash;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dash = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Lab3D");
        }

        Vector3 shadowSpritePosition = shadowSprite.transform.position;
        shadowSpritePosition.y = 0;
        shadowSprite.transform.position = shadowSpritePosition;

        flip();

        if (facingRight)
        {
            faceDir = 1;
        }

        if (!facingRight)
        {
            faceDir = -1;
        }
    }

    private void FixedUpdate()
    {
        if(isPlayer && !attacking)
            rb.velocity = new Vector3(inputX * MS, rb.velocity.y, inputY * MS);

        
        rb.AddForce(new Vector3(0, -grav, 0));

        if (jump && !attacking)
        {
            rb.velocity = new Vector3(inputX, jumpForce, 0);
            jump = false;
        }

        if (dash && !attacking)
        {
            rb.velocity = new Vector3(movement.x * dashForce, rb.velocity.y, movement.z * dashForce)*Time.deltaTime;
            dash = false;
        }

  /*      if (attacking)
        {
            attacking = false;
        }*/

    }

    private void flip()
    {
        if ((inputX < 0f && facingRight) || (inputX > 0 && !facingRight))
        {
            facingRight = !facingRight;
            transform.localScale = (new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z));
        }
    }
}
