using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.Networking;

namespace Game_Shooting
{
    public class GameManager : MonoBehaviour
    {
        // ---------------- Stage & Flow ----------------
        [Header("Stage / Flow")]
        public int stage;
        public List<Spawn> spawnList;
        public int spawnIndex;
        public bool spawnEnd;

        // ---------------- UI References ----------------
        [Header("UI References")]
        public Animator stageAnim;
        public Animator clearAnim;
        public Animator fadeAnim;
        public GameObject gameOverSet;
        public GameObject startSetUI;
        public Text scoreText;
        public Text getCoinText;
        public Text txtHighScore;
        public Text txtCoinGameOver;
        public Image[] lifeImage;
        public Image[] boomImage;

        private int localCoin = 0;
        private bool stageStarted = false;

        // ---------------- Player & Position ----------------
        [Header("Player")]
        public GameObject player;
        public Transform playerPos;
        // 현재 장착된 ItemID
        public int equippedItemId = 0;

        // ---------------- Enemy & Spawn ----------------
        [Header("Enemy Spawn")]
        public string[] enemyObj;
        public Transform[] spawnPoints;
        public float nextSpawnDelay;
        public float curSpawnDelay;

        // ---------------- Manager ----------------
        [Header("Managers")]
        public ObjectManager objectManager;

        void Start()
        {
            // 실행 시 최고점 표시
            if (txtHighScore != null)
                txtHighScore.text = string.Format("{0:n0}", GameSession.ShootingHighScore);

            if (getCoinText != null)
                getCoinText.text = "0";

            // Player 스크립트에 equippedItemdId 전달
            Player playerLogic = player.GetComponent<Player>();
            if (playerLogic != null)
            {
                // PlayerPrefs에서 현재 장착 코스튬 불러오기
                int costumeId = PlayerPrefs.GetInt("UsingShootingCostume", 0); // 기본은 0(Blue)
                playerLogic.equippedItemId = costumeId;
                // 코스튬 적용
                playerLogic.ApplyCostume(costumeId);

                // 액션(폭탄) 아이템 체크 후 초기 폭탄 개수 지급
                int actionId = PlayerPrefs.GetInt("UsingShootingAction", -1);

                if (actionId == 9)
                {
                    playerLogic.boom = Mathf.Max(playerLogic.boom, 3);
                    playerLogic.maxBoom = Mathf.Max(playerLogic.maxBoom, 3);
                }
                else
                {
                    // 미장착 - 폭탄 없음
                    playerLogic.boom = 0;
                }

                // 폭탄 UI 갱신
                UpdateBoomIcon(playerLogic.boom);
            }
        }


        void Awake()
        {
            spawnList = new List<Spawn>();
            enemyObj = new string[] { "EnemyS", "EnemyM", "EnemyL", "EnemyB" };

            Time.timeScale = 0f;
            startSetUI.SetActive(true);
        }

        public void StageStart()
        {
            //Stage UI Load
            stageAnim.SetTrigger("On");
            stageAnim.GetComponent<Text>().text = "STAGE " + stage;
            clearAnim.GetComponent<Text>().text = "STAGE " + stage + "\nClear!!";
            //Enemy Spawn File Read
            ReadSpawnFile();

            //Fade In
            //fadeAnim.SetTrigger("In");
        }

        public void StageEnd()
        {
            //Clear UI Load
            clearAnim.SetTrigger("On");

            //Fade Out
            //fadeAnim.SetTrigger("Out");

            //Player Repos
            //player.transform.position = playerPos.position;

            // Player 무적 타임 재활용 (Respawn 때 쓰던 것 호출)
            Player playerLogic = player.GetComponent<Player>();
            if (playerLogic != null)
            {
                playerLogic.isRespawnTime = false;   // 무적 상태 초기화
                playerLogic.Invoke("Unbeatable", 0); // 바로 Unbeatable 실행
                playerLogic.Invoke("Unbeatable", 3); // 3초 뒤 해제
            }


            //Stage Increament
            stage++;
            if (stage > 10)
                Invoke("GameOver", 6);
            else
                Invoke("StageStart", 5);
        }

        void ReadSpawnFile()
        {
            //변수 초기화
            spawnList.Clear();
            spawnIndex = 0;
            spawnEnd = false;

            //리스폰 파일 읽기
            TextAsset textFile = Resources.Load("Stage " + stage) as TextAsset;
            StringReader stringReader = new StringReader(textFile.text);

            while (stringReader != null)
            {
                string line = stringReader.ReadLine();
                Debug.Log(line);

                if (line == null)
                {
                    break;
                }

                //리스폰 데이터 생성
                Spawn spawnData = new Spawn();
                spawnData.delay = float.Parse(line.Split(',')[0]);
                spawnData.type = line.Split(',')[1];
                spawnData.point = int.Parse(line.Split(',')[2]);
                spawnList.Add(spawnData);
            }

            //텍스트 파일 닫기
            stringReader.Close();

            //첫번째 스폰 딜레이 적용
            nextSpawnDelay = spawnList[0].delay;
        }

        void Update()
        {

            curSpawnDelay += Time.deltaTime;

            if (curSpawnDelay > nextSpawnDelay && !spawnEnd)
            {
                SpawnEnemy();
                curSpawnDelay = 0;
            }

            //UI Score Update
            Player playerLogic = player.GetComponent<Player>();
            scoreText.text = string.Format("{0:n0}", playerLogic.score);

            // 획득 코인 표시 (게임 중)
            if (getCoinText != null)
                getCoinText.text = localCoin.ToString();

            // 스테이지 클리어 조건
            if (spawnEnd && stageStarted)
            {
                bool enemyAlive = false;

                foreach (string name in enemyObj)
                {
                    GameObject[] objs = objectManager.GetPool(name);
                    foreach (var obj in objs)
                    {
                        if (obj.activeSelf)
                        {
                            enemyAlive = true;
                            break; // 에너미 하나라도 살아 있으면 break
                        }
                    }
                    if (enemyAlive) break;
                }

                // 모든 적이 사라진 경우에만 스테이지 종료
                if (!enemyAlive)
                {
                    stageStarted = false;   // 다음 스테이지를 위해 초기화
                    StageEnd();
                }
            }
        }

