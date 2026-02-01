using UnityEngine;

public class Float : MonoBehaviour
{
    private readonly float floatAmplitude = 0.1f; // how far it moves up/down
    private readonly float floatSpeed = 2f;       // how fast it moves
    private Vector3 startPos;

    private bool rotating;

    private void OnEnable()
    {
        Player.OnRotateStartStop += RotationStartStop;
    }
    private void OnDisable()
    {
        Player.OnRotateStartStop -= RotationStartStop;
    }

    private void Start()
    {
        startPos = transform.position; // save original position
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity; // Stay upright when rotating

        if (!rotating)
        {
            // sine wave movement
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

            transform.position = new(transform.position.x, newY);
        }
    }

    private void RotationStartStop(bool start)
    {
        rotating = start;

        if (start)
            transform.position = startPos;
        else
            startPos = transform.position;
    }
}