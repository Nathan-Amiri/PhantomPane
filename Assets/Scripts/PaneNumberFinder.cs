using UnityEngine;

public class PaneNumberFinder : MonoBehaviour
{
    public static int GetPaneNumber(Vector2 position)
    {
        if (position.y > 2)
        {
            if (position.x < -2)
                return 0;

            if (position.x < 2)
                return 1;

            return 2;
        }

        if (position.y > -2)
        {
            if (position.x < -2)
                return 3;

            if (position.x < 2)
                return 4;

            return 5;
        }

        if (position.x < -2)
            return 6;

        if (position.x < 2)
            return 7;

        return 8;
    }
}