using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlaneController2D : MonoBehaviour
{
    public int planeIndex;                 // Unique index for each plane
    public float forwardSpeed = 5f;        // Constant forward speed in the local X direction
    public float rotationSpeed = 200f;     // Rotation speed of the plane
    public float gravityForce = 9.81f;     // Gravity force to pull the plane downwards
    public Sprite destroyedSprite;         // Sprite to display when the plane is destroyed
    public AudioClip destroySound;         // Sound effect to play when the plane is destroyed
    public float bottomOffset = 0.5f;      // Offset from the bottom of the screen in world units

    private Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isDestroyed = false;
    private bool isControlEnabled = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        rb.gravityScale = 0; // Disable gravity at the start
        rb.velocity = Vector2.zero; // Set initial velocity to 0
    }

    private void Update()
    {
        if (isControlEnabled && !isDestroyed)
        {
            HandleRotation();
        }

        FlipYAxisBasedOnZRotation();
    }

    private void FixedUpdate()
    {
        if (isControlEnabled && !isDestroyed)
        {
            ApplyGravity();
        }
    }

    private void ApplyGravity()
    {
        rb.AddForce(Vector2.down * gravityForce);
    }

    private void HandleRotation()
    {
        float rotationInput = 0;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rotationInput = -1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rotationInput = 1;
        }

        transform.Rotate(0, 0, rotationInput * rotationSpeed * Time.deltaTime);

        // Maintain constant forward velocity
        rb.velocity = transform.right * forwardSpeed;
    }

    private void FlipYAxisBasedOnZRotation()
    {
        // Get the Z rotation angle
        float zAngle = transform.eulerAngles.z;

        // Normalize the angle to -180 to 180 range for easier comparison
        zAngle = (zAngle > 180) ? zAngle - 360 : zAngle;

        // Flip Y-axis based on Z angle
        if (zAngle > 90 || zAngle < -90)
        {
            // Flip Y-axis to 180 degrees
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
        else
        {
            // Reset Y-axis to 0 degrees
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }

    public void EnableControl(bool enable)
    {
        isControlEnabled = enable;
        if (!enable)
        {
            // Stop movement when control is disabled
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }
        else
        {
            // Activate movement when control is enabled
            rb.velocity = transform.right * forwardSpeed;
            rb.gravityScale = 1; // Enable gravity
        }
    }

    private void OnBecameInvisible()
    {
        if (!isDestroyed && isControlEnabled)
        {
            TriggerDestruction();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDestroyed && collision.collider.CompareTag("munition"))
        {
            TriggerDestruction();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDestroyed && other.CompareTag("munition"))
        {
            TriggerDestruction();
        }
    }

    private void TriggerDestruction()
    {
        isDestroyed = true;

        // Replace the sprite with the destroyed version
        if (destroyedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = destroyedSprite;
        }

        // Play destruction sound
        if (destroySound != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        rb.velocity = Vector2.zero;
        rb.gravityScale = 2f;

        Destroy(gameObject, 3f);

        // Notify PlaneManager to activate the next plane
        PlaneManager.Instance.ActivateNextPlane();
    }
}
