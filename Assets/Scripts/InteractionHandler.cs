using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 10f;

    [Header("Time Slow Settings")]
    [SerializeField] private float timeScaleDuringInteraction = 0.15f;
    [SerializeField] private float rotationSpeed = 120f;

    [Header("Interaction Window")]
    [SerializeField] private float maxInteractionDuration = 3f;

    private IInteractable currentInteractable;
    private float interactionTimer;
    private bool isInInteraction;
    private bool interactionCompleted;
    private Vector2 lastMousePosition;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (!isInInteraction)
        {
            // Normal mode — detect hover via raycast and handle click to start interaction
            HandleRaycastHoverDetection();

            if (Input.GetMouseButtonDown(0) && currentInteractable != null)
            {
                StartInteraction();
            }
        }
        else
        {
            // Interaction mode — handle rotation and timer
            HandleInteraction();
        }
    }

    void HandleRaycastHoverDetection()
    {
        Vector2 mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, interactRange);

        IInteractable hoveredInteractable = null;

        if (hit.collider != null)
        {
            hoveredInteractable = hit.collider.GetComponent<IInteractable>();
        }

        // If we changed hover target, update both
        if (hoveredInteractable != currentInteractable)
        {
            // Unhover previous
            if (currentInteractable != null)
            {
                (currentInteractable as MonoBehaviour)?.SendMessage("SetHovered", false, SendMessageOptions.DontRequireReceiver);
            }

            currentInteractable = hoveredInteractable;

            // Hover new
            if (currentInteractable != null)
            {
                (currentInteractable as MonoBehaviour)?.SendMessage("SetHovered", true, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void StartInteraction()
    {
        isInInteraction = true;
        interactionTimer = 0f;
        interactionCompleted = false;
        lastMousePosition = Input.mousePosition;

        // Slow time
        Time.timeScale = timeScaleDuringInteraction;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        currentInteractable.OnInteract();
    }

    void HandleInteraction()
    {
        interactionTimer += Time.unscaledDeltaTime;

        // Rotate object by dragging the mouse (like Unity Editor's rotation handle)
        if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            float mouseDeltaX = currentMousePos.x - lastMousePosition.x;
            lastMousePosition = currentMousePos;

            if (Mathf.Abs(mouseDeltaX) > 0.1f)
            {
                float rotationChange = mouseDeltaX * rotationSpeed * 0.1f;

                MonoBehaviour monoBehaviour = currentInteractable as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    Vector3 currentRot = monoBehaviour.transform.eulerAngles;
                    float newZ = currentRot.z + rotationChange;

                    monoBehaviour.SendMessage("SetTargetRotation", newZ, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // Release mouse button to complete
        if (Input.GetMouseButtonUp(0))
        {
            CompleteInteraction();
        }

        // Auto-complete if timer runs out
        if (interactionTimer >= maxInteractionDuration)
        {
            CompleteInteraction();
        }
    }

    void CompleteInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnInteractComplete();
        }

        isInInteraction = false;
        currentInteractable = null;
        interactionCompleted = true;

        // Restore time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnDestroy()
    {
        // Ensure time is restored if this object is destroyed mid-interaction
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}