using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game_Run
{
    public class CoinManager : MonoBehaviour
    {
        [Header("프리팹 및 참조")]
        public GameObject coinPrefab;
        public Transform slotsRoot;
        public Transform player;

        [Header("스폰 설정 (0~1 사이")]
        [Range(0f, 1f)]
        public float spawnRate = 0.3f;

        [Header("이동 및 제거 설정")]
        // 코인 이동 속도 배율
        public float coinSpeedMul = 1.0f;
        // 코인이 화면 왼쪽에서 제거되는 위치
        public float despawnX = -15f;

        [Header("충돌 판정")]
        // 플레이어와의 충돌 거리 반경
        public float pickupRadius = 0.5f;

        [Header("런타임 관리")]
        // 현재 씬에서 살아있는 코인 리스트
        private List<Transform> activeCoins = new List<Transform>();
        private Transform[] slots;

        [Header("획득 코인 텍스트")]
        // 게임 내 코인 UI (좌측 하단)
        public Text txtCoinInGame;

        public static CoinManager instance;

        public static int earnedCoin = 0;
        private bool isRespawning = false;

        void Awake()
        {
            instance = this;

            // 슬롯 자식들을 배열로 저장
            int count = slotsRoot.childCount;
            slots = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = slotsRoot.GetChild(i);
            }

            // 플레이어를 찾지 못했다면 자동으로 Player 태그 검색
            if (player == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                if (found != null)
                {
                    player = found.transform;
                }
            }
        }

        void Update()
        {
            // 게임이 진행 중이 아니라면 아무것도 하지 않음
            if (!GameManager.isLive)
                return;

            // 코인 이동 처리
            float dx = GameManager.globalSpeed * coinSpeedMul * Time.deltaTime * -1f;

            // 모든 활성 코인 확인
            for (int i = activeCoins.Count - 1; i >= 0; i--)
            {
                // 현재 코인 Transform
                Transform t = activeCoins[i];

                // 코인이 이미 제거됐다면 리스트에서 삭제
                if (t == null)
                {
                    activeCoins.RemoveAt(i);
                    continue;
                }

                // 코인을 왼쪽으로 이동
                t.Translate(dx, 0f, 0f, Space.World);

                // 화면 왼쪽 경계 넘으면 제거
                if (t.position.x <= despawnX)
                {
                    Destroy(t.gameObject);
                    activeCoins.RemoveAt(i);
                    continue;
                }

                // 플레이어가 존재하면 거리 체크
                if (player != null)
                {
                    // 플레이어와 코인의 거리 계산
                    float dist = Vector2.Distance(player.position, t.position);

                    // 일정 반경 이내면 코인 획득 처리
                    if (dist <= pickupRadius)
                    {
                        if (t.CompareTag("Coin"))
                        {
                            // 코인 수치 증가
                            earnedCoin++;

                            // UI 텍스트 즉시 반영
                            if (txtCoinInGame != null)
                                txtCoinInGame.text = earnedCoin.ToString();

                            // 코인 제거
                            Destroy(t.gameObject);
                            activeCoins.RemoveAt(i);
                        }
                    }
                }
            }

            if (activeCoins.Count == 0 && !isRespawning)
            {
                StartCoroutine(RespawnDelay());
            }
        }

        // 외부에서 호출되는 스폰 함수
        public void SpawnCoin()
        {
            // 기존 코인 모두 제거
            for (int i = activeCoins.Count - 1; i >= 0; i--)
            {
                if (activeCoins[i] != null)
                {
                    Destroy(activeCoins[i].gameObject);
                }
            }

            // 리스트 비우기
            activeCoins.Clear();

            // 슬롯마다 새 코인 생성 시도
            foreach (Transform slot in slots)
            {
                // 확률에 따라 코인 생성
                if (Random.value < spawnRate)
                {
                    // 슬롯 좌표에 코인 프리팹 생성
                    GameObject coin = Instantiate(coinPrefab, slot.position, Quaternion.identity, slotsRoot);

                    // 활성 리스트에 등록
                    activeCoins.Add(coin.transform);
                }
            }
        }

        private IEnumerator RespawnDelay()
        {
            isRespawning = true;
            yield return new WaitForSeconds(2.0f);
            SpawnCoin();
            isRespawning = false;
        }
    }
}