using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Game_Run
{
    public class Player : MonoBehaviour
    {
        public enum State { Stand, Run, Jump, Hit }  // 0.Stand, 1.Run, 2.Jump, 3.Hit

        [Header("Jump Settings")]
        public float startJumpPower;
        public float jumpPower;

        [Header("Runtime Flags")]
        public bool isGround;
        public bool isJumpKey;

        [Header("Events")]
        public UnityEvent onHit;

        Rigidbody2D rigid;
        Animator anim;
        Sounder sound;

        // ============================ 코스튬 관련 (Animator 유지) ============================
        // [FIX] 더 이상 스프라이트 한 장 교체 안 함 → Animator를 "살린다".
        //       그래서 상태(Run/Jump/Hit)에 연결된 "애니메이션 클립"을 코스튬용으로 갈아끼운다.

        [Header("Animator 기본 클립")]
        // [FIX] Animator Controller에서 Run/Jump/Hit 상태에 연결된 '기본' 클립을 드래그
        [SerializeField] AnimationClip baseRun;
        [SerializeField] AnimationClip baseJump;
        [SerializeField] AnimationClip baseHit;

        [System.Serializable]
        public class CostumeClips
        {
            public int itemId;
            public string name;          // 디버그/구분용
            public AnimationClip run;    // 코스튬의 Run 클립
            public AnimationClip jump;   // 코스튬의 Jump 클립
            public AnimationClip hit;    // 코스튬의 Hit 클립
        }

        [System.Serializable]
        public class ActionClips
        {
            public int itemId;
            public string name;
            public MonoBehaviour script; // 기능 스크립트
        }

        [Header("Costume Set")]
        // [FIX] Inspector에서 코스튬 개수만큼 Element 추가하고 각 클립 지정
        [SerializeField] CostumeClips[] costumes;

        [Header("인벤토리에서 저장한 코스튬 인덱스 키")]
        // 키
        [SerializeField] string prefsKey = "UsingRunCostume";

        [Header("Action Set")]
        // 기능형 아이템 세트 (인스펙터에서 Element 배열로 관리)
        [SerializeField] ActionClips[] Actions;

        [Header("인벤토리에서 저장한 액션 인덱스 키")]
        // 인벤토리에서 저장된 장착 값 불러올 키 (게임별 Action용)
        [SerializeField] string prefsKeyFunctional = "UsingRunAction";

        void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sound = GetComponent<Sounder>();

            // ---------- [핵심 FIX] Animator 비활성화 금지 ----------
            // 기존 코드의 if(anim) anim.enabled = false; 제거!
            // -------------------------------------------------------

            // ---------- [핵심 FIX] 코스튬 인덱스 읽어서 애니메이션 클립 갈아끼우기 ----------
            if (anim == null)
            {
                Debug.LogError("[Player] Animator 없음.");
            }
            else
            {
                // ItemID 읽어오기
                int equippedItemId = PlayerPrefs.GetInt(prefsKey, -1);


                // ItemID로 CostumeClips 찾기
                CostumeClips sel = null;
                if (equippedItemId > 0 && costumes != null)
                {
                    foreach (var c in costumes)
                    {
                        if (c.itemId == equippedItemId) { sel = c; break; }   // ★ 변경됨
                    }
                }

                if (sel == null || sel.run == null || sel.jump == null || sel.hit == null
                    || baseRun == null || baseJump == null || baseHit == null)
                {
                    Debug.LogWarning("[Player] 코스튬 없음/세팅 불완전 → 기본 Animator 사용");
                }
                else
                {
                    // ★ 기존 index 방식 → AnimatorOverrideController에 ItemID 매핑된 클립 적용
                    var aoc = new AnimatorOverrideController(anim.runtimeAnimatorController);

                    var map = new List<KeyValuePair<AnimationClip, AnimationClip>>
                    {
                        new KeyValuePair<AnimationClip, AnimationClip>(baseRun,  sel.run),
                        new KeyValuePair<AnimationClip, AnimationClip>(baseJump, sel.jump),
                        new KeyValuePair<AnimationClip, AnimationClip>(baseHit,  sel.hit),
                    };

                    aoc.ApplyOverrides(map);
                    anim.runtimeAnimatorController = aoc;

                    Debug.Log($"[Player] Run 코스튬 적용: ItemID={equippedItemId}");  // ★ 변경됨
                }
            }

            if (Actions != null)
            {
                foreach (var fi in Actions)
                {
                    if (fi.script != null) fi.script.enabled = false;
                }
            }

            // 장착된 액션 아이템 불러오기
            int equippedFunctionalId = PlayerPrefs.GetInt(prefsKeyFunctional, -1);

            // 해당 ItemId와 일치하는 스크립트만 활성화
            if (equippedFunctionalId > 0 && Actions != null)
            {
                foreach (var fi in Actions)
                {
                    if (fi.itemId == equippedFunctionalId && fi.script != null)
                    {
                        fi.script.enabled = true;
                        Debug.Log("[Player] Functional Enabled: " + fi.name + " (ID=" + fi.itemId + ")");
                        break;
                    }
                }
            }

        }

        void Start()
        {
            sound.PlaySound(Sounder.Sfx.Reset);
        }

        // 1A. 점프
        void Update()
        {
            if (!GameManager.isLive)
                return;

            if ((Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(0)) && isGround)
            {
                // 기본 점프 (=숏 점프)
                rigid.AddForce(Vector2.up * startJumpPower, ForceMode2D.Impulse);
            }

            isJumpKey = Input.GetButton("Jump") || Input.GetMouseButton(0);
        }

        // 1B. 롱 점프
        void FixedUpdate()
        {
            if (!GameManager.isLive)
                return;

            // 롱 점프
            if (isJumpKey && !isGround)
            {
                jumpPower = Mathf.Lerp(jumpPower, 0, 0.1f);
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            }
        }

        // 2. 착지 (물리 충돌 이벤트)
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!isGround)
            {
                ChangeAnim(State.Run);
                jumpPower = 1;
                // 5. 사운드
                sound.PlaySound(Sounder.Sfx.Land);
            }

            isGround = true;
        }

        // 점프 시의 변화
        private void OnCollisionExit2D(Collision2D collision)
        {
            ChangeAnim(State.Jump);
            isGround = false;
            // 5. 사운드
            sound.PlaySound(Sounder.Sfx.Jump);
        }

        // 3. 장애물 터치 (트리거 충돌 이벤트)
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 무적(부활) 중엔 어떤 트리거도 무시
            if (Game_Run.ReviveItem.BlockGameOver)
                return;

            if (collision.CompareTag("Coin"))
            {
                CoinManager.earnedCoin++;

                if (CoinManager.instance.txtCoinInGame != null)
                    CoinManager.instance.txtCoinInGame.text = CoinManager.earnedCoin.ToString();

                Destroy(collision.gameObject);
                return;
            }

            if (collision.CompareTag("Enemy"))
            {
                rigid.simulated = false;
                ChangeAnim(State.Hit);
                onHit.Invoke();
                // 5. 사운드
                sound.PlaySound(Sounder.Sfx.Hit);

                // 직접 GameOver 호출하지 않음
                if (!Game_Run.ReviveItem.BlockGameOver)
                {
                    FindObjectOfType<GameManager>().GameOver();
                }
            }
            
        }

        // 4. 애니메이션
        void ChangeAnim(State state)
        {
            // Animator 파라미터 값 변경
            anim.SetInteger("State", (int)state);
        }

    }
}

