using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class InteractableObstacles : MonoBehaviour, IInteractable
{
    [Header("Passive Effect")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseMinAlpha = 0.6f;

    [Header("Hover Effect")]
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private float hoverPulseSpeed = 2f;

    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 startPosition;
    private Color originalColor;
    private float hoverTimer;
    private bool isHovered;
    private bool isInteracting;


    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;
        originalColor = sr.color;
    }

    void Update()
    {
        if (isInteracting) return;

        PassiveEffect();

        if (isHovered)
        {
            HoverEffect();
        }
    }

    // ── IInteractable Implementation ──

    public void PassiveEffect()
    {
        // Gentle pulse on alpha
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(pulseMinAlpha, 1f, pulse);
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    public void HoverEffect()
    {
        // Brightness pulse
        float pulse = (Mathf.Sin(Time.time * hoverPulseSpeed) + 1f) * 0.5f;
        sr.color = Color.Lerp(originalColor * 0.7f, hoverColor, pulse);
    }

    public void OnInteract()
    {
        isInteracting = true;
    }

    public void OnInteractComplete()
    {
        isInteracting = false;
        isHovered = false;
        sr.color = originalColor;
    }

    // ── Called by InteractionHandler ──

    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        if (!hovered && !isInteracting)
        {
            sr.color = originalColor;
        }
    }

    public void SetTargetRotation(float rotationZ)
    {
        // Visually update rotation in real-time during interaction
        Vector3 rot = transform.eulerAngles;
        rot.z = rotationZ;
        transform.eulerAngles = rot;
    }

    public void ResetVisuals()
    {
        isInteracting = false;
        isHovered = false;
        sr.color = originalColor;
        transform.position = startPosition;
    }

    void OnMouseEnter()
    {
        if (!isInteracting)
            SetHovered(true);
    }

    void OnMouseExit()
    {
        SetHovered(false);
    }
}