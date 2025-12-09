using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoardManager : MonoBehaviour
{
    public InventoryManager inventoryManager;

    [Header("게임별 보드")]
    public GameObject boardShooting;
    public GameObject boardRun;
    public GameObject boardPuzzle;

    [Header("Shooting")]
    public GameObject shootingActionList;
    public GameObject shootingCostumeList;

    [Header("Run")]
    public GameObject runActionList;
    public GameObject runCostumeList;

    [Header("Puzzle")]
    public GameObject puzzleActionList;
    public GameObject puzzleCostumeList;

    private string currentGame = "Shooting";
    private string currentItemType = "Action";

    void Start()
    {
        ShowBoard("Shooting");
        ShowItemType("Action");
    }

    public void OnGameButtonClicked(string game)
    {
        if (currentGame != game)
        {
            ShowBoard(game);
            ShowItemType("Action");
        }
    }

    public void OnItemTypeButtonClicked(string itemType)
    {
        if (currentItemType != itemType)
        {
            ShowItemType(itemType);
        }
    }

    private void ShowBoard(string game)
    {
        boardShooting.SetActive(game == "Shooting");
        boardRun.SetActive(game == "Run");
        boardPuzzle.SetActive(game == "Puzzle");

        currentGame = game;
    }

    private void ShowItemType(string itemType)
    {
            
        if (currentGame == "Shooting")
            (itemType == "Action" ? (System.Action)inventoryManager.ShowShootingAction
                                  : inventoryManager.ShowShootingCostume)();
        else if (currentGame == "Run")
            (itemType == "Action" ? (System.Action)inventoryManager.ShowRunAction
                                  : inventoryManager.ShowRunCostume)();
        else if (currentGame == "Puzzle")
            (itemType == "Action" ? (System.Action)inventoryManager.ShowPuzzleAction
                                  : inventoryManager.ShowPuzzleCostume)();

        currentItemType = itemType;
    }
}
