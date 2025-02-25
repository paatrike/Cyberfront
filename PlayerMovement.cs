using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour
{
    Rigidbody rb;

    [Tooltip("Current players speed")]
    public float currentSpeed;
    [Tooltip("Assign players camera here")]
    [HideInInspector] public Transform cameraMain;
    [Tooltip("Force that moves player into jump")]
    public float jumpForce = 500;
    [Tooltip("Position of the camera inside the player")]
    [HideInInspector] public Vector3 cameraPosition;

    public bool isClimbing = false; // Flag to check if the player is climbing a ladder
    public float ladderSpeed = 5f;  // Speed at which the player climbs

    private Vector3 slowdownV;
    private Vector2 horizontalMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cameraMain = transform.Find("Main Camera").transform;
        bulletSpawn = cameraMain.Find("BulletSpawn").transform;
        ignoreLayer = 1 << LayerMask.NameToLayer("Player");
    }

    void FixedUpdate()
    {
        RaycastForMeleeAttacks();
        PlayerMovementLogic();
    }

    void PlayerMovementLogic()
    {
        currentSpeed = rb.velocity.magnitude;
        horizontalMovement = new Vector2(rb.velocity.x, rb.velocity.z);

        if (isClimbing)
        {
            // Allow vertical movement when climbing a ladder
            float verticalInput = Input.GetAxis("Vertical"); // W or S to move up/down
            rb.velocity = new Vector3(0, verticalInput * ladderSpeed, 0);
        }
        else
        {
            // Normal ground movement logic
            if (horizontalMovement.magnitude > maxSpeed)
            {
                horizontalMovement = horizontalMovement.normalized;
                horizontalMovement *= maxSpeed;
            }

            rb.velocity = new Vector3(
                horizontalMovement.x,
                rb.velocity.y,
                horizontalMovement.y
            );

            if (grounded)
            {
                rb.velocity = Vector3.SmoothDamp(rb.velocity,
                    new Vector3(0, rb.velocity.y, 0),
                    ref slowdownV,
                    deaccelerationSpeed);
            }

            if (grounded)
            {
                rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime);
            }
            else
            {
                rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed / 2 * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed / 2 * Time.deltaTime);
            }

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                deaccelerationSpeed = 0.5f;
            }
            else
            {
                deaccelerationSpeed = 0.1f;
            }
        }
    }
 void Jumping()
 {
     if (Input.GetKeyDown(KeyCode.Space) && grounded)
     {
         if (CanJumpOverObstacle()) // Check if there is an obstacle
         {
             rb.AddForce(Vector3.up * jumpForce); // Jump over the obstacle
         }
         else
         {
             rb.AddRelativeForce(Vector3.up * jumpForce); // Normal jump
         }

         if (_jumpSound)
             _jumpSound.Play();
         else
             print("Missing jump sound.");

         _walkSound.Stop();
         _runSound.Stop();
     }
 }