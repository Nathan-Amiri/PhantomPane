using UnityEngine;
using System.Collections.Generic;
using static Player;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> slotSRs = new();
    [SerializeField] private List<SpriteRenderer> paneBackgrounds = new();

    [SerializeField] Texture2D defaultCursorTexture;
    [SerializeField] Texture2D handCursorTexture;

    private readonly List<Vector2> slotPositions = new();

    private readonly List<PaneColor> storedPanes = new();

    private Vector2 mousePosition;

    private int draggingSlot = -1; // -1 = not dragging

    [SerializeField] private Player player;

    private readonly List<PaneColor> gridColors = new(); // The current colors of the grid sections
    private int lastGridSection; // The last grid section that the mouse was in while dragging

    private bool wasOverGridLastFrame; // I CANNOT figure out how to express in words what this does, I'm way too tired for this, it works though

    [SerializeField] private Color redPaneColor;
    [SerializeField] private Color bluePaneColor;
    [SerializeField] private Color purplePaneColor;

    [SerializeField] private GameObject tutorialScreen;

    private void Awake()
    {
        for (int i = 0; i < 6; i++)
        {
            storedPanes.Add(PaneColor.Colorless);

            slotPositions.Add(slotSRs[i].transform.position);
        }

        for (int i = 0; i < 9; i++)
            gridColors.Add(PaneColor.Colorless);
    }

    public void PickupPane(PaneColor paneColor)
    {
        for (int i = 0; i < storedPanes.Count; i++)
        {
            if (storedPanes[i] == PaneColor.Colorless) // Colorless panes don't exist, so Colorless means an empty inventory slot
            {
                storedPanes[i] = paneColor;

                slotSRs[i].color = GetColorFromEnum(paneColor); // Will never be colorless

                return;
            }
        }
    }

    private bool defaultCursor = true;
    private bool mouseOverInteractable;
    public void ToggleMouseOverInteractable(bool over, int paneNumber = -1)
    {
        if (paneNumber != -1 && storedPanes[paneNumber] == PaneColor.Colorless)
            return;
        mouseOverInteractable = over;
    }
    private void Update()
    {
        if ((mouseOverInteractable || draggingSlot != -1) && defaultCursor)
        {
            defaultCursor = false;

            Cursor.SetCursor(handCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        if (!mouseOverInteractable && draggingSlot == -1 && !defaultCursor)
        {
            defaultCursor = true;

            Cursor.SetCursor(defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        }

        Vector3 tempMousePosition = Input.mousePosition;
        tempMousePosition.z = -Camera.main.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(tempMousePosition);

        if (draggingSlot != -1)
        {
            if (Input.GetMouseButtonUp(0)) // Only the left mouse button can click buttons by default, I actually didn't know this until testing just now!
            {
                ReleasePaneSlot();
                return;
            }

            DraggingPaneSlot();
        }
    }



    // Drag and drop methods
    public void SelectPaneSlot(int slotNumber)
    {
        if (tutorialScreen.activeSelf) // Since pane slots can be selected through UI fog objects
            return;

        if (player.rotating)
            return;

        if (storedPanes[slotNumber] == PaneColor.Colorless)
            return;

        player.ToggleStun(true);

        draggingSlot = slotNumber;
    }
    private void DraggingPaneSlot() // Run in Update
    {
        if (CheckIfPositionIsInBox(mousePosition))
        {
            wasOverGridLastFrame = true;

            slotSRs[draggingSlot].enabled = false;
            int currentGridSection = PaneNumberFinder.GetPaneNumber(mousePosition);

            // If dragging to a new grid section, reset the one you left
            if (currentGridSection != lastGridSection && lastGridSection != -1)
            {
                ChangePaneBackgroundColor(lastGridSection, gridColors[lastGridSection]);

                foreach (LevelElement levelElement in LevelElement.levelElements[lastGridSection])
                    levelElement.NewPane(gridColors[lastGridSection]);
            }

            lastGridSection = currentGridSection;

            foreach (LevelElement levelElement in LevelElement.levelElements[currentGridSection])
                levelElement.NewPane(storedPanes[draggingSlot]);

            ChangePaneBackgroundColor(currentGridSection, storedPanes[draggingSlot]);
        }
        else
        {
            if (wasOverGridLastFrame) // So it only happens once
            {
                wasOverGridLastFrame = false;

                if (lastGridSection != -1)
                {
                    foreach (LevelElement levelElement in LevelElement.levelElements[lastGridSection])
                        levelElement.NewPane(gridColors[lastGridSection]);

                    ChangePaneBackgroundColor(lastGridSection, gridColors[lastGridSection]);
                }

                slotSRs[draggingSlot].enabled = true;
            }

            slotSRs[draggingSlot].transform.position = mousePosition;
        }
    }
    private void ReleasePaneSlot()
    {
        if (CheckIfPositionIsInBox(mousePosition))
        {
            gridColors[PaneNumberFinder.GetPaneNumber(mousePosition)] = storedPanes[draggingSlot];

            storedPanes[draggingSlot] = PaneColor.Colorless;

            slotSRs[draggingSlot].color = worldBackgroundColor;

            slotSRs[draggingSlot].enabled = true;
        }

        slotSRs[draggingSlot].transform.position = slotPositions[draggingSlot];

        draggingSlot = -1;
        lastGridSection = -1;
        wasOverGridLastFrame = false;

        player.ToggleStun(false);
    }

    private void ChangePaneBackgroundColor(int paneBackgroundIndex, PaneColor paneColor)
    {
        float a = paneBackgrounds[paneBackgroundIndex].color.a;
        Color newColor = GetColorFromEnum(paneColor);
        newColor.a = a;
        paneBackgrounds[paneBackgroundIndex].color = newColor;
    }


    // Helper methods
    private Color GetColorFromEnum(PaneColor color)
    {
        if (color == PaneColor.Red)
            return redPaneColor;
        else if (color == PaneColor.Blue)
            return bluePaneColor;
        else if (color == PaneColor.Purple)
            return purplePaneColor;
        else
            return worldPaneColor;
    }

    private bool CheckIfPositionIsInBox(Vector2 position)
    {
        return !(position.x > 6 || position.x < -6 || position.y > 6 || position.y < -6);
    }
}