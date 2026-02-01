using UnityEngine;

public class Float : MonoBehaviour
{
    private readonly float floatAmplitude = 0.1f; // how far it moves up/down
    private readonly float floatSpeed = 2f;       // how fast it moves
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position; // save original position
    }

    private void Update()
    {
        // sine wave movement
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}