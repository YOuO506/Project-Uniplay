using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Game_Run
{
    public class GameManager : MonoBehaviour
    {
        const float ORIGIN_SPEED = 3;

        public static float globalSpeed;
        public static float score;
        public static bool isLive;
        public GameObject uiOver;
        public CoinManager coinManager;

        public Text txtHighScore;

        void Start()
        {
            // 게임 시작 전 멈춤 상태
            isLive = false;
            Time.timeScale = 0f;

            // GameStart UI 켜주기
            GameObject gameStartUI = GameObject.Find("GameStart");
            if (gameStartUI != null)
                gameStartUI.SetActive(true);

            CoinManager.instance.SpawnCoin();
        }


        void Awake()
        {
            isLive = true;

            if (!PlayerPrefs.HasKey("Score"))
                PlayerPrefs.SetFloat("Score", 0); // 저장해둔 기본 스코어값이 없을 경우 0으로 지정
        }

        void Update()
        {
            if (!isLive)
                return;

            score += Time.deltaTime * 2;
            globalSpeed = ORIGIN_SPEED + score * 0.01f;   // 시간이 지날수록 스피드 UP

            //Debug.Log(score);
        }

        // 게임 시작 버튼 클릭 시 호출되는 함수
        public void StartGame()
        {
            // 게임 진행 상태 ON
            isLive = true;

            // 일시정지 해제
            Time.timeScale = 1f;

            // 이번 판에서 얻은 코인 초기화
            Game_Run.CoinManager.earnedCoin = 0;

            // 시작 UI 비활성화
            GameObject gameStartUI = GameObject.Find("GameStart");
            if (gameStartUI != null)
                gameStartUI.SetActive(false);
        }

        // 게임 오버
        public void GameOver()
        {
            if (Game_Run.ReviveItem.BlockGameOver) return;

            uiOver.SetActive(true);
            isLive = false;

            // 최고 점수 기록 PlayerPrefs
            // float highScore = PlayerPrefs.GetFloat("Score");
            // PlayerPrefs.SetFloat("Score", Mathf.Max(highScore, score));

            // ---------------- 코인 처리 ----------------
            int earnedCoin = CoinManager.earnedCoin;
            // 세션의 총 코인 값 기준으로 합산
            int newCoin = GameSession.Coins + earnedCoin;
            // 세션 갱신
            GameSession.Coins = newCoin;

            // 서버에 새 총합 전송 (한 번만)
            StartCoroutine(UpdateCoinToServer(newCoin));

            // ---------------- 스코어 처리 추가 ---------------- ★
            int finalScore = Mathf.FloorToInt(score);
            StartCoroutine(UpdateScoreToServer(2, finalScore)); // Run 게임 = GameID 2
        }

        private IEnumerator UpdateCoinToServer(int currentCoin)
        {
            string url = ApiConfig.BaseUrlApi + "LoginApi.aspx";



            Debug.Log("GameSession.UserId(before JSON) = " + GameSession.UserId);
            Debug.Log("GameSession.Password(before JSON) = " + GameSession.Password);

            // 요청 JSON 만들기
            var requestData = new CoinUpdateRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                coinOp = "update",
                currentCoin = currentCoin
            };

            string json = JsonUtility.ToJson(requestData);
            Debug.Log("REQ JSON = " + json);

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                yield return req.SendWebRequest();
                Debug.Log("RESP CODE = " + req.responseCode);
                Debug.Log("RESP BODY = " + req.downloadHandler.text);

                if (req.result == UnityWebRequest.Result.Success)
                {
                    // 응답 JSON 파싱
                    var res = JsonUtility.FromJson<CoinResponse>(req.downloadHandler.text);
                    if (res.success)
                    {
                        GameSession.Coins = res.coin;
                        // UI 업데이트
                        var txt = uiOver.transform.Find("TxtCoinGameOver").GetComponent<Text>();
                        txt.text = "MyCoin : " + res.coin;

                        // 인벤토리 UI 즉시 갱신
                        var inv = FindObjectOfType<InventoryManager>();
                        if (inv != null) inv.RefreshCoinUI();

                    }
                }
                else
                {
                    Debug.LogError("Coin update failed: " + req.error);
                }
            }
        }

        // ScoreApi 호출 코루틴
        private IEnumerator UpdateScoreToServer(int gameId, int score)
        {
            // 요청 URL
            string url = ApiConfig.BaseUrlApi + "ScoreApi.aspx";

            // 요청 JSON 데이터
            var requestData = new ScoreRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                gameId = gameId,
                score = score
            };

            // JSON 직렬화
            string json = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            // HTTP POST 요청
            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                yield return req.SendWebRequest();

                // 서버 응답 확인
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var res = JsonUtility.FromJson<ScoreResponse>(req.downloadHandler.text);
                    if (res.success)
                    {
                        // UI에 최고기록 표시
                        // Inspector에 직접 연결한 Text 사용
                        GameSession.RunHighScore = res.newMaxScore;
                    }
                }
                else
                {
                    Debug.LogError("Score update failed: " + req.error);
                }
            }
        }

        [System.Serializable]
        private class CoinUpdateRequest
        {
            public string userId;
            public string password;
            public string coinOp;
            public int currentCoin;
        }

        // 응답 구조체
        [System.Serializable]
        private class CoinResponse
        {
            public bool success;
            public string userId;
            public int coin;
        }

        // 요청 DTO
        [System.Serializable]
        private class ScoreRequest
        {
            public string userId;
            public string password;
            public int gameId;
            public int score;
        }

        // 응답 DTO
        [System.Serializable]
        private class ScoreResponse
        {
            public bool success;
            public string userId;
            public int gameId;
            public int newMaxScore;
        }

        // 재시작 버튼
        public void Restart()
        {
            SceneManager.LoadScene("Run_Stage");
            score = 0;
            isLive = true;
        }

        // 타이틀 버튼
        public void GoTitle()
        {
            SceneManager.LoadScene("Main_Stage");
            score = 0;
        }
    }
}

