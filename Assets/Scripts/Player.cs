using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum PaneColor { Colorless, Red, Blue, Green };

    // PREFAB REFERENCE:
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D myCol;
    [SerializeField] private CircleCollider2D groundCheckCol;

    // SCENE REFERENCE:
    [SerializeField] private Transform playZone;

    // CONSTANT:
    public float moveSpeed = 8;
    public float jumpForce = 15.5f;
    public float doubleJumpForce = 15.5f;
    //private readonly float dashForce = 15;
    private readonly float fallMultiplier = 3; // Fastfall
    //private readonly float diagonalDashMultiplier = 2;
    //private readonly float horizontalDashMultiplier = 4.5f;
    //private readonly float verticalDashMultiplier = 1.05f;
    public float bounceForce = 35;
    public float horizontalBounceIncrease = 4;

    public float teleportDistance = 3;

    // DYNAMIC:
    private Vector2 moveInput;
    private bool jumpInput;
    private int jumpCount;
    //private bool dashInput;
    //private bool hasDash;

    static  int currentScene = 1;

    private void Update()
    {
        if (transform.position.x > 5.6f)
            transform.position = new(5.6f, transform.position.y);
        if (transform.position.x < -5.6f)
            transform.position = new(-5.6f, transform.position.y);
        if (transform.position.y > 5.6f)
            transform.position = new(transform.position.x, 5.6f);
        if (transform.position.y < -5.6f)
            transform.position = new(transform.position.x, -5.6f);

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetButtonDown("Jump"))
            jumpInput = true;
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

            if (jumpCount > 0)
            {
                jumpCount -= 1;
                rb.linearVelocity = new(rb.linearVelocity.x, 0);
                float newForce = jumpCount == 1 ? jumpForce : doubleJumpForce;
                rb.AddForce(Vector2.up * newForce, ForceMode2D.Impulse);
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


    public void OnGroundCheckEnter(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
            jumpCount = 2;
    }

    public void Die() // Called by Spikes
    {
        sr.enabled = false;
        ToggleStun(true);
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(Restart), 1);
    }
    public void Win()
    {
        currentScene += 1;

        sr.enabled = false;
        ToggleStun(true);
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(Restart), 1f);
    }

    public void Restart()
    {
        // Must reset static int BEFORE going to new scene, otherwise stuff will be messed up during Awake of new scene
        PaneNumberFinder.rotationSteps = 0;

        print( "Loading scene: " + currentScene);
        SceneManager.LoadScene(currentScene);
    }

    public void ToggleStun(bool on)
    {
        rb.constraints = on? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
        myCol.enabled = !on;
        groundCheckCol.enabled = !on;
    }

    public void Bouncer(Vector2 bounceDirection)
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 bounceVelocity = bounceForce * bounceDirection;
        bounceVelocity.x *= horizontalBounceIncrease;
        rb.AddForce(bounceVelocity, ForceMode2D.Impulse);
        jumpCount = 2;
    }

    public void Teleporter(Vector2 teleportDirection)
    {
        rb.linearVelocity = Vector2.zero;

        transform.position += (Vector3)teleportDirection * teleportDistance;
    }

    public void Rotator(bool rotateClockwise)
    {
        float rotation = rotateClockwise ? 90 : -90;
        playZone.transform.rotation *= Quaternion.Euler(0, 0, rotation);
        PaneNumberFinder.Rotate(rotateClockwise);
    }
}