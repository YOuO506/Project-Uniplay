using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


// 응답 DTO


public class InventoryManager : MonoBehaviour
{
    [Header("Coin UI")]
    public Text myCoinText;

    // ───────────────────────────────── Boards (게임별 큰 판넬) ─────────────────────────────────
    [Header("Board Roots (Game tabs)")]
    public GameObject boardShootingRoot;   // ItemBoard_Shooting
    public GameObject boardRunRoot;        // ItemBoard_Run
    public GameObject boardPuzzleRoot;     // ItemBoard_Puzzle

    // ──────────────────────────────── List Roots ────────────────────────────────
    [Header("List Roots (toggle these)")]
    public GameObject shootingActionListRoot;     // ItemBoard_Shooting/Shooting_ActionList
    public GameObject shootingCostumeListRoot;    // ItemBoard_Shooting/Shooting_CostumeList
    public GameObject runActionListRoot;          // ItemBoard_Run/Run_ActionList
    public GameObject runCostumeListRoot;         // ItemBoard_Run/Run_CostumeList
    public GameObject puzzleActionListRoot;       // ItemBoard_Puzzle/Puzzle_ActionList
    public GameObject puzzleCostumeListRoot;      // ItemBoard_Puzzle/Puzzle_CostumeList

    // ──────────────────────────────── Content (카드 붙일 부모) ────────────────────────────────
    // ※ 여기에는 **각 스크롤뷰의 Content(…/Viewport/Content)** 를 Drag&Drop.
    [Header("Contents (instantiate under these)")]
    public Transform shootingActionContent;
    public Transform shootingCostumeContent;
    public Transform runActionContent;
    public Transform runCostumeContent;
    public Transform puzzleActionContent;
    public Transform puzzleCostumeContent;

    // ───────────────────────────────────── API/Prefab ─────────────────────────────────────
    [Header("API")]
    [Tooltip("http://localhost:8080/Api/InventoryApi.aspx | https://localhost:44357/Api/InventoryApi.aspx")]
    public string inventoryApiUrl;

    [Header("UI")]
    public GameObject inventoryPanel;       // 인벤토리 전체 패널
    public GameObject itemPrefab;           // 카드 프리팹

    private bool _busy = false;

    private void Awake()
    {
        // 실행 시 ApiConfig에서 자동 세팅
        inventoryApiUrl = ApiConfig.BaseUrlApi + "InventoryApi.aspx";
    }

    // 서버 DTO
    [System.Serializable]
    public class ItemDto
    {
        public int GameID;
        public int ItemID;
        public string ItemName;
        public int ItemTypeID;  // 1=Action, 2=Costume
        public bool IsOwned;
        public bool IsEquipped;
        public string ImagePath;
    }

    [System.Serializable]
    public class InventoryRes
    {
        public bool success;
        public ItemDto[] items;
    }

    // ───────────────────────────────── API: 목록 로드 ─────────────────────────────────
    public void LoadInventory(int gameId)
    {
        if (_busy) return;
        if (!GameSession.IsLoggedIn) { Debug.LogWarning("로그인 먼저 하세요."); return; }
        ClearAllUI();   // 로그인 안 되어 있으면 UI 비움
        if (inventoryPanel) inventoryPanel.SetActive(false);
        RefreshCoinUI();
        StartCoroutine(CoLoadInventory(GameSession.UserId, gameId));
    }

    private IEnumerator CoLoadInventory(string userId, int gameId)
    {
        _busy = true;

        string url = $"{inventoryApiUrl}?mode=list&userId={userId}&gameId={gameId}";
        using (var req = UnityWebRequest.Get(url))
        {
            req.certificateHandler = new DevOnly_BypassCertificate();
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("인벤토리 API 요청 실패: " + req.error);
                _busy = false; yield break;
            }

            var res = JsonUtility.FromJson<InventoryRes>(req.downloadHandler.text);
            if (res == null || !res.success || res.items == null)
            {
                Debug.LogError("인벤토리 응답 에러: " + req.downloadHandler.text);
                _busy = false; yield break;
            }

            RefreshUI(res.items, gameId);
        }

