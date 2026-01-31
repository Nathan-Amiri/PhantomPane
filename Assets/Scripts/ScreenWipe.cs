using UnityEngine;
using UnityEngine.UI;

public class ScreenWipe : MonoBehaviour
{
    public Image image;
    public Color winColor;
    public Color deathColor;
    public float wipeSpeed;

    private bool wiping;

    private bool initialWipe = true;

    public static bool justDied;

    private void Start()
    {
        transform.localPosition = Vector3.zero;
        StartWipe(justDied);
    }

    public void StartWipe(bool deathWipe)
    {
        justDied = deathWipe;

        image.color = deathWipe ? deathColor : winColor;

        wiping = true;
    }

    private void Update()
    {
        if (wiping)
        {
            transform.Translate(Time.deltaTime * wipeSpeed * Vector2.left);

            if (transform.localPosition.x < (initialWipe ? -2000 : 0))
            {
                wiping = false;

                if (initialWipe)
                {
                    initialWipe = false;
                    transform.localPosition = new(2000, 0);
                }
            }
        }
    }
}