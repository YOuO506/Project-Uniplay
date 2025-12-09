using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.Networking;


namespace Game_Puzzle
{
    public class GameManager : MonoBehaviour
    {
        [Header("----------------[ Core ]")]
        public bool isOver;
        public int score;
        public int maxLevel;
        public int currentCoin = 0;
        public int spawnLevelLimit = 2;

        [Header("----------------[ Object Pooling ]")]
        public GameObject donglePrefabs;
        public Transform dongleGroup;
        public List<Dongle> donglePool;
        public GameObject effectPrefabs;
        public Transform effectGroup;
        public List<ParticleSystem> effectPool;
        [Range(1, 30)]
        public int poolSize;
        public int poolCursor;
        public Dongle lastDongle;

        [Header("----------------[ Audio ]")]
        public AudioSource bgmPlayer;
        public AudioSource[] sfxPlayer;
        public AudioClip[] sfxClip;
        public enum Sfx { LevelUp, Next, Attach, Button, Over };
        int sfxCursor;

        [Header("----------------[ UI ]")]
        public GameObject endGroup;
        public GameObject startGroup;
        public Text scoreText;
        public Text maxScoreText;
        public Text subScoreText;
        public Button titleButton;
        public Button itemButton;
        public GameObject getCoin;
        public GameObject tableImage;
        public Text getCoinText;
        public Text txtCoinGameOver;

        [Header("----------------[ ETC ]")]
        public GameObject line;
        public GameObject bottom;

        void Awake()
        {
            bgmPlayer.Play();
            Application.targetFrameRate = 60;

            // 풀 초기화 (씬 재입장 시에도 확실히 초기화)
            if (donglePool == null) donglePool = new List<Dongle>();
            else donglePool.Clear();

            if (effectPool == null) effectPool = new List<ParticleSystem>();
            else effectPool.Clear();

            for (int index = 0; index < poolSize; index++)
            {
                MakeDongle();
            }

            // 최고점은 DB에서 불러오기
            StartCoroutine(LoadHighScoreFromServer());

            Debug.Log("[PUZZLE] Pool 생성 완료: " + donglePool.Count);
        }

        public void GameStart()
        {
            Time.timeScale = 1f;

            // 상태 초기화
            isOver = false;
            lastDongle = null;
            poolCursor = -1;

            // 풀에 남아 있는 동글 전부 비활성화 (씬 재입장 대비)
            foreach (var d in donglePool)
            {
                if (d != null)
                    d.gameObject.SetActive(false);
            }

            // 기능형 아이템 리셋
            Hammer hammer = FindObjectOfType<Hammer>();
            if (hammer != null)
                hammer.ResetCount();

            // 오브젝트 활성화
            line.SetActive(true);
            bottom.SetActive(true);
            scoreText.gameObject.SetActive(true);
            maxScoreText.gameObject.SetActive(true);
            startGroup.SetActive(false);
            titleButton.gameObject.SetActive(true);
            //itemButton.gameObject.SetActive(true);
            getCoin.SetActive(true);
            tableImage.SetActive(true);

            // 사운드 플레이
            bgmPlayer.Play();
            SfxPlayer(Sfx.Button);

            // 게임 시작 (동글생성)
            Invoke("NextDongle", 1.5f);

            currentCoin = 0;
            getCoinText.text = "+0";
        }

        public void GoTitle()
        {
            SceneManager.LoadScene("Main_Stage");
        }

        public void GoGameOver()
        {
            if (!isOver)   // 이미 게임오버가 아니라면
            {
                // 강제로 게임오버 시퀀스 실행
                isOver = true;
                StartCoroutine(GameOverRoutine());
            }
        }

        Dongle MakeDongle()
        {
            // 이펙트 생성
            GameObject instantEffectObj = Instantiate(effectPrefabs, effectGroup);
            instantEffectObj.name = "Effect " + effectPool.Count;
            ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
            effectPool.Add(instantEffect);

            // 동글 생성
            GameObject instantDongleObj = Instantiate(donglePrefabs, dongleGroup);
            instantDongleObj.name = "Dongle " + donglePool.Count;
            Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
            instantDongle.manager = this;
            instantDongle.effect = instantEffect;
            donglePool.Add(instantDongle);

            return instantDongle;
        }

