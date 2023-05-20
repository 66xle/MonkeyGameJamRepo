using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float maxVelocityChange = 10f;


    [Header("Jump")]
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpCooldown;
    private float jumpCounter;
    [SerializeField] float jumpCoyoteTime;
    private float jumpCoyoteCounter;
    [SerializeField] float jumpBufferTime;
    private float jumpBufferCounter;

    [Header("Detect Wall")]
    [SerializeField] float checkWallDistance = 0.1f;
    [SerializeField] LayerMask layerGround;

    [Header("Gravity")]
    [SerializeField] float gravity;
    [SerializeField] float gravityScale;
    [SerializeField] float fallGravityMultiplier;

    [Header("Ground")]
    [SerializeField] Vector2 boxSize;
    [SerializeField] Vector3 boxOffset;
    private bool isGrounded = false;

    private Rigidbody2D rb;

    float input;

    private bool isFlipped = false;
    public bool isStandingStill = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();

        Jump();

        StandStill();
        SwitchDirection();

        
    }

    private void FixedUpdate()
    {
        Movement();
        //Gravity();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
    }

    void StandStill()
    {
        if (!isStandingStill)
            return;

        input = Input.GetAxisRaw("Horizontal");

        if (input == 0f)
            return;

        if (isFlipped && input > 0f || !isFlipped && input < 0f)
            Flip();

        isStandingStill = false;
    }

    void Movement()
    {
        if (isStandingStill)
            return;

        rb.velocity = new Vector2(transform.right.x * speed, rb.velocity.y);
    }

    void Flip()
    {
        if (isFlipped)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            isFlipped = false;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            isFlipped = true;
        }
    }

    void SwitchDirection()
    {
        if (!isGrounded)
            return;

        Debug.DrawRay(transform.position, transform.right * checkWallDistance, Color.red);

        if (Physics2D.Raycast(transform.position, transform.right, checkWallDistance, layerGround))
        {
            Flip();
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }


    void Jump()
    {
        #region Coyote and Jump Buffer Timers


        // Coyote Time
        if (isGrounded && jumpCounter <= 0f)
        {
            jumpCoyoteCounter = jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;

            // Jumping cooldown
            jumpCounter -= Time.deltaTime;
        }

        //Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        #endregion

        if (!isGrounded)
            return;

        // Player jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("test");

            // Calculate Velocity
            rb.gravityScale = gravityScale;
            float velocity = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * rb.gravityScale) * -2) * rb.mass;
            //velocity += -rb.velocity.y; // Cancel out current velocity

            // Jump
            rb.AddForce(new Vector2(0f, velocity), ForceMode2D.Impulse);

            // Set jump cooldown
            jumpCounter = jumpCooldown;
        } 
    }


    void Gravity()
    {
        if (rb.velocity.y <= 0f)
        {
            // Player Falling
            rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * fallGravityMultiplier);
        }
        else
        {
            rb.AddForce(new Vector3(0, gravity, 0));
        }
    }

    void GroundCheck()
    {
        if (Physics2D.OverlapBox(transform.position + boxOffset, boxSize, 0f, layerGround))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

}
