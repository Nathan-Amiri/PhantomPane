using UnityEngine;

public class MouseOverInteractable : MonoBehaviour
{
    public Inventory inventory;
    public void OnMouseOver()
    {
        inventory.ToggleMouseOverInteractable(true);
    }
    public void OnMouseExit()
    {
        inventory.ToggleMouseOverInteractable(false);
    }
}