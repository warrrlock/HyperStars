using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrawlerControl : MonoBehaviour
{
    [SerializeField] private float ms, dashForce, jumpForce;
    [SerializeField] private float detectRadius;
    [SerializeField] private float movementSmooth = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform floorDetector;
    [SerializeField] private GameObject dashDust;
    [SerializeField] private bool isBot, doesCharacterJump;
    [SerializeField] private int jumpCount;

    public Rigidbody2D floorRB;

    public Rigidbody2D rb;
    public Collider2D pushBox;
    public Collider2D floorBox;

    private float inputX, inputY;

    private Vector2 movement;
    private Vector2 velocity = Vector2.zero;

    private bool facingRight = true;

    public bool attacking, dashing, jumping, crouching, grounded, canMove;

    public Animator charAnimator;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.transform.GetChild(0).GetComponent<Rigidbody2D>();
        floorRB = GetComponent<Rigidbody2D>();
        charAnimator = gameObject.transform.GetChild(0).GetComponent<Animator>();
        pushBox = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Brawler");
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(floorDetector.position, detectRadius, groundLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
            }
            else
            {
                grounded = false;
            }
            Debug.Log("Grounded" + grounded);
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            movement = new Vector2(inputX * ms, inputY * ms);
            //Vector2 _velocity = Vector2.SmoothDamp(floorRB.velocity, movement, ref velocity, movementSmooth);
            //floorRB.velocity = _velocity;

            floorRB.velocity = movement;

            if (dashing)
            {
                floorRB.AddForce(movement * dashForce, ForceMode2D.Impulse);
            }

            if (!attacking && !dashing)
            {
                flipX();
            }
            if (doesCharacterJump)
            {
                if (grounded)
                {   //on floor
                    //rb.velocity = movement;

                    if (jumping)
                    {
                        rb.velocity = movement;
                        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                        grounded = false;
                        jumping = false;
                        Debug.Log("Jump");
                    }
                }
                else
                {   //in air
                    if (rb.velocity.y < 0)
                    {
                        rb.gravityScale = 5;
                    }
                    rb.velocity = new Vector2(movement.x, rb.velocity.y);
                }

            }

            if (attacking)
            {
                movement = Vector2.zero;
            }
        }


    }
    private void flipX()
    {
        if ((movement.x < 0f && facingRight) || (movement.x > 0 && !facingRight))
        {
            facingRight = !facingRight;
            transform.Rotate(new Vector3(0, 180, 0));
        }
    }
    public void AE_DashDust()
    {
        Vector3 spawnPosition = floorDetector.position;

        if (dashDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(dashDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
        }
    }
    private void checkGround()
    {
        grounded = Physics2D.OverlapCircle(floorDetector.position, detectRadius, groundLayer);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(floorDetector.position, detectRadius);
    }
}
