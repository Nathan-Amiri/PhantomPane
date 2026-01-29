using UnityEngine;

public class StoredPane : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private int slotNumber;

    public void OnMouseDown()
    {
        inventory.SelectPaneSlot(slotNumber);
    }
}