using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Wheel Settings")]
    [SerializeField] private Transform wheelVisual;
    [SerializeField] private float maxAngularVelocity = 360f;
    [SerializeField] private float scrollForce = 150f;
    [SerializeField] private float friction = 15f;
    [SerializeField] private float airFriction = 3f;

    [Header("Helicopter Hat Settings")]
    [SerializeField] private float helicopterUpForce = 8f;
    [SerializeField] private float helicopterDownForce = 3f;
    [SerializeField] private float maxHelicopterVelocity = 10f;

    [Header("Ground & Slope Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.65f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float slopeSlideForce = 8f;
    [SerializeField] private float maxSlopeAngle = 60f;

    private Rigidbody2D rb;
    private float angularVelocity;
    private bool isGrounded;
    private float wheelRadius = 0.5f;
    private bool isHelicopterActive;

    // Slope data
    private Vector2 groundNormal = Vector2.up;
    private float currentSlopeAngle;
    private bool onSlope;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (wheelVisual != null)
        {
            SpriteRenderer wheelRenderer = wheelVisual.GetComponent<SpriteRenderer>();
            wheelRadius = wheelRenderer != null
                ? wheelRenderer.bounds.extents.x
                : wheelVisual.localScale.x * 0.5f;
        }
    }

    void Update()
    {
        HandleScrollInput();
        HandleHelicopterInput();
    }

    void LateUpdate()
    {
        RotateWheel();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
        ApplyFriction();
        ApplySlopeGravity();
    }

    // ── Input ──

    void HandleScrollInput()
    {
        float scrollDelta = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            angularVelocity += scrollDelta * scrollForce;
            angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);
        }
    }

    void HandleHelicopterInput()
    {
        isHelicopterActive = Input.GetKey(KeyCode.Space);

        if (isHelicopterActive)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.MoveTowards(rb.linearVelocity.y, maxHelicopterVelocity, helicopterUpForce * Time.deltaTime)
            );
        }
        else if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.MoveTowards(rb.linearVelocity.y, -helicopterDownForce, helicopterDownForce * Time.deltaTime)
            );
        }
    }

    // ── Ground Detection with Slope Normal ──

    void CheckGround()
    {
        if (groundCheck == null) return;

        // Use OverlapCircle at the wheel's center with the wheel's radius
        // This is physically accurate — the full contact patch of the wheel checks the ground
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        isGrounded = hit != null;

        if (isGrounded)
        {
            // Cast a single ray downward from wheel center to get the surface normal
            RaycastHit2D normalHit = Physics2D.Raycast(groundCheck.position, Vector2.down, wheelRadius + 0.1f, groundLayer);

            if (normalHit.collider != null)
            {
                groundNormal = normalHit.normal;
            }
            else
            {
                groundNormal = Vector2.up;
            }

            currentSlopeAngle = Vector2.SignedAngle(groundNormal, Vector2.up);
            onSlope = Mathf.Abs(currentSlopeAngle) > 1f;

            // Clamp slope angle — too steep = not grounded
            if (Mathf.Abs(currentSlopeAngle) > maxSlopeAngle)
            {
                isGrounded = false;
                onSlope = false;
                groundNormal = Vector2.up;
            }
        }
        else
        {
            groundNormal = Vector2.up;
            currentSlopeAngle = 0f;
            onSlope = false;
        }
    }

    // ── Movement ──

    void ApplyMovement()
    {
        // Convert angular velocity (deg/s) to tangential linear velocity
        float linearSpeed = (angularVelocity * Mathf.Deg2Rad) * wheelRadius;

        if (isGrounded && !isHelicopterActive)
        {
            // On ground: move along the slope tangent (perpendicular to ground normal)
            Vector2 slopeTangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
            Vector2 velocityAlongSlope = slopeTangent * linearSpeed;

            rb.linearVelocity = new Vector2(velocityAlongSlope.x, velocityAlongSlope.y);
        }
        else
        {
            // In air or helicopter mode: standard horizontal/vertical
            rb.linearVelocity = new Vector2(linearSpeed, rb.linearVelocity.y);
        }
    }

    // ── Friction ──

    void ApplyFriction()
    {
        float frictionAmount = isGrounded ? friction : airFriction;

        if (Mathf.Abs(angularVelocity) > 0.1f)
        {
            angularVelocity = Mathf.MoveTowards(angularVelocity, 0f, frictionAmount * Time.deltaTime);
        }
        else
        {
            angularVelocity = 0f;
        }
    }

    // ── Slope Gravity (natural slide when unpowered) ──

    void ApplySlopeGravity()
    {
        //To be Reviewed
    }

    // ── Visual ──

    void RotateWheel()
    {
        if (wheelVisual == null) return;

        wheelVisual.Rotate(0f, 0f, -angularVelocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        // Ground check circle at wheel radius
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Ground normal
        if (isGrounded)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(groundCheck.position, groundNormal * 0.5f);

            // Slope tangent (movement direction)
            Vector2 slopeTangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
            Gizmos.color = Color.white;
            Gizmos.DrawRay(groundCheck.position, slopeTangent * 0.5f);
        }
    }
}