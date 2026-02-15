using UnityEngine;
using UnityEngine.UI;

public class ScreenWipe : MonoBehaviour
{
    //public static bool titleScreen = true; // True when title screen is displayed at the start of the game
    public GameObject titleScreen;

    public Image image;
    public Color deathColor;
    public float wipeSpeed;

    private bool wiping;

    private bool initialWipe = true;

    public static bool justDied;

    private void Start()
    {
        if (titleScreen != null && titleScreen.activeSelf)
            return;

        transform.localPosition = Vector3.zero;
        StartWipe(justDied);
    }

    public void StartWipe(bool deathWipe, bool initial = false)
    {
        if (initial)
        {
            initialWipe = true;
            transform.localPosition = Vector3.zero;
        }

        AudioManager.Instance.PlayScreenWipeSfx();

        justDied = deathWipe;

        image.color = deathWipe ? deathColor : Player.worldBackgroundColor;

        wiping = true;
    }

    private void Update()
    {
        if (wiping)
        {
            transform.Translate(Time.deltaTime * wipeSpeed * Vector2.left);

            if (transform.localPosition.x < (initialWipe ? -2500 : 0))
            {
                wiping = false;

                if (initialWipe)
                {
                    initialWipe = false;
                    transform.localPosition = new(2500, 0);
                }
            }
        }
    }
}