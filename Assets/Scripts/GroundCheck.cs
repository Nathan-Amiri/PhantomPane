using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public Player player;

    private float offset;
    private void Awake()
    {
        offset = player.transform.position.y - transform.position.y;
        transform.parent = null;
    }
    public void OnTriggerStay2D(Collider2D col)
    {
        player.OnGroundCheckStay(col);
    }
    public void OnTriggerExit2D(Collider2D col)
    {
        player.OnGroundCheckExit(col);
    }

    private void Update()
    {
        transform.position = player.transform.position - new Vector3(0, offset);
    }
}