using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

/*Sources:
Charging using timer:  https://www.youtube.com/watch?v=FkXiAthKwV8
Jumping/landing collisions: https://www.youtube.com/watch?v=qcRRYeGMZ68&list=PLyH-qXFkNSxmDU8ddeslEAtnXIDRLPd_V&index=11
*/

public class PlayerController : MonoBehaviour
{
    // will cause a warning for 'being assigned but not used', ignore for now
    private bool isOnIce;

    private bool _isFacingRight = false;
    public bool IsFacingRight { get { return _isFacingRight; } private set {
        if (_isFacingRight != value) {
            transform.localScale *= new Vector2(-1, 1);
        }
        _isFacingRight = value;
    }}

    Rigidbody2D rb;
    Vector2 direction;
    public float jumpMax = 12f; //this is the maximum jump power, can multiply down to get partially charged jumps
    public float minJump = 4f; //this is the min jump power
    public float maxChargeTime = 1f; // maximum charge time set to 1 second

    //timer for charging jumps
    float timer;
    bool timerOn;

    Animator animator;

    public LogicManager logic;

    private bool isGrounded;

    public AudioClip chargeJumpClip;
    public AudioClip jumpClip;
    public AudioClip landingClip;

    private AudioSource audioSource;
    private bool wasGroundedLastFrame = true;
    private bool isChargingSoundPlaying = false;

    public bool hasFly = false;
    public float flyPauseDuration = 0.1f; // pause before double jump
    public float jumpScaleFactor = 0.7f; //can change jump size with this

    private bool usedFlyJump = false;
    private FlyPickup lastCollectedFly;
    private bool readyForFlyJump = false;
    public bool isFlyPaused = false;

    public Image flyIconUI; // Fly icon image
    public TextMeshProUGUI timerText; // Timer Text
    public TextMeshProUGUI endTimerText; //to display your time at the game over screen

    private float levelTimer = 0f;
    private bool levelFinished = false;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start() {
        timer = 0;
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManager>();
    }

    void Update() {
        //if game is over, nothing happens
        if (logic.gameIsOver) {
            endTimerText.text = "You got there in " + Mathf.FloorToInt(levelTimer) + " seconds!";
            return;
        }

        Timer();

        if (!isGrounded && hasFly && !usedFlyJump && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(PerformFlyJump());
            isFlyPaused = true;
            animator.SetBool("isPaused", true);
        }

        if (flyIconUI != null)
        {
            flyIconUI.enabled = hasFly;
        }

        if (!levelFinished)
        {
            levelTimer += Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = "Time: " + Mathf.FloorToInt(levelTimer);
            }
        }
        float yVel = rb.velocity.y;

        const float eps = 0.01f;
        if (Mathf.Abs(yVel) < eps)
            yVel = 0f;

        if (yVel < 0) {
            isGrounded = false;
            animator.SetBool("isRising", false);
        }
        else if (yVel > 0) {
            isGrounded = false;
            animator.SetBool("isRising", true);
        }
        else if (yVel == 0) {
            isGrounded = true;
            Debug.Log("Grounded!!");
            animator.SetBool("isRising", false);
        }

        if (isGrounded && !wasGroundedLastFrame) {
            audioSource.PlayOneShot(landingClip);
        }
        wasGroundedLastFrame = isGrounded;
        // Debug.Log("Y Velocity: " + yVel);


