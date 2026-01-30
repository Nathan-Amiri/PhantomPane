using UnityEngine;

public class PanePickup : MonoBehaviour
{
    public Player.PaneColor paneColor;

    private Inventory inventory;

    // Floating variables
    public float floatAmplitude = 0.2f; // how far it moves up/down
    public float floatSpeed = 2f;       // how fast it moves
    private Vector3 startPos;

    private void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        startPos = transform.position; // save original position
    }

    private void Update()
    {
        FloatObject();
    }

    private void FloatObject()
    {
        // sine wave movement
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            inventory.PickupPane(paneColor);

            if (TryGetComponent(out LevelElement levelElement))
            {
                int pane = PaneNumberFinder.GetPaneNumber(transform.position);
                LevelElement.levelElements[pane].Remove(levelElement);
            }

            Destroy(gameObject);
        }
    }
}
