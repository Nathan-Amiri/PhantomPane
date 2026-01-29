using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PaneColor { Colorless, Red, Blue, Green };

    // PREFAB REFERENCE:
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;

    // CONSTANT:
    public float moveSpeed = 8;
    public float jumpForce = 15.5f;
    //private readonly float dashForce = 15;
    private readonly float fallMultiplier = 3; // Fastfall
    //private readonly float diagonalDashMultiplier = 2;
    //private readonly float horizontalDashMultiplier = 4.5f;
    //private readonly float verticalDashMultiplier = 1.05f;

    // DYNAMIC:
    private bool isGrounded;

    private bool isStunned;

    private Vector2 moveInput;
    private bool jumpInput;
    private bool hasJump;
    //private bool dashInput;
    //private bool hasDash;

    private int currentScene;

    private void Update()
    {
        if (transform.position.x > 5.5f)
            transform.position = new(5.5f, transform.position.y);
        if (transform.position.x < -5.5f)
            transform.position = new(-5.5f, transform.position.y);
        if (transform.position.y > 5.5f)
            transform.position = new(transform.position.x, 5.5f);
        if (transform.position.y < -5.5f)
            transform.position = new(transform.position.x, -5.5f);

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetButtonDown("Jump"))
            jumpInput = true;
    }

    private void FixedUpdate()
    {
        if (isStunned)
            return;

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

        if (isGrounded)
            hasJump = true;

        if (jumpInput)
        {
            jumpInput = false;

            if (hasJump)
            {
                hasJump = false;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        //if (dashInput)
        //{
        //    dashInput = false;

        //    if (hasDash)
        //    {
        //        hasDash = false;

        //        rb.linearVelocity = Vector2.zero;



        //        // Because of the movement code, gravity, and the nature of diagonal vs straight dashing, different directions need custom values
        //        Vector2 velocity = moveInput * dashForce;

        //        if (moveInput.x != 0 && moveInput.y != 0) // Diagonal dashes get a horizontal boost
        //            velocity.x *= diagonalDashMultiplier;
        //        else if (moveInput.x == 0) // Vertical dashes get a slight boost
        //            velocity *= verticalDashMultiplier;
        //        else if (moveInput.y == 0) // Horizontal dashes get a large boost
        //            velocity *= horizontalDashMultiplier;



        //        rb.AddForce(velocity, ForceMode2D.Impulse);
        //    }
        //}
    }


    public void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
            isGrounded = true;
    }
    public void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
            isGrounded = false;
    }

    public void Die() // Called by Spikes
    {
        sr.enabled = false;
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(Restart), 1);
    }
    public void Win()
    {
        currentScene += 1;

        sr.enabled = false;
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(Restart), 1);
    }

    private void Restart()
    {
        SceneManager.LoadScene(currentScene);
    }

    public void ToggleStun(bool on)
    {
        isStunned = on;
    }
}