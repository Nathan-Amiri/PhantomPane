using UnityEngine;

public class WorldColorMatch : MonoBehaviour
{
    public SpriteRenderer sr;
    public bool useWorldPaneColor;

    private void Start()
    {
        // Player updates with each new scene load in Awake prior to this
        sr.color = useWorldPaneColor ? Player.worldPaneColor : Player.worldBackgroundColor;
    }
}