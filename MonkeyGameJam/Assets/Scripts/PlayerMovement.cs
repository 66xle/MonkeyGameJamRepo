using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 1f;
    [SerializeField] float fastSpeed = 1f;
    private bool badBanana = false;
    

    [Header("Jump")]
    [SerializeField] float reduceVelocity;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpCooldown;
    private float jumpCounter;
    [SerializeField] float coyoteTime;
    private float coyoteTimeCounter;
    [SerializeField] float jumpBufferTime;
    private float jumpBufferCounter;
    private bool isHoldingJump = false;

    [Header("Banana")]
    [SerializeField] int maxBanana = 3;
    private int currentBanana;
    [SerializeField] float eatCooldown = 0.5f;
    private float currentEatCooldown;
    [SerializeField] TextMeshProUGUI displayBananaCount;
    [SerializeField] GameObject bananaYellow;
    [SerializeField] GameObject bananaRed;

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

    [Header("Sounds")]
    [SerializeField] List<AudioClip> grassFootsteps;
    private int grassFootstepIndex;
    [SerializeField] AudioSource audioGrassFS;
    [SerializeField] AudioSource audioFall;
    [SerializeField] AudioSource audioJump;
    [SerializeField] AudioSource audioEat;
    [SerializeField] AudioSource audioRestock;

    float input;

    private bool isFlipped = false;
    public bool isStandingStill = true;
    private bool isFalling = false;

    private Animator animController;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<Animator>();
        rb.gravityScale = gravityScale;
        displayBananaCount.text = currentBanana.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        animController.SetFloat("Velocity", rb.velocity.y);

        GroundCheck();

        Fall();

        CoyoteAndJumpBuffer();
        

        EatBanana();

        StandStill();
        SwitchDirection();

        Sounds();
    }

    private void FixedUpdate()
    {
        Movement();
        Jump();
        //Gravity();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
    }

    void StandStill()
    {
        if (!isStandingStill || currentEatCooldown > 0f)
            return;

        input = Input.GetAxisRaw("Horizontal");

        if (input == 0f)
            return;

        if (isFlipped && input > 0f || !isFlipped && input < 0f)
            Flip();

        isStandingStill = false;
        animController.SetBool("isIdle", false);
    }

    void EatBanana()
    {
        if (currentEatCooldown > 0f)
        {
            currentEatCooldown -= Time.deltaTime;
        }

        if (!isGrounded)
            return;

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentBanana > 0 && currentEatCooldown <= 0f)
            {
                rb.velocity = Vector2.zero;

                badBanana = false;
                bananaRed.SetActive(false);
                bananaYellow.SetActive(true);

                currentEatCooldown = eatCooldown;
                currentBanana--;
                displayBananaCount.text = currentBanana.ToString();
                animController.SetTrigger("EatBanana");
                animController.SetBool("Fire", false);

                animController.SetBool("isIdle", true);
                isStandingStill = true;
            }
        }
    }

    void Movement()
    {
        if (isStandingStill)
        {
            return;
        }

        if (badBanana)
        {
            rb.velocity = new Vector2(transform.right.x * fastSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(transform.right.x * speed, rb.velocity.y);
        }
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
        if (!isGrounded || isStandingStill)
            return;

        Debug.DrawRay(transform.position, transform.right * checkWallDistance, Color.red);

        if (Physics2D.Raycast(transform.position, transform.right, checkWallDistance, layerGround))
        {
            Flip();
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    void CoyoteAndJumpBuffer()
    {
        #region Coyote and Jump Buffer Timers

        // Coyote Time
        if (isGrounded && jumpCounter <= 0f)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            // Jumping cooldown
            jumpCounter -= Time.deltaTime;
        }

        //Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        #endregion

        #region Holding Jump
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
            isHoldingJump = true;
        else
            isHoldingJump = false;
        #endregion

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            coyoteTimeCounter = 0f;
        }
    }

    void Jump()
    {
        if (currentEatCooldown > 0f)
            return;

        // Player jump input
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            animController.SetTrigger("Jump");
            audioJump.Play();

            // Calculate Velocity
            rb.gravityScale = gravityScale;
            float velocity = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * rb.gravityScale) * -2) * rb.mass;
            velocity += -rb.velocity.y; // Cancel out current velocity

            // Jump
            rb.AddForce(new Vector2(0f, velocity), ForceMode2D.Impulse);

            // Set jump cooldown
            jumpCounter = jumpCooldown;
            jumpBufferCounter = 0f;
        }
    }

    void Fall()
    {
        if (!isHoldingJump && rb.velocity.y > 0f && !isGrounded)
        {
            rb.AddForce(new Vector2(0f, reduceVelocity));
        }

        if (rb.velocity.y < -20f && !isGrounded)
        {
            isFalling = true;
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
            animController.SetBool("isGrounded", true);

            if (isFalling)
            {
                audioFall.Play();
                StartCoroutine(Slide());

                isFalling = false;
                isStandingStill = true;
                animController.SetBool("isIdle", true);
            }
        }
        else
        {
            isGrounded = false;
            animController.SetBool("isGrounded", false);
        }
    }

    IEnumerator Slide()
    {
        rb.velocity = new Vector2(transform.right.x * speed / 3, 0f);

        yield return new WaitForSeconds(0.5f);

        rb.velocity = Vector2.zero;
    }

    public void RefillBanana()
    {
        if (currentBanana < maxBanana)
        {
            currentBanana = maxBanana;

            displayBananaCount.text = currentBanana.ToString();
            audioRestock.Play();
        }
    }

    public void FastSpeed()
    {
        if (!badBanana)
        {
            currentBanana++;
            displayBananaCount.text = currentBanana.ToString();
            audioRestock.Play();
        }

        badBanana = true;
        animController.SetBool("Fire", true);

        bananaRed.SetActive(true);
        bananaYellow.SetActive(false);
    }

    void Sounds()
    {
        if (!isGrounded || isStandingStill)
        {
            //audioGrassFS.Stop();
            return;
        }

        if (!audioGrassFS.isPlaying)
        {
            grassFootstepIndex++;
            if (grassFootstepIndex >= grassFootsteps.Count)
                grassFootstepIndex = 0;

            audioGrassFS.clip = grassFootsteps[grassFootstepIndex];
            audioGrassFS.Play();
        }
    }

    void GulpSound()
    {
        audioEat.Play();
    }
}
