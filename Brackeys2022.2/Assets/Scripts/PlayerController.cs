using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float runSpeed = 1;
    [SerializeField] float puddleSpeed = 0.5f;
    private float currentSpeed = 1;
    [SerializeField] Animator anim;
    private Vector2 movementVector = new Vector2(0, 0);
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed;
    }
    void Update()
    {
        Movement(); 
    }
    private void Movement()
    {
        movementVector = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            movementVector.x = -1;
        if (Input.GetKey(KeyCode.D))
            movementVector.x = 1;
        if (Input.GetKey(KeyCode.W))
            movementVector.y = 1;
        if (Input.GetKey(KeyCode.S))
            movementVector.y = -1;
        movementVector = movementVector.normalized * currentSpeed;
        if (movementVector.magnitude > 0.1)
            anim.SetBool("Running", true);
        else
            anim.SetBool("Running", false);

            rb.velocity = movementVector;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Puddles"))
        {
            currentSpeed = puddleSpeed;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Puddles"))
        {
            currentSpeed = runSpeed;
        }
    }
}
