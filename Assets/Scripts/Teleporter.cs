using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Vector2 direction;
    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            col.GetComponent<Player>().Teleporter(transform.position, direction);
    }
}