        //animator.SetBool("isGrounded", isGrounded);
    }

    //controllers for buttons for each direction
    //currently just mapped to the directional keys (except down) to press and hold
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!logic.gameIsOver) { //can't move if you're on the game over screen
            //Debug.Log("Direction: " + context.ReadValue<Vector2>());
            //check if grounded first (to avoid accidental double jumps)
            if (isGrounded || readyForFlyJump )
            {
                if (context.started)
                {
                    //begin charging jump
                    timerOn = true;
                    //get direction to jump in (left, right)
                    direction = context.ReadValue<Vector2>();
                    SetFacingDirection(direction);

                    animator.SetBool("isCharging", true);

                    if (!isChargingSoundPlaying)
                    {
                        audioSource.PlayOneShot(chargeJumpClip);
                        isChargingSoundPlaying = true;
                    }
                }

                if (context.canceled)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(jumpClip);

                    jump();

                    timerOn = false;
                    animator.SetBool("isCharging", false);
                    isChargingSoundPlaying = false;

                    isGrounded = false;
                    readyForFlyJump = false;
                    isFlyPaused = false;
                    animator.SetBool("isPaused", false);

                    rb.gravityScale = 1; // re-enable gravity after jump
                }
            }
        }
    }

    void jump()
    {
        float jumpPower = Mathf.Max(minJump, jumpMax * (timer / maxChargeTime)); 
        jumpPower *= jumpScaleFactor;

        Debug.Log("Jump power: " + jumpPower);

        rb.velocity = Vector2.zero; // reset current velocity first
        animator.SetTrigger("jump");

        if (direction == Vector2.up)
        {
            rb.velocity = new Vector2(0, jumpPower);
        }
        else if (direction == Vector2.left)
        {
            float jumpX = .5f * jumpPower;
            float jumpY = .94f * jumpPower;
            rb.velocity = new Vector2(-jumpX, jumpY);
        }
        else if (direction == Vector2.right)
        {
            float jumpX = .5f * jumpPower;
            float jumpY = .94f * jumpPower;
            rb.velocity = new Vector2(jumpX, jumpY);
        }
        else
        {
            Debug.LogWarning("Invalid direction: " + direction);
        }
    }

    public void OnRestart(InputAction.CallbackContext context) {
        if (context.started) {
            if (logic.gameIsOver) {
                logic.restartGame();
            }
        }
    }

    void SetFacingDirection(Vector2 direction) {
        if (direction.x < 0 && IsFacingRight) {
            IsFacingRight = false;
        }
        else if (direction.x > 0 && !IsFacingRight) {
            IsFacingRight = true;
        }
    }

    void Ricochet(Vector2 bounceDirection) {
        Vector2 velocity = rb.velocity;

        // Reverse horizontal direction with a forceful push
        velocity.x = bounceDirection.x * Mathf.Max(Mathf.Abs(velocity.x) *2f, 3f);

        // Preserve vertical velocity and add a slight boost
        velocity.y = Mathf.Max(velocity.y * 2f, 2f);

        rb.velocity = velocity;

        SetFacingDirection(velocity);

        Debug.Log("Wall ricochet!");
    }

    void Timer() {
        if (timerOn) {
            timer += Time.deltaTime;
            if (timer > maxChargeTime) {
                timer = maxChargeTime;
            }
        }
        else {
            timer = 0;
        }
    }

    public void CollectFly(FlyPickup fly)
    {
        hasFly = true;
        usedFlyJump = false;
        lastCollectedFly = fly;

        Debug.Log("Fly collected!");
    }

    IEnumerator PerformFlyJump()
    {
        usedFlyJump = true;
        hasFly = false;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(flyPauseDuration);

        readyForFlyJump = true;

        if (lastCollectedFly != null)
        {
            lastCollectedFly.RespawnFly();
            lastCollectedFly = null;
        }

        Debug.Log("Fly jump: paused, waiting for player jump input.");
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.CompareTag("Ice")) {
        isOnIce = true;
        isGrounded = true;
        Debug.Log("Landed on ICE");
        return;
    }
    else if (collision.collider.CompareTag("RicochetWall")) {
        isOnIce = false;
        Debug.Log("landed on floor");
        return;
    }
        if (!isGrounded && collision.gameObject.layer == LayerMask.NameToLayer("Ricochet")) {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 normal = contact.normal;

            if (Mathf.Abs(normal.x) > 0.8f) {
                Ricochet(new Vector2(Mathf.Sign(normal.x), 0));
            }
        }
        
        
    }

    public void LevelFinished()
    {
        levelFinished = true;
    }
}