        Dongle GetDongle()
        {
            for (int index = 0; index < donglePool.Count; index++)
            {
                poolCursor = (poolCursor + 1) % donglePool.Count;

                if (!donglePool[poolCursor].gameObject.activeSelf)
                {
                    return donglePool[poolCursor];
                }
            }

            return MakeDongle();
        }

        void NextDongle()
        {

            Debug.Log("[PUZZLE] NextDongle 호출됨, isOver=" + isOver);

            if (isOver)
            {
                return;
            }

            lastDongle = GetDongle();

            // 기본 제한 0~1
            int spawnLimit = 1;

            // 점수 20 이상 → 0~2 가능
            if (score >= 20) spawnLimit = 2;

            // 점수 100 이상 → 0~3 가능
            if (score >= 100) spawnLimit = 3;

            lastDongle.level = Random.Range(0, spawnLimit + 1);
            lastDongle.gameObject.SetActive(true);

            SfxPlayer(Sfx.Next);
            StartCoroutine(WaitNext());
        }

        IEnumerator WaitNext()
        {
            // lastDongle 없어질 때까지 대기
            while (lastDongle != null)
            {
                yield return null;
            }

            // 해머 중이면 대기
            while (Hammer.IsAiming)
            {
                yield return null;
            }

            yield return new WaitForSeconds(2.5f);

            NextDongle();
        }

        public void TouchDown()
        {
            if (lastDongle == null)
                return;

            lastDongle.Drag();
        }

        public void TouchUp()
        {
            if (lastDongle == null)
                return;

            lastDongle.Drop();
            lastDongle = null;
        }

        public void GameOver()
        {
            if (isOver)
            {
                return;
            }
            isOver = true;

            StartCoroutine("GameOverRoutine");
        }

        IEnumerator GameOverRoutine()
        {
            // 장면 안에 활성화 되어있는 모든 동글 목록 가져오기
            Dongle[] dongles = FindObjectsOfType<Dongle>();

            // 지우기 전에 모든 동글의 물리효과 비활성화
            for (int index = 0; index < dongles.Length; index++)
            {
                dongles[index].rigid.simulated = false;
            }

            // 목록을 하나씩 접근해서 지우기
            for (int index = 0; index < dongles.Length; index++)
            {
                dongles[index].Hide(Vector3.up * 100);
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(1f);

            // 최고 점수 갱신 (로컬 PlayerPrefs)
            int prevMax = PlayerPrefs.GetInt("MaxScore");
            int newMax = Mathf.Max(score, prevMax);
            PlayerPrefs.SetInt("MaxScore", newMax);

            // DB(GameScores) 갱신 요청
            // - ScoreApi 호출: userId, password, gameId=3, score 

            // 게임 오버 UI 표시
            subScoreText.text = "점수 : " + scoreText.text;

            // 이번 판 획득 코인 표시
            int predictedTotalCoin = GameSession.Coins + currentCoin;
            txtCoinGameOver.text = $"MyCoin : {GameSession.Coins}";

            endGroup.SetActive(true);

            StartCoroutine(UpdateCoinToServer(currentCoin));
            StartCoroutine(UpdateScoreToServer(score));

            bgmPlayer.Stop();
            SfxPlayer(Sfx.Over);
        }

        // ★ DB(GameScores) 업데이트 코루틴 추가
        IEnumerator UpdateScoreToServer(int finalScore)
        {
            // 요청 DTO 생성
            var req = new ScoreRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                gameId = 3,            // 퍼즐 ID = 3
                score = finalScore
            };

            // JSON 직렬화
            string json = JsonUtility.ToJson(req);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            // ScoreApi 호출
            using (UnityWebRequest request = new UnityWebRequest(ApiConfig.BaseUrlApi + "ScoreApi.aspx", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.certificateHandler = new DevOnly_BypassCertificate(); // 로컬 개발용 인증서 무시

                yield return request.SendWebRequest();

                // 응답 성공 처리
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var res = JsonUtility.FromJson<ScoreResponse>(request.downloadHandler.text);
                    if (res.success)
                    {
                        // ★ 세션 갱신
                        GameSession.PuzzleHighScore = res.newMaxScore;

                        // ★ UI 반영
                        maxScoreText.text = res.newMaxScore.ToString();
                    }
                }
            }
        }

