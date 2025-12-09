using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemListManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCardPrefab; // ItemCard 프리펩
    [SerializeField] private Transform shootingActionContent;
    [SerializeField] private Transform shootingCostumeContent;
    [SerializeField] private Transform runActionContent;
    [SerializeField] private Transform runCostumeContent;
    [SerializeField] private Transform puzzleActionContent;
    [SerializeField] private Transform puzzleCostumeContent;

    // 임시 아이템 데이터 구조
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public Sprite itemIcon;
        public bool isOwned;
        public string gameName;      // "Shooting", "Run", "Puzzle"
        public bool isActionType;    // true = Action, false = Costume
    }

    public List<ItemData> allItems;  // 전체 아이템 리스트

    private string currentGame = "Shooting";
    private bool showAction = true;

    void Start()
    {
        RefreshItemList();
    }

    public void SetGame(string game)
    {
        currentGame = game;
        RefreshItemList();
    }

    public void SetType(bool isAction)
    {
        showAction = isAction;
        RefreshItemList();
    }

    public void RefreshItemList()
    {
        Transform targetContent = null;

        // [1] 어떤 콘텐츠에 넣을지 결정
        if (currentGame == "Shooting")
        {
            targetContent = showAction ? shootingActionContent : shootingCostumeContent;
        }
        else if (currentGame == "Run")
        {
            targetContent = showAction ? runActionContent : runCostumeContent;
        }
        else if (currentGame == "Puzzle")
        {
            targetContent = showAction ? puzzleActionContent : puzzleCostumeContent;
        }

        // [2] targetContent가 null이면 경고 후 리턴
        if (targetContent == null)
        {
            Debug.LogWarning("타겟 Content가 지정되지 않았습니다.");
            return;
        }

        targetContent.gameObject.SetActive(true); // 비활성 상태 대응

        // [3] 이전 프리팹 제거
        foreach (Transform child in targetContent)
        {
            Destroy(child.gameObject);
        }

        // [4] 조건에 맞는 아이템 필터링해서 프리팹 생성
        foreach (var item in allItems)
        {
            if (item.gameName != currentGame)
                continue;
            if (item.isActionType != showAction)
                continue;

            Debug.Log($"Generating: {item.itemName} in {targetContent.name}");

            GameObject newItem = Instantiate(itemCardPrefab, targetContent);

            newItem.transform.Find("ItemImage").GetComponent<Image>().sprite = item.itemIcon;
            newItem.transform.Find("ItemText").GetComponent<Text>().text = item.itemName;

            var button = newItem.transform.Find("UseButton").GetComponent<Button>();
            button.interactable = item.isOwned;
            button.GetComponentInChildren<Text>().text = showAction ? "Use" : "Apply";
        }
    }
}
