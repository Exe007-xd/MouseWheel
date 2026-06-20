public interface IInteractable
{
    /// <summary>
    /// Idle passive effect to show the player this is an interactable object (e.g. gentle bobbing, pulse, etc.)
    /// </summary>
    void PassiveEffect();

    /// <summary>
    /// Highlight effect when the player hovers their mouse over this object's sprite
    /// </summary>
    void HoverEffect();

    /// <summary>
    /// Called when the player clicks on the hovered object.
    /// Should slow time and allow the player to rotate the object by scrolling.
    /// </summary>
    void OnInteract();

    /// <summary>
    /// Called when the interaction window ends.
    /// Rotates the object to the player's desired rotation.
    /// </summary>
    void OnInteractComplete();
}