        // 요청/응답 DTO (LoginManager와 동일 구조)
        [System.Serializable]
        class ScoreRequest
        {
            public string userId;
            public string password;
            public int gameId;
            public int score;
        }
        [System.Serializable]
        class ScoreResponse
        {
            public bool success;
            public string userId;
            public int gameId;
            public int newMaxScore;
        }

        IEnumerator UpdateCoinToServer(int gainedCoin)
        {
            // 현재 코인 = 세션 코인 + 이번 판 획득 코인
            int newTotalCoin = GameSession.Coins + gainedCoin;

            // 요청 DTO
            var reqObj = new CoinUpdateRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                coinOp = "update",
                currentCoin = newTotalCoin
            };

            string json = JsonUtility.ToJson(reqObj);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest req = new UnityWebRequest(ApiConfig.BaseUrlApi + "LoginApi.aspx", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.certificateHandler = new DevOnly_BypassCertificate();

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    // 서버 응답에서 최종 코인 반영
                    var res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                    if (res != null && res.success)
                    {
                        // 세션 갱신
                        GameSession.Coins = res.coin;

                        // 게임오버 UI에 소지 코인 표시
                        txtCoinGameOver.text = $"MyCoin : {GameSession.Coins}";

                    }
                }
            }
        }

        // 응답 DTO (LoginManager와 동일)
        [System.Serializable]
        class LoginResponse
        {
            public bool success;
            public string userId;
            public int coin;
        }

        [System.Serializable]
        class CoinUpdateRequest
        {
            public string userId;
            public string password;
            public string coinOp;
            public int currentCoin;
        }

        // DB에서 퍼즐 최고점 불러오기
        IEnumerator LoadHighScoreFromServer()
        {
            var req = new ScoreRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                gameId = 3,   // 퍼즐 ID
                score = 0     // 조회만
            };

            string json = JsonUtility.ToJson(req);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (var request = new UnityWebRequest(ApiConfig.BaseUrlApi + "ScoreApi.aspx", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.certificateHandler = new DevOnly_BypassCertificate();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var res = JsonUtility.FromJson<ScoreResponse>(request.downloadHandler.text);
                    if (res.success)
                    {
                        GameSession.PuzzleHighScore = res.newMaxScore;
                        maxScoreText.text = res.newMaxScore.ToString();
                    }
                }
            }
        }

        public void Reset()
        {
            SfxPlayer(Sfx.Button);
            StartCoroutine("ResetCoroutine");
        }

        IEnumerator ResetCoroutine()
        {
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("Puzzle_Stage");
        }

        public void SfxPlayer(Sfx type)
        {
            switch (type)
            {
                case Sfx.LevelUp:
                    sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                    break;
                case Sfx.Next:
                    sfxPlayer[sfxCursor].clip = sfxClip[3];
                    break;
                case Sfx.Attach:
                    sfxPlayer[sfxCursor].clip = sfxClip[4];
                    break;
                case Sfx.Button:
                    sfxPlayer[sfxCursor].clip = sfxClip[5];
                    break;
                case Sfx.Over:
                    sfxPlayer[sfxCursor].clip = sfxClip[6];
                    break;
            }

            sfxPlayer[sfxCursor].Play();
            sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
        }

        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                Application.Quit();
            }

            // ★ 해머가 이전 클릭을 소비하도록 플래그가 켜져 있으면, MouseUp 한 번만 소비하고 TouchUp 호출 차단
            if (Hammer.BlockClick && Input.GetMouseButtonUp(0))
            {
                Hammer.BlockClick = false;   // 한 번만
                return;                             // ★ TouchUp() 호출 안 함
            }

            // 마우스 입력 → 집게 조작. 해머 조준 중이면 무시.
            if (!Hammer.IsAiming)
            {
                if (Input.GetMouseButtonDown(0)) TouchDown();
                if (Input.GetMouseButtonUp(0)) TouchUp();
            }
        }

        void LateUpdate()
        {
            scoreText.text = score.ToString();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

    }

}
