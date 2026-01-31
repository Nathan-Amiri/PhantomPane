using UnityEngine;
using System.Collections.Generic;
using static Player;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> slotSRs = new();
    [SerializeField] private List<SpriteRenderer> paneBackgrounds = new();


    private readonly List<Vector2> slotPositions = new();

    private readonly List<PaneColor> storedPanes = new();

    private Vector2 mousePosition;

    private int draggingSlot = -1; // -1 = not dragging

    private Player player;

    private readonly List<PaneColor> gridColors = new(); // The current colors of the grid sections
    private int lastGridSection; // The last grid section that the mouse was in while dragging

    private bool wasOverGridLastFrame; // I CANNOT figure out how to express in words what this does, I'm way too tired for this, it works though

    [SerializeField] private Color32 red = new(208, 70, 60, 255);
    [SerializeField] private Color32 blue = new(122, 164, 203, 255);
    [SerializeField] private Color32 green = new(153, 255, 102, 255);

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

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

    private void Update()
    {
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

            SetPaneSortingPlaced(currentGridSection);

            // If dragging to a new grid section, reset the one you left
            if (currentGridSection != lastGridSection && lastGridSection != -1)
            {
                ChangePaneBackgroundColor(lastGridSection, gridColors[lastGridSection]);
                if (gridColors[lastGridSection] == PaneColor.Colorless)
                {
                    SetPaneSortingDefault(lastGridSection);
                }

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

                    if (gridColors[lastGridSection] == PaneColor.Colorless)
                    {
                        SetPaneSortingDefault(lastGridSection);
                    }
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

            slotSRs[draggingSlot].color = Color.black;

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
        Color newColor = paneColor == PaneColor.Colorless ? Color.black : GetColorFromEnum(paneColor);
        newColor.a = a;
        paneBackgrounds[paneBackgroundIndex].color = newColor;
    }


    // Helper methods
    private Color GetColorFromEnum(PaneColor color)
    {
        if (color == PaneColor.Red)
            return red;
        else if (color == PaneColor.Blue)
            return blue;
        else if (color == PaneColor.Green)
            return green;
        else
            return Color.white;
    }

    private bool CheckIfPositionIsInBox(Vector2 position)
    {
        return !(position.x > 6 || position.x < -6 || position.y > 6 || position.y < -6);
    }

    private void SetPaneSortingPlaced(int index)
    {
        paneBackgrounds[index].sortingLayerName = "PlacedPanes";
    }

    private void SetPaneSortingDefault(int index)
    {
        paneBackgrounds[index].sortingLayerName = "Default";
    }

}