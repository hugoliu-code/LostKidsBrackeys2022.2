using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float runSpeed = 1;
    [SerializeField] Animator anim;
    private Vector2 movementVector = new Vector2(0, 0);
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        movementVector = movementVector.normalized * runSpeed;
        if (movementVector.magnitude > 0.1)
            anim.SetBool("Running", true);
        else
            anim.SetBool("Running", false);

            rb.velocity = movementVector;
    }
}
