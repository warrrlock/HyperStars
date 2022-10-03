using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyControls : MonoBehaviour
{
    private Animator m_animator;
    private Rigidbody2D rb;
    public Transform m_GroundCheck;

    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private BoxCollider2D pushBox;
    [SerializeField] private Rigidbody2D charRB;

    const float k_GroundedRadius = 0.2f;
    public bool Grounded;

    public int m_facingDirection = 1;

    public Transform m_otherPlayer;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < m_otherPlayer.transform.position.x && m_facingDirection != 1 && Grounded)
        {

            flipX();
            m_facingDirection = 1; //facing left
        }

        else if (transform.position.x > m_otherPlayer.transform.position.x && m_facingDirection != -1 && Grounded)
        {
            flipX();
            m_facingDirection = -1;
        }

    }

    private void FixedUpdate()
    {

        if (Grounded)
        {
            Debug.Log("Dummy grounded");
            m_animator.SetBool("Grounded", true);
            pushBox.GetComponent<Collider2D>().enabled = true;
        }
        else if(!Grounded)
        {
            Debug.Log("Dummy not grounded");
            m_animator.SetBool("Grounded", false);
            pushBox.GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }

    public void TakeDmg() {
        m_animator.SetTrigger("Hurt");
        Debug.Log(" hurt");
    }
    void flipX()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
}
