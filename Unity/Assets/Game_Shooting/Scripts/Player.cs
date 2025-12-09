using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Game_Shooting
{
    public class Player : MonoBehaviour
    {
        [Header("Player State")]
        // 플레이어 상태
        public bool isTouchTop;
        public bool isTouchBottom;
        public bool isTouchLeft;
        public bool isTouchRight;
        public bool isHit;
        public bool isBoomTime;
        public bool isRespawnTime;
        public bool isControl;

        [Header("Control Panel")]
        // 컨트롤 패널
        public bool[] joyControl;
        public bool isButtonA;
        public bool isButtonB;

        [Header("Value")]
        // 목숨, 스코어, 파워, 아이템 등등
        public int life;
        public int score;
        public int power;
        public int maxPower;
        public int boom;
        public int maxBoom;

        [Header("Speed")]
        // 속도 관련
        public float speed;
        public float maxShotDelay;
        public float curShotDelay;

        [Header("Item Prefabs / Object")]
        // 아이템 관련
        public GameObject bulletObj0;
        public GameObject bulletObj1;
        public GameObject boomEffect;
        public GameObject[] followers;

        [Header("Manager")]
        // 매니저
        public GameManager gameManager;
        public ObjectManager objectManager;

        // GetComponent
        Animator anim;
        SpriteRenderer spriteRenderer;

        // 코스튬 세트 정의
        [System.Serializable]
        public class CostumeClips
        {
            public int ItemId;
            public AnimationClip center;
            public AnimationClip left;
            public AnimationClip right;
        }

        bool applied = false;

        [Header("Costume Set")]
        // 코스튬 세트 등록
        public CostumeClips[] costumes;
        // 현재 적용할 코스튬의 ItemId
        public int equippedItemId = 0;

        [System.Serializable]
        public class ActionClips
        {
            public int ItemId;
            public string name;
            public MonoBehaviour script;
        }

        [Header("Action Set")]
        public ActionClips[] Actions;
        // Boom 사용 가능 여부
        [HideInInspector] public bool boomEnabled = false;

        void Awake()
        {
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

        }

        void Start()
        {
            // 현재 장착된 Shooting-Action 읽기
            int equippedActionId = PlayerPrefs.GetInt("UsingShootingAction", -1);

            boomEnabled = (equippedActionId == 9);

            // 등록된 액션 스크립트들 전부 순회 - 해당 ID만 활성화
            if (Actions != null)
            {
                foreach (var a in Actions)
                {
                    if (a.script == null) continue;
                    a.script.enabled = (a.ItemId == equippedActionId);
                }
            }

            // 폭탄 아이템 장착 시에만 초기 폭탄 지급
            //if (equippedActionId == 9) // Boom 아이템 장착 시
            //{
            //    boom = Mathf.Max(boom, 3);
            //    maxBoom = Mathf.Max(maxBoom, 3);
            //    gameManager.UpdateBoomIcon(boom);
            //}
            //else
            //{
            //    boom = 0;
            //    gameManager.UpdateBoomIcon(boom);
            //}
        }

        // 코스튬 적용 메서드
        public void ApplyCostume(int itemId)
        {
            foreach (var costume in costumes)
            {
                if (costume.ItemId == itemId)
                {
                    applied = true;

                    var overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
                    overrideController["Player_Center"] = costume.center;
                    overrideController["Player_Left"] = costume.left;
                    overrideController["Player_Right"] = costume.right;
                    anim.runtimeAnimatorController = overrideController;
                    break;
                }
            }

            // 해당 ItemId 못 찾으면 디폴트(0) 세트 적용
            if (!applied)
            {
                foreach (var costume in costumes)
                {
                    if (costume.ItemId == 0) // 기본 세트
                    {
                        var overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
                        overrideController["Player_Center"] = costume.center;
                        overrideController["Player_Left"] = costume.left;
                        overrideController["Player_Right"] = costume.right;
                        anim.runtimeAnimatorController = overrideController;
                        break;
                    }
                }
            }
        }

        void OnEnable()
        {
            // 무적 시작
            SetInvincible(true);
            // 3초 뒤 무적 해제
            Invoke(nameof(DisableInvincible), 3f);
        }

        // 무적 켜기/끄기 함수
        void SetInvincible(bool active)
        {
            isRespawnTime = active;

            float alpha = active ? 0.5f : 1f;
            spriteRenderer.color = new Color(1, 1, 1, alpha);

            for (int i = 0; i < followers.Length; i++)
            {
                followers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
            }
        }

        void DisableInvincible()
        {
            // 무적 종료
            SetInvincible(false);
        }

        void Update()
        {
            // 키보드 A키 = 총알 발사 버튼 역할
            if (Input.GetKeyDown(KeyCode.Z))
                isButtonA = true;
            if (Input.GetKeyUp(KeyCode.Z))
                isButtonA = false;

            // 키보드 X키 = 폭탄 버튼 역할
            if (Input.GetKeyDown(KeyCode.X))
                isButtonB = true;
            if (Input.GetKeyUp(KeyCode.X))
                isButtonB = false;

            // 방향키 눌렀을 땐 조이스틱 컨트롤 켜기
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)
                || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
                isControl = true;
            else if (!isControl) // UI 조작 중이면 유지
                isControl = false;

            Move();
            Fire();
            Boom();
            Reload();
        }

        public void JoyPanel(int type)
        {
            for (int index = 0; index < 9; index++)
            {
                joyControl[index] = index == type;
            }
        }

        public void JoyDown()
        {
            isControl = true;
        }

        public void JoyUp()
        {
            isControl = false;

            // joyControl 전체 해제
            for (int i = 0; i < joyControl.Length; i++)
                joyControl[i] = false;
        }

        // 이전 프레임 h값 저장
        private float prevH = 0f;

        void Move()
        {
            float h = 0f;
            float v = 0f;

            // 키보드 입력 
            if (Input.GetKey(KeyCode.LeftArrow)) h = -1;
            if (Input.GetKey(KeyCode.RightArrow)) h = 1;
            if (Input.GetKey(KeyCode.UpArrow)) v = 1;
            if (Input.GetKey(KeyCode.DownArrow)) v = -1;

            // 조이컨트롤
            if (joyControl != null && joyControl.Length >= 0)
            {
                if (joyControl[0]) { h = -1; v = 1; }
                if (joyControl[1]) { h = 0; v = 1; }
                if (joyControl[2]) { h = 1; v = 1; }
                if (joyControl[3]) { h = -1; v = 0; }
                if (joyControl[4]) { h = 0; v = 0; }
                if (joyControl[5]) { h = 1; v = 0; }
                if (joyControl[6]) { h = -1; v = -1; }
                if (joyControl[7]) { h = 0; v = -1; }
                if (joyControl[8]) { h = 1; v = -1; }
            }

            // 충돌 경계 처리
            if ((isTouchRight && h == 1) || (isTouchLeft && h == -1))
                h = 0;

            if ((isTouchTop && v == 1) || (isTouchBottom && v == -1))
                v = 0;

            // 실제 이동
            Vector3 curPos = transform.position;
            Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;
            transform.position = curPos + nextPos;

            // 애니메이션
            if (prevH != h)
            {
                anim.SetInteger("Input", (int)h);
                prevH = h;
            }
        }

        public void ButtonADown()
        {
            isButtonA = true;
        }

        public void ButtonAUp()
        {
            isButtonA = false;
        }

        public void ButtonBDown()
        {
            isButtonB = true;
        }

        void Fire()
        {
            //Player Fire
            //if(!Input.GetButton("Fire1"))
            //    return;


            if (!isButtonA)
                return;

            if (curShotDelay < maxShotDelay)
                return;

            switch (power)
            {
                case 0:
                    //Power 0
                    GameObject bullet = objectManager.MakeObj("BulletPlayer0");
                    bullet.transform.position = transform.position;

                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
                case 1:
                    //Power 1
                    GameObject bulletL = objectManager.MakeObj("BulletPlayer0");
                    bulletL.transform.position = transform.position + Vector3.left * 0.1f;
                    GameObject bulletR = objectManager.MakeObj("BulletPlayer0");
                    bulletR.transform.position = transform.position + Vector3.right * 0.1f;

                    Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                    rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
                default:
                    //Power 2~5
                    GameObject bulletLL = objectManager.MakeObj("BulletPlayer0");
                    bulletLL.transform.position = transform.position + Vector3.left * 0.35f;
                    GameObject bulletCC = objectManager.MakeObj("BulletPlayer1");
                    bulletCC.transform.position = transform.position;
                    GameObject bulletRR = objectManager.MakeObj("BulletPlayer0");
                    bulletRR.transform.position = transform.position + Vector3.right * 0.35f;

                    Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                    rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
            }

            curShotDelay = 0; //초기화
        }

        void Reload()
        {
            curShotDelay += Time.deltaTime;
        }

        void Boom()
        {
            Debug.Log($"[Boom] enabled={boomEnabled}, boom={boom}, isBoomTime={isBoomTime}, isButtonB={isButtonB}");


            //if (!Input.GetButton("Fire2"))
            //    return;

            // 장착 안 했으면 사용 불가
            if (!boomEnabled)
                return;

            if (!isButtonB)
                return;

            if (isBoomTime)
                return;

            if (boom == 0)
                return;

            boom--;
            isBoomTime = true;
            gameManager.UpdateBoomIcon(boom);

            //Effect Visible
            boomEffect.SetActive(true);
            Invoke("OffBoomEffect", 4f);

            //Remove Enemy
            GameObject[] enemiesL = objectManager.GetPool("EnemyL");
            GameObject[] enemiesM = objectManager.GetPool("EnemyM");
            GameObject[] enemiesS = objectManager.GetPool("EnemyS");
            for (int index = 0; index < enemiesL.Length; index++)
            {
                if (enemiesL[index].activeSelf)
                {
                    Enemy enemyLogic = enemiesL[index].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }

            for (int index = 0; index < enemiesM.Length; index++)
            {
                if (enemiesM[index].activeSelf)
                {
                    Enemy enemyLogic = enemiesM[index].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }

            for (int index = 0; index < enemiesS.Length; index++)
            {
                if (enemiesS[index].activeSelf)
                {
                    Enemy enemyLogic = enemiesS[index].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }

            //Remove Enemy Bullet
            GameObject[] bullets0 = objectManager.GetPool("BulletEnemy0");
            GameObject[] bullets1 = objectManager.GetPool("BulletEnemy1");

            for (int index = 0; index < bullets0.Length; index++)
            {
                if (bullets0[index].activeSelf)
                {
                    bullets0[index].SetActive(false);
                }
            }

            for (int index = 0; index < bullets1.Length; index++)
            {
                if (bullets1[index].activeSelf)
                {
                    bullets1[index].SetActive(false);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            //화면 벗어남 방지
            if (collision.gameObject.tag == "Border")
            {
                switch (collision.gameObject.name)
                {
                    case "Top":
                        isTouchTop = true;
                        break;
                    case "Bottom":
                        isTouchBottom = true;
                        break;
                    case "Left":
                        isTouchLeft = true;
                        break;
                    case "Right":
                        isTouchRight = true;
                        break;
                }
            }

            else if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
            {
                if (isRespawnTime) //무적때 피격 방지
                    return;
                if (isHit) //맞은 상태에서 중복 피격 방지
                    return;

                isHit = true;
                life--;
                gameManager.UpdateLifeIcon(life);
                gameManager.CallExplosion(transform.position, "P");

                if (life == 0)
                {
                    gameManager.GameOver();
                }
                else
                {
                    gameManager.RespawnPlayer();
                }

                gameObject.SetActive(false);
                if (collision.gameObject.tag == "Enemy")
                {
                    GameObject bossGo = collision.gameObject;
                    Enemy enemyBoss = bossGo.GetComponent<Enemy>();
                    if (enemyBoss.enemyName == "B")
                    {
                        return;
                    }
                    else
                    {
                        collision.gameObject.SetActive(false);
                    }
                }
            }

            else if (collision.gameObject.tag == "Item")
            {
                Item item = collision.gameObject.GetComponent<Item>();
                switch (item.type)
                {
                    case "Coin":
                        //score += 1000;

                        // 세션 코인 증가
                        GameSession.Coins++;

                        // DB 업데이트 요청 (LoginApi.aspx coinOp=update)
                        StartCoroutine(CoUpdateCoin(GameSession.Coins));

                        // 이번 판 합계
                        gameManager.AddCoin();
                        break;

                    case "Power":
                        if (power == maxPower)
                            score += 500;
                        else
                        {
                            power++;
                            AddFollower();
                        }
                        break;

                    case "Boom":
                        if (boom == maxBoom)
                            score += 500;
                        else
                        {
                            boom++;
                            gameManager.UpdateBoomIcon(boom);
                        }
                        break;

                }
                collision.gameObject.SetActive(false);
            }
        }

        void OffBoomEffect()
        {
            boomEffect.SetActive(false);
            isBoomTime = false;
        }

        void AddFollower()
        {
            if (power == 4)
            {
                followers[0].SetActive(true);
            }
            else if (power == 5)
            {
                followers[1].SetActive(true);
            }
            else if (power == 6)
            {
                followers[2].SetActive(true);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Border")
            {
                switch (collision.gameObject.name)
                {
                    case "Top":
                        isTouchTop = false;
                        break;
                    case "Bottom":
                        isTouchBottom = false;
                        break;
                    case "Left":
                        isTouchLeft = false;
                        break;
                    case "Right":
                        isTouchRight = false;
                        break;
                }
            }
        }

        private IEnumerator CoUpdateCoin(int newCoin)
        {
            string url = ApiConfig.BaseUrlApi + "LoginApi.aspx";

            // DTO 객체 생성
            CoinRequest reqObj = new CoinRequest
            {
                userId = GameSession.UserId,
                password = GameSession.Password,
                coinOp = "update",
                currentCoin = newCoin
            };

            string json = JsonUtility.ToJson(reqObj);
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
                    Debug.Log("[Coin Update] 성공: " + req.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("[Coin Update] 실패: " + req.error);
                    Debug.LogError("[Coin Update] 응답: " + req.downloadHandler.text);
                }
            }

        }

        private class DevOnly_BypassCertificate : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }

        public class CoinRequest
        {
            public string userId;
            public string password;
            public string coinOp;
            public int currentCoin;
        }


    }

}
