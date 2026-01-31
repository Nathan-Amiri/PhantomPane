using System.Collections.Generic;
using UnityEngine;
using static Player;

public class LevelElement : MonoBehaviour
{
    public PaneColor paneColorThatRevealsMe;

    public static readonly List<List<LevelElement>> levelElements = new(); // List 0 = pane 0 elements, etc

    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        // Since there might be a delay on Start as all the level elements add themselves to the list before turning themselves off,
        // they might be visible for a split second at the start of the game. So to prevent any chance of the player seeing the solution at the beginning,
        // we hide all level elements in awake asap before any of the start code runs
        sr.enabled = false;

        // Clear static lists when loading a new scene
        foreach (List<LevelElement> levelElementList in levelElements)
            levelElementList.Clear();

        // Make a list of level elements, one list for each grid section
        if (levelElements.Count == 0) // Don't add to the static list on every level element at the start! Just once
            for (int i = 0; i < 9; i++)
                levelElements.Add(new());
    }

    private void Start()
    {
        int myPane = PaneNumberFinder.GetPaneNumber(transform.position);
        levelElements[myPane].Add(this);

        if (paneColorThatRevealsMe != PaneColor.Colorless)
            gameObject.SetActive(false);

        sr.enabled = true;
    }

    public void NewPane(PaneColor paneColor)
    {
        gameObject.SetActive(paneColor == paneColorThatRevealsMe);
    }
}