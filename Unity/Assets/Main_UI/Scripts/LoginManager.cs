using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// 응답 DTO

public class LoginManager : MonoBehaviour
{
    [Header("API")]
    [Tooltip("http://localhost:8080/Api/LoginApi.aspx | https://localhost:44357/Api/LoginApi.aspx")]
    public string loginApiUrl;                           // 로그인 API URL

    [Header("UI References")]
    public GameObject loginPanel;
    public InputField inputUserId;
    public InputField inputPassword;
    public Button btnOpenLogin;
    public Button btnLogin;
    public Button btnLogout;
    public Text successMessage;
    public Text failMessage;

    private bool _busy = false;
    private string _currentUserId = null;
    private string _currentNick = null;

    public InventoryManager inventoryManager;
    public GameObject gameList;


    private void Awake()
    {
        // 실행 시 ApiConfig에서 자동 세팅
        loginApiUrl = ApiConfig.BaseUrlApi + "LoginApi.aspx";
    }

    private void Start()
    {
        // 초기 상태 : 로그인 버튼만 활성, 로그아웃 버튼 숨김, 메시지들 숨김
        if (btnOpenLogin != null) btnOpenLogin.gameObject.SetActive(true);
        if (btnLogout != null) btnLogout.gameObject.SetActive(false);
        if (successMessage != null) successMessage.gameObject.SetActive(false);
        if (failMessage != null) failMessage.gameObject.SetActive(false);

        // 버튼에 클릭 이벤트 연결 ---- 
        if (btnLogin != null) btnLogin.onClick.AddListener(OnClickLogin);
        if (btnLogout != null) btnLogout.onClick.AddListener(OnClickLogout);

        //GameSession.LoadFromPrefs();  // 로그인 후 재실행시 유지 기능 (추후 할지말지 결정)
        ApplySessionToUI();
    }

    void Update()
    {
        // 로그인 화면 입력 간편화 처리
        // Tab 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == inputUserId.gameObject)
            {
                // ID 입력칸 → PW 입력칸
                inputPassword.ActivateInputField();
                EventSystem.current.SetSelectedGameObject(inputPassword.gameObject);
            }
            else if (EventSystem.current.currentSelectedGameObject == inputPassword.gameObject)
            {
                // PW 입력칸 → 로그인 버튼
                EventSystem.current.SetSelectedGameObject(btnLogin.gameObject);
            }
        }

