using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PaneColor { Colorless, Red, Blue, Purple };

    // STATIC:
    public static int currentScene;
    public static int currentWorld;

    public static Color worldBackgroundColor;
    public static Color worldPaneColor;
    private static bool tutorialHasDisplayedAtStart;
    private static bool titleHasDisplayedAtStart;

    // PREFAB REFERENCE:
    [SerializeField] private List<SpriteRenderer> playerSRs = new();
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D myCol;
    [SerializeField] private CircleCollider2D groundCheckCol;

    // SCENE REFERENCE:
    [SerializeField] private Transform playZone;
    [SerializeField] private ScreenWipe screenWipe;
    [SerializeField] private TMP_Text levelName;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private ScreenShake screenShake;
    [SerializeField] private GameObject titleScreen;

    // CONSTANT:
    public int worldNumber; // Set in each scene!!
    public int levelNumber; // Set in each scene!!

    private readonly float moveSpeed = 5;
    private readonly float jumpForce = 12.5f;
    private readonly float doubleJumpForce = 10;
    private readonly float fallMultiplier = 1.8f; // Fastfall
    
    private readonly float aerialRollDrag = 1.5f;

    private readonly float rotateSpeed = 150;

    private readonly float deathShakeDuration = .12f;
    private readonly float deathShakeStrength = .4f;

    [SerializeField] private List<Color> worldBackgroundColors = new();
    [SerializeField] private List<Color> worldPaneColors = new();

    // DYNAMIC:
    private Vector2 moveInput;
    private bool jumpInput;
    private int jumpCount;

    [NonSerialized] public bool rotating; // Read by Inventory

    [NonSerialized] public bool isGrounded; // Read by Inventory

    private float currentRollSpeed;

    private void Awake()
    {
        if (!tutorialHasDisplayedAtStart)
        {
            Tutorial(false);
            tutorialHasDisplayedAtStart = true; // Never gets set back to false
        }

        if (!titleHasDisplayedAtStart && titleScreen != null)
        {
            titleScreen.SetActive(true);

            titleHasDisplayedAtStart = true; // Never gets set back to false
        }

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (worldNumber != currentWorld) // True the first time we've moved to a new world
        {
            currentWorld = worldNumber;

            if (worldNumber == 1)
                AudioManager.Instance.ForceStartWorldMusic(currentWorld);
            else
                AudioManager.Instance.SetWorldMusic(currentWorld);

            worldBackgroundColor = worldBackgroundColors[currentWorld - 1];
            worldPaneColor = worldPaneColors[currentWorld - 1];
        }

        levelName.text = worldNumber + "-" + levelNumber;
    }

    private void Update()
    {
        // Player boundaries:
        if (!rotating)
        {
            if (transform.position.x > 5.637f)
                transform.position = new(5.637f, transform.position.y);
            if (transform.position.x < -5.637f)
                transform.position = new(-5.637f, transform.position.y);
            if (transform.position.y > 5.637f)
                transform.position = new(transform.position.x, 5.637f);
            if (transform.position.y < -5.637f)
                transform.position = new(transform.position.x, -5.637f);
        }

        Roll();

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
            jumpInput = true;

        if (jumpInput && titleScreen != null && titleScreen.activeSelf)
        {
            titleScreen.SetActive(false);

            screenWipe.StartWipe(false, true);
        }
    }

    private void FixedUpdate()
    {
        // Snappy horizontal movement:
        // (This movement method will prevent the player from slowing completely in a frictionless environment. To prevent this,
        // this rigidbody's linear drag is set to .01)
        float desiredVelocity = moveInput.x * moveSpeed;
        float velocityChange = desiredVelocity - rb.linearVelocity.x;
        float acceleration = velocityChange / .05f;
        float force = rb.mass * acceleration;
        rb.AddForce(new(force, 0));

        // Fastfall
        if (rb.linearVelocity.y < 0)
            // Subtract fall and lowjump multipliers by 1 to more accurately represent the multiplier (fallmultiplier = 2 means fastfall will be x2)
            rb.linearVelocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;

        if (jumpInput)
        {
            jumpInput = false;

            if (!myCol.enabled) // Stunned, such as when dragging or when tutorial screen is active
                return;

            if (jumpCount > 0)
            {
                bool isFirstJump = (jumpCount == 2);

                jumpCount -= 1;
                rb.linearVelocity = new(rb.linearVelocity.x, 0);

                float newForce = isFirstJump ? jumpForce : doubleJumpForce;
                rb.AddForce(Vector2.up * newForce, ForceMode2D.Impulse);

                if (isFirstJump) AudioManager.Instance.PlayJumpSfx();
                else AudioManager.Instance.PlayDoubleJumpSfx();
            }
        }
    }

    public void OnGroundCheckStay(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
        {
            jumpCount = 2;
            isGrounded = true;
        }
    }
    public void OnGroundCheckExit(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
        {
            jumpCount = 1;
            isGrounded = false;
        }
    }

    public void Die() // Called by Spikes
    {
        rb.linearVelocity = Vector2.zero;
        ToggleStun(true);

        AudioManager.Instance.PlayDeathSfx();

        screenShake.StartShake(deathShakeDuration, deathShakeStrength);

        GetComponentInChildren<Animator>().SetTrigger("isDie");
        Invoke(nameof(FinishDying), .3f);
        // If we don't have a delay here we can't see the player death animation because the screenwipe is instant
    }
    private void FinishDying()
    {
        screenWipe.StartWipe(true);

        Invoke(nameof(Restart), 1);
    }

    public void Win()
    {
        currentScene += 1; // It will be set to the new scene in awake anyway, but this is so Restart knows which scene to load

        ToggleStun(true);
        rb.linearVelocity = Vector2.zero;

        AudioManager.Instance.PlayVictorySfx();

        screenWipe.StartWipe(false);

        Invoke(nameof(Restart), 1f);
    }

    public void SelectRestart()
    {
        if (rotating)
            return;

        rb.linearVelocity = Vector2.zero;
        ToggleStun(true);

        screenWipe.StartWipe(false);
        Invoke(nameof(Restart), 1);
    }

    public void Restart()
    {
        // Must reset static int BEFORE going to new scene, otherwise stuff will be messed up during Awake of new scene
        PaneNumberFinder.rotationSteps = 0;

        SceneManager.LoadScene(currentScene);
    }

    public void ToggleStun(bool on)
    {
        rb.constraints = on ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
        myCol.enabled = !on;
        groundCheckCol.enabled = !on;
    }

    public void Teleporter(Vector2 destination)
    {
        StartCoroutine(TeleportRoutine(destination));
    }

    private IEnumerator TeleportRoutine(Vector2 destination)
    {
        rb.linearVelocity = Vector2.zero;
        ToggleStun(true);

        AudioManager.Instance.PlayTeleportSfx();

        foreach (SpriteRenderer sr in playerSRs)
            sr.enabled = false;

        yield return new WaitForSeconds(.15f);

        transform.position = destination;

        foreach (SpriteRenderer sr in playerSRs)
            sr.enabled = true;

        ToggleStun(false);
    }

    public void Rotator(bool rotateClockwise)
    {
        if (rotating)
            return;

        AudioManager.Instance.PlayRotateSfx();

        float rotation = rotateClockwise ? 90 : -90;
        StartCoroutine(Rotate(rotation, rotateSpeed));

        PaneNumberFinder.Rotate(rotateClockwise);
    }
    public static event Action<bool> OnRotateStartStop; // bool = start
    private IEnumerator Rotate(float rotation, float speed)
    {
        rotating = true;

        rb.linearVelocity = Vector2.zero;
        myCol.enabled = false;
        ToggleStun(true);

        OnRotateStartStop?.Invoke(true);

        Quaternion target = playZone.rotation * Quaternion.Euler(0, 0, -rotation);

        while (Quaternion.Angle(playZone.rotation, target) > 0f)
        {
            playZone.rotation = Quaternion.RotateTowards(
                playZone.rotation,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        ToggleStun(false);
        myCol.enabled = true;

        OnRotateStartStop?.Invoke(false);

        yield return new WaitForSeconds(.3f);

        rotating = false;
    }

    public void Tutorial(bool playSound = true)
    {
        if (rotating)
            return;

        if (playSound)
            AudioManager.Instance.PlayUIButtonSfx();

        tutorialScreen.SetActive(!tutorialScreen.activeSelf);
        ToggleStun(tutorialScreen.activeSelf);
    }

    private void Roll() // Run in Update
    {
        if (!myCol.enabled) // If stunned, pause rolling but don't change the cached currentRollSpeed so it'll resume correctly
            return;

        if (isGrounded) // Roll on ground
        {
            float radius = transform.localScale.x / 2;
            float rotationAngle = rb.linearVelocityX * Time.deltaTime / radius;
            currentRollSpeed = rotationAngle * Mathf.Rad2Deg;
        }
        else // Aerial roll drag
            currentRollSpeed -= currentRollSpeed * aerialRollDrag * Time.deltaTime;

        transform.Rotate(0, 0, -currentRollSpeed);
    }
}