        // 이번 판 코인 +1 (Player에서 호출)
        public void AddCoin()
        {
            localCoin++;
        }

        void SpawnEnemy()
        {

            // 리스트 범위 벗어나면 바로 리턴
            if (spawnIndex >= spawnList.Count)
            {
                spawnEnd = true;
                return;
            }

            int enemyIndex = 0;
            switch (spawnList[spawnIndex].type)
            {
                case "S":
                    enemyIndex = 0;
                    break;
                case "M":
                    enemyIndex = 1;
                    break;
                case "L":
                    enemyIndex = 2;
                    break;
                case "B":
                    enemyIndex = 3;
                    break;
            }

            if (spawnIndex >= spawnList.Count)
            {
                spawnEnd = true;
                return;
            }

            if (!stageStarted) stageStarted = true;


            int enemyPoint = spawnList[spawnIndex].point;
            GameObject enemy = objectManager.MakeObj(enemyObj[enemyIndex]);
            enemy.transform.position = spawnPoints[enemyPoint].position;

            Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
            Enemy enemyLogic = enemy.GetComponent<Enemy>();
            enemyLogic.player = player;
            enemyLogic.gameManager = this;
            enemyLogic.objectManager = objectManager;

            if (enemyPoint == 5 || enemyPoint == 6) //Right Spawn
            {
                enemy.transform.Rotate(Vector3.back * 90); //방향 회전
                rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
            }
            else if (enemyPoint == 7 || enemyPoint == 8) //Left Spawn
            {
                enemy.transform.Rotate(Vector3.forward * 90); //방향 회전
                rigid.velocity = new Vector2(enemyLogic.speed, -1);
            }
            else //Front Spawn
            {
                rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
            }

            //리스폰 인덱스 증가
            spawnIndex++;
            if (spawnIndex >= spawnList.Count)
            {
                spawnEnd = true;
                return;
            }

            //다음 리스폰 딜레이 갱신
            nextSpawnDelay = spawnList[spawnIndex].delay;
        }

        public void UpdateLifeIcon(int life)
        {
            //UI Life Init Disable
            for (int index = 0; index < 3; index++)
            {
                lifeImage[index].color = new Color(1, 1, 1, 0);
            }

            //UI Life Active
            for (int index = 0; index < life; index++)
            {
                lifeImage[index].color = new Color(1, 1, 1, 1);
            }
        }

        public void UpdateBoomIcon(int boom)
        {
            //UI Life Init Disable
            for (int index = 0; index < 3; index++)
            {
                boomImage[index].color = new Color(1, 1, 1, 0);
            }

            //UI Life Active
            for (int index = 0; index < boom; index++)
            {
                boomImage[index].color = new Color(1, 1, 1, 1);
            }
        }

        public void RespawnPlayer()
        {
            Invoke("RespawnPlayerExe", 2f);
        }

        void RespawnPlayerExe()
        {
            player.transform.position = Vector3.down * 3.5f;
            player.SetActive(true);

            Player playerLogic = player.GetComponent<Player>();
            playerLogic.isHit = false;
        }

        public void CallExplosion(Vector3 pos, string type)
        {
            GameObject explosion = objectManager.MakeObj("Explosion");
            Explosion explosionLogic = explosion.GetComponent<Explosion>();

            explosion.transform.position = pos;
            explosionLogic.StartExplosion(type);
        }

        public void GameStart()
        {
            Time.timeScale = 1f;
            StageStart();
            startSetUI.SetActive(false);
        }

        public void GameOver()
        {
            gameOverSet.SetActive(true);

            // 게임 오버시 내 보유 코인 (세션 값)
            if (txtCoinGameOver != null)
                txtCoinGameOver.text = $"My Coin = {GameSession.Coins}";

            // 이번 점수
            int curScore = player.GetComponent<Player>().score;

            // 항상 서버로 전송 (DB가 Max 처리)
            StartCoroutine(UpdateScoreToServer(1, curScore)); // Shooting = GameID 1

            // 로컬 UI 갱신: 현재 점수가 기록보다 크면 반영
            if (curScore > GameSession.ShootingHighScore)
            {
                GameSession.ShootingHighScore = curScore;

                if (txtHighScore != null)
                    txtHighScore.text = string.Format("{0:n0}", curScore);
            }
        }

        private IEnumerator UpdateScoreToServer(int gameId, int score)
        {
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
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.certificateHandler = new DevOnly_BypassCertificate();

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var res = JsonUtility.FromJson<ScoreResponse>(req.downloadHandler.text);
                    if (res.success)
                    {
                        // 서버에서 받은 newMaxScore를 세션/텍스트에 반영
                        GameSession.ShootingHighScore = res.newMaxScore;

                        if (txtHighScore != null)
                            txtHighScore.text = string.Format("{0:n0}", res.newMaxScore);
                    }
                }
                else
                {
                    Debug.LogError("Shooting Score update failed: " + req.error);
                }
            }
        }

        private class DevOnly_BypassCertificate : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }

        public void GameRetry()
        {
            SceneManager.LoadScene("Shooting_Stage");
        }

        public void GoTitle()
        {
            SceneManager.LoadScene("Main_Stage");
        }

        // DTO
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
}

