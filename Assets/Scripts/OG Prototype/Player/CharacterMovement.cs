using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] public bool canMove = true;
    [SerializeField] private bool isPlayer = true;
    [Tooltip(("For characters that do not require jump"))]
    [SerializeField] private bool doesCharacterJump = false;

    [Header("Base / Floor")]
    [SerializeField] private Rigidbody2D baseRB;
    [SerializeField] private float hSpeed = 10f;
    [SerializeField] private float vSpeed = 6f;
    [Range(0, 1.0f)]
    [SerializeField] float movementSmooth = 0.5f;
    [SerializeField] private float dashForce;

    [Header("If Character Jumps")]
    [SerializeField] private Rigidbody2D charRB;
    [SerializeField] private float jumpVal = 10f;
    [SerializeField] private int possibleJumps = 1;
    [SerializeField] private int currentJumps = 0;
    [SerializeField] public bool onBase = false;
    [SerializeField] private Transform jumpDetector;
    [SerializeField] private float detectionDistance;
    [SerializeField] private LayerMask detectLayer;
    [SerializeField] private float jumpingGravityScale;
    [SerializeField] private float fallingGravityScale;
    //[SerializeField] private GameObject dashDust;
    public bool jump, dash, attacking;

    private bool facingRight = true;

    public int faceDir;

    private Vector3 velocity = Vector3.zero;

    private Vector2 movement;
    private float inputX, inputY;
    

    // Start is called before the first frame update
    private void Awake()
    {

    }

    private void Update()
    {

        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (isPlayer)
        {
            movement = new Vector3(inputX, inputY);
        }

        if (Input.GetButtonDown("Jump") && currentJumps < possibleJumps)
        {
            jump = true;
        }

/*        if (Input.GetButtonDown("Dash"))
        {
            dash = true;
        }*/

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Lab");
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (!onBase && doesCharacterJump)
        {
            charRB.gravityScale = fallingGravityScale;
            detectBase();
        }

        if (canMove)
        {
            Vector3 targetVelocity = new Vector3(movement.x * hSpeed, 0, movement.y * vSpeed);

            Vector2 _velocity = Vector3.SmoothDamp(baseRB.velocity, targetVelocity, ref velocity, movementSmooth);
            baseRB.velocity = _velocity;

            flip();

            //----- 
            if (doesCharacterJump)
            {
                if (onBase)
                {
                    // on base
                    charRB.velocity = _velocity;
                }
                else
                {
                    // in air
                    if (charRB.velocity.y < 0)
                    {
                        charRB.gravityScale = fallingGravityScale;
                    }
                    Debug.Log("Not on base");
                    
                    charRB.velocity = new Vector3(_velocity.x, charRB.velocity.y, _velocity.x);
                }

                if (jump)
                {
                    charRB.AddForce(Vector2.up * jumpVal, ForceMode2D.Impulse);
                    charRB.gravityScale = jumpingGravityScale;
                    jump = false;
                    currentJumps++;
                    onBase = false;
                }

                if (dash)
                {
                    charRB.AddForce(new Vector2 (movement.x * dashForce, movement.y * dashForce), ForceMode2D.Impulse);
                    dash = false;
                }
            }
    
        }

        if (!canMove) 
        {
            //baseRB.velocity = Vector3.zero;
            //charRB.velocity = Vector3.zero;
        }

        if (facingRight) {
            faceDir = 1;
        }

        if (!facingRight)
        {
            faceDir = -1;
        }
    }
    public void TakeDmg()
    {
        //m_animator.SetTrigger("Hurt");
        Debug.Log(" hurt");
    }
    private void flip()
    {
        if ((movement.x < 0f && facingRight) || (movement.x > 0 && !facingRight))
        {
            facingRight = !facingRight;
            transform.Rotate(new Vector3(0, 180, 0));
        }
    }

    private void detectBase()
    {

        RaycastHit2D hit = Physics2D.Raycast(jumpDetector.position, -Vector2.up, detectionDistance, detectLayer);
        if (hit.collider != null)
        {
            onBase = true;
            currentJumps = 0;
            Debug.Log("on base: " + onBase);
        }
    }

    private void OnDrawGizmos()
    {
        if (doesCharacterJump)
        {
            Gizmos.DrawRay(jumpDetector.transform.position, -Vector3.up * detectionDistance);
        }
    }

    /*    public void AE_DashDust()
    {
        Vector3 spawnPosition = jumpDetector.position;

        if (dashDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(dashDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
        }
    }*/
}