        _busy = false;
    }

    // ───────────────────────────────── API: 장착 ─────────────────────────────────
    private IEnumerator CoEquip(int gameId, int itemId, int itemTypeId)
    {
        string url = inventoryApiUrl + "?mode=equip"
                   + "&userId=" + GameSession.UserId
                   + "&gameId=" + gameId
                   + "&itemId=" + itemId;

        using (var req = UnityWebRequest.Get(url))
        {
            req.certificateHandler = new DevOnly_BypassCertificate();
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("equip 실패: " + req.error);
                yield break;
            }
        }
        // 다시 로드해서 UI 반영
        yield return CoLoadInventory(GameSession.UserId, gameId);

        // 게임별 PlayerPrefs 저장
        if (gameId == 1) // Shooting
        {
            if (itemTypeId == 1) PlayerPrefs.SetInt("UsingShootingAction", itemId);
            else if (itemTypeId == 2) PlayerPrefs.SetInt("UsingShootingCostume", itemId); ;
        }
        else if (gameId == 2) // Run
        {
            if (itemTypeId == 1) PlayerPrefs.SetInt("UsingRunAction", itemId);
            else if (itemTypeId == 2) PlayerPrefs.SetInt("UsingRunCostume", itemId);
        }
        else if (gameId == 3) // Puzzle
        {
            if (itemTypeId == 1) PlayerPrefs.SetInt("UsingPuzzleAction", itemId);
            else if (itemTypeId == 2) PlayerPrefs.SetInt("UsingPuzzleCostume", itemId);
        }
        PlayerPrefs.Save();
    }

    // ───────────────────────────────── UI 갱신 ─────────────────────────────────
    private void RefreshUI(ItemDto[] items, int gameId)
    {
        // 1) 기존 카드 제거 (모든 Content)
        ClearChildren(shootingActionContent);
        ClearChildren(shootingCostumeContent);
        ClearChildren(runActionContent);
        ClearChildren(runCostumeContent);
        ClearChildren(puzzleActionContent);
        ClearChildren(puzzleCostumeContent);

        // Run 게임에서 현재 장착된 아이템 추적
        int equippedRunItemId = -1;

        // 2) 아이템 카드 생성
        foreach (var item in items)
        {
            // 소유하지 않은 아이템은 MyItem에 표시하지 않음
            if (!item.IsOwned)
                continue;

            Debug.Log($"[ITEM] {item.ItemName}, path={item.ImagePath}");

            Transform parent = null;
            if (item.GameID == 1) parent = (item.ItemTypeID == 1) ? shootingActionContent : shootingCostumeContent;
            else if (item.GameID == 2) parent = (item.ItemTypeID == 1) ? runActionContent : runCostumeContent;
            else if (item.GameID == 3) parent = (item.ItemTypeID == 1) ? puzzleActionContent : puzzleCostumeContent;

            if (!parent) continue;

            // 레이아웃 깨짐 방지: worldPositionStays=false, localScale=1
            var go = Instantiate(itemPrefab, parent, false);
            var rt = go.GetComponent<RectTransform>();
            if (rt) rt.localScale = Vector3.one;

            // 이름
            var nameText = go.transform.Find("ItemText")?.GetComponent<Text>();
            if (nameText) nameText.text = item.ItemName + (item.IsEquipped ? " (Using)" : "");

            // 이미지
            var img = go.transform.Find("ItemImage")?.GetComponent<Image>();
            if (img && !string.IsNullOrEmpty(item.ImagePath))
            {
                string fullUrl = ApiConfig.BaseUrlWeb + item.ImagePath;
                Debug.Log("[IMAGE URL] " + fullUrl);   // 콘솔 출력
                StartCoroutine(LoadImage(fullUrl, img));
            }
            // 버튼
            var useBtn = go.transform.Find("UseButton")?.GetComponent<Button>();
            var useBtnText = go.transform.Find("UseButton/Item Text")?.GetComponent<Text>();
            if (useBtn)
            {
                if (item.IsEquipped)
                {
                    useBtn.interactable = false;
                    if (useBtnText) useBtnText.text = "Using";
                }
                else
                {
                    if (item.IsOwned)   // 소유한 경우만 Use 가능
                    {
                        useBtn.interactable = true;
                        if (useBtnText) useBtnText.text = "Use";
                        useBtn.onClick.RemoveAllListeners();
                        useBtn.onClick.AddListener(() => StartCoroutine(CoEquip(item.GameID, item.ItemID, item.ItemTypeID)));
                    }
                    else
                    {
                        useBtn.interactable = false;
                        if (useBtnText) useBtnText.text = "Locked";
                    }
                }
            }

            RefreshCoinUI();
        }

        // Run 장착 코스튬을 PlayerPrefs에 반영
        if (equippedRunItemId > 0)
        {
            PlayerPrefs.SetInt("UsingRunCostume", equippedRunItemId);
            PlayerPrefs.Save();
        }

        if (inventoryPanel) inventoryPanel.SetActive(true);

        // 3) 기본 탭 표시: 게임 보드 On + Action 리스트만 On (리스트 루트만 토글)
        //ShowBoard(gameId);
        //if (gameId == 1) SetActiveLists(true, false, false, false, false, false);
        //else if (gameId == 2) SetActiveLists(false, false, true, false, false, false);
        //else if (gameId == 3) SetActiveLists(false, false, false, false, true, false);
    }

    public void RefreshCoinUI()
    {
        if (myCoinText != null)
            myCoinText.text = GameSession.IsLoggedIn ? GameSession.Coins.ToString() : "0"; // 로그인 아니면 0
    }

    // ───────────────────────────── 탭 토글(게임별/타입별) ─────────────────────────────
    public void OnClickShooting() { ShowBoard(1); LoadInventory(1); ShowShootingAction(); }
    public void OnClickRun() { ShowBoard(2); LoadInventory(2); ShowRunAction(); }
    public void OnClickPuzzle() { ShowBoard(3); LoadInventory(3); ShowPuzzleAction(); }

    public void ShowShootingAction() { ShowBoard(1); SetActiveLists(true, false, false, false, false, false); }
    public void ShowShootingCostume() { ShowBoard(1); SetActiveLists(false, true, false, false, false, false); }
    public void ShowRunAction() { ShowBoard(2); SetActiveLists(false, false, true, false, false, false); }
    public void ShowRunCostume() { ShowBoard(2); SetActiveLists(false, false, false, true, false, false); }
    public void ShowPuzzleAction() { ShowBoard(3); SetActiveLists(false, false, false, false, true, false); }
    public void ShowPuzzleCostume() { ShowBoard(3); SetActiveLists(false, false, false, false, false, true); }

    public void ShowBoard(int gameId)
    {
        if (boardShootingRoot) boardShootingRoot.SetActive(gameId == 1);
        if (boardRunRoot) boardRunRoot.SetActive(gameId == 2);
        if (boardPuzzleRoot) boardPuzzleRoot.SetActive(gameId == 3);
    }

    private void SetActiveLists(bool sA, bool sC, bool rA, bool rC, bool pA, bool pC)
    {
        if (shootingActionListRoot) shootingActionListRoot.SetActive(sA);
        if (shootingCostumeListRoot) shootingCostumeListRoot.SetActive(sC);
        if (runActionListRoot) runActionListRoot.SetActive(rA);
        if (runCostumeListRoot) runCostumeListRoot.SetActive(rC);
        if (puzzleActionListRoot) puzzleActionListRoot.SetActive(pA);
        if (puzzleCostumeListRoot) puzzleCostumeListRoot.SetActive(pC);
    }

    // ───────────────────────────────── 기타 ─────────────────────────────────
    private void ClearChildren(Transform t)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
    }

    private IEnumerator LoadImage(string url, Image target)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.certificateHandler = new DevOnly_BypassCertificate();
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var tex = DownloadHandlerTexture.GetContent(req);

                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;

                var sprite = Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);

                target.sprite = sprite;
                target.preserveAspect = true;
                //target.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning("이미지 로드 실패: " + url);
            }
        }
    }

    public void ClearAllUI()
    {
        ClearChildren(shootingActionContent);
        ClearChildren(shootingCostumeContent);
        ClearChildren(runActionContent);
        ClearChildren(runCostumeContent);
        ClearChildren(puzzleActionContent);
        ClearChildren(puzzleCostumeContent);
    }

}
