using UnityEngine;

public class PanePickup : MonoBehaviour
{
    public Player.PaneColor paneColor;

    private Inventory inventory;

    private void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            inventory.PickupPane(paneColor);
            Destroy(gameObject);
        }
    }
}