        // Enter 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (EventSystem.current.currentSelectedGameObject == inputUserId.gameObject ||
                EventSystem.current.currentSelectedGameObject == inputPassword.gameObject)
            {
                // 로그인 버튼 실행
                btnLogin.onClick.Invoke();
            }
        }
    }

    // 세션 상태를 UI로 반영
    private void ApplySessionToUI()                                    
    {
        if (GameSession.IsLoggedIn)
        {
            _currentUserId = GameSession.UserId;                       // 현재 유저ID 보관
            _currentNick = GameSession.NickName;                       // 현재 닉네임 보관
            if (successMessage != null)                                // 성공 메시지 있으면
            {
                successMessage.text = $"Hello! {_currentNick}";        // 환영 문구 세팅
                successMessage.gameObject.SetActive(true);             // 표시
            }
            if (btnOpenLogin != null) btnOpenLogin.gameObject.SetActive(false); // 로그인 버튼 숨김
            if (btnLogout != null) btnLogout.gameObject.SetActive(true);        // 로그아웃 버튼 표시
            if (loginPanel != null) loginPanel.SetActive(false);                // 로그인 패널 닫기
        }
        else
        {
            _currentUserId = null;                                     // 유저ID 클리어
            _currentNick = null;                                       // 닉네임 클리어
            if (successMessage != null) successMessage.gameObject.SetActive(false); // 성공 메시지 숨김
            if (failMessage != null) failMessage.gameObject.SetActive(false);       // 실패 메시지 숨김
            if (btnOpenLogin != null) btnOpenLogin.gameObject.SetActive(true);      // 로그인 버튼 표시
            if (btnLogout != null) btnLogout.gameObject.SetActive(false);           // 로그아웃 숨김

            // if (loginPanel != null) loginPanel.SetActive(true);                  // 로그인 패널 열어두기
        }
    }

    void OnClickLogin()
    {
        if (_busy) return;

        // 메시지들 초기화
        if (successMessage != null) successMessage.gameObject.SetActive(false);
        if (failMessage != null) failMessage.gameObject.SetActive(false);

        string uid = inputUserId != null ? inputUserId.text.Trim() : "";
        string pwd = inputPassword != null ? inputPassword.text.Trim() : "";

        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pwd))
        {
            if (failMessage != null) failMessage.gameObject.SetActive(true); // "Invalid ID or PW" 
            return;
        }

        // 서버로 로그인 요청 보내기 (나중에 UnityWebRequest로 ASP.NET 연결)
        StartCoroutine(CoLogin(uid, pwd));
    }

    // 로그아웃 버튼에서 호출
    public void OnClickLogout()
    {
        if (_busy) return;

        // 상태 리셋
        _currentUserId = null;
        _currentNick = null;

        // UI 원복: 성공/실패 메시지 숨김, 로그인 버튼 보이기, 로그아웃 버튼 숨김
        if (successMessage != null) successMessage.gameObject.SetActive(false);
        if (failMessage != null) failMessage.gameObject.SetActive(false);

        if (btnOpenLogin != null) btnOpenLogin.gameObject.SetActive(true);
        if (btnLogout != null) btnLogout.gameObject.SetActive(false);

        // 비밀번호만 비우기(선택)
        if (inputPassword != null) inputPassword.text = "";

        GameSession.Clear();
        ApplySessionToUI();

        // 로그아웃 시 인벤토리 UI 초기화
        if (inventoryManager != null)
        {
            inventoryManager.ClearAllUI();   // 전체 카드 제거 함수
            inventoryManager.inventoryPanel.SetActive(false);
        }

        if (gameList != null) gameList.SetActive(true);
    }

    private IEnumerator CoLogin(string userId, string password)
    {
        _busy = true;
        if (btnLogin != null) btnLogin.interactable = false;

        // 요청 JSON 만들기
        var reqObj = new LoginRequest { userId = userId, password = password };
        string json = JsonUtility.ToJson(reqObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // UnityWebRequest 구성
        using (var req = new UnityWebRequest(loginApiUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            // 로컬 https(self-signed) 개발용: 인증서 무시 핸들러(배포에서는 절대 사용 금지)
            req.certificateHandler = new DevOnly_BypassCertificate();

            yield return req.SendWebRequest();

            bool ok = req.result == UnityWebRequest.Result.Success;

            if (!ok)
            {
                // 네트워크/서버 오류 → 실패 메시지만 표시
                if (failMessage != null) failMessage.gameObject.SetActive(true);
                FinishRequest();
                yield break;
            }

            // 응답 파싱
            var respText = req.downloadHandler.text;
            LoginResponse resp = null;
            try
            {
                resp = JsonUtility.FromJson<LoginResponse>(respText);
            }
            catch
            {
                // 파싱 실패도 실패 처리
                if (failMessage != null) failMessage.gameObject.SetActive(true);
                FinishRequest();
                yield break;
            }

            if (resp != null && resp.success)
            {
                // 성공: SuccessMessage 활성화 및 텍스트 "Hello {NickName}"
                _currentUserId = resp.userId;
                _currentNick = resp.nickName;

                // 세션에 기록 -----------------
                GameSession.UserId = resp.userId;
                GameSession.NickName = resp.nickName;
                GameSession.UGrade = resp.uGrade;
                GameSession.Password = password;

                Debug.Log($"[LoginManager] Save Session UserId={GameSession.UserId}, Pw={GameSession.Password}");

                int coinVal = (resp.coin != 0) ? resp.coin : resp.Coin;
                GameSession.Coins = coinVal;
                Debug.Log($"[Login] Coin from server = {coinVal}");

                //GameSession.SaveToPrefs();     // PlayerPrefs 저장 : 얘도 재접속 시 로그인 유지 기능

                // 응답에 들어온 장착 아이템들을 PlayerPrefs에 기록
                if (resp.equipped != null)
                {
                    foreach (var e in resp.equipped)
                    {
                        if (e.gameId == 2 && e.itemTypeId == 2)
                            PlayerPrefs.SetInt("UsingRunCostume", e.itemId);
                        else if (e.gameId == 2 && e.itemTypeId == 1)
                            PlayerPrefs.SetInt("UsingRunAction", e.itemId);
                        else if (e.gameId == 1 && e.itemTypeId == 2)
                            PlayerPrefs.SetInt("UsingShootingCostume", e.itemId);
                        else if (e.gameId == 1 && e.itemTypeId == 1)
                            PlayerPrefs.SetInt("UsingShootingAction", e.itemId);
                        else if (e.gameId == 3 && e.itemTypeId == 2)
                            PlayerPrefs.SetInt("UsingPuzzleCostume", e.itemId);
                        else if (e.gameId == 3 && e.itemTypeId == 1)
                            PlayerPrefs.SetInt("UsingPuzzleAction", e.itemId);
                    }
                    PlayerPrefs.Save();
                }

                // 로그인 성공 시 Hello! NickName
                if (successMessage != null)
                {
                    successMessage.text = $"Hello! {_currentNick}";
                    successMessage.gameObject.SetActive(true);
                }

                // 버튼 전환: 로그인 버튼 비활성(숨김), 로그아웃 버튼 활성
                if (btnOpenLogin != null) btnOpenLogin.gameObject.SetActive(false);
                if (btnLogout != null) btnLogout.gameObject.SetActive(true);

                // 실패 메시지 숨김
                if (failMessage != null) failMessage.gameObject.SetActive(false);

                // 로그인 성공 시 패널 닫기
                if (loginPanel != null) loginPanel.SetActive(false);

                if (inventoryManager != null) inventoryManager.RefreshCoinUI();

                // 로그인 성공 후 내 최고점 조회
                StartCoroutine(FetchHighScore(1)); // Shooting
                StartCoroutine(FetchHighScore(2)); // Run
                StartCoroutine(FetchHighScore(3)); // Puzzle
            }
            else
            {
                // 실패: Fail Message 객체 활성화
                if (failMessage != null) failMessage.gameObject.SetActive(true);

                // 보안상 비밀번호 입력만 비우기 권장
                if (inputPassword != null) inputPassword.text = "";
            }

            FinishRequest();
        }
    }

    // 내 최고점 조회 코루틴
    private IEnumerator FetchHighScore(int gameId)
    {
        string url = ApiConfig.BaseUrlApi + "ScoreApi.aspx";

        // 요청 DTO
        var requestData = new ScoreRequest
        {
            userId = GameSession.UserId,
            password = GameSession.Password,
            gameId = gameId,
            score = 0 // 조회만 하므로 0 전달
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<ScoreResponse>(req.downloadHandler.text);
                if (res.success)
                {
                    // 게임 ID에 따라 세션 변수에 저장
                    switch (gameId)
                    {
                        case 1:
                            GameSession.ShootingHighScore = res.newMaxScore;
                            break;
                        case 2:
                            GameSession.RunHighScore = res.newMaxScore;
                            break;
                        case 3:
                            GameSession.PuzzleHighScore = res.newMaxScore;
                            break;
                    }
                }
            }
        }
    }

    // 요청 종료시 공통 마무리
    private void FinishRequest()
    {
        _busy = false;
        if (btnLogin != null) btnLogin.interactable = true;
    }

    // 요청/응답 DTO
    [System.Serializable]
    private class LoginRequest
    {
        public string userId;
        public string password;
    }
    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public string userId;
        public string nickName;
        public int uGrade;
        public int coin;
        public int Coin;
        public EquippedItem[] equipped;
        public string error;
        public string message;
    }

    [System.Serializable]
    public class EquippedItem   // 새 클래스 추가
    {
        public int gameId;
        public int itemId;
        public int itemTypeId;  // 1=Action, 2=Costume
    }

    // ScoreApi 요청/응답 DTO
    [System.Serializable]
    private class ScoreRequest
    {
        public string userId;
        public string password;
        public int gameId;
        public int score;
    }

    [System.Serializable]
    private class ScoreResponse
    {
        public bool success;
        public string userId;
        public int gameId;
        public int newMaxScore;
    }
}
