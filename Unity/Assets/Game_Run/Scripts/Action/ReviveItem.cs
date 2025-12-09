using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_Run
{
    public class ReviveItem : MonoBehaviour
    {
        [Header("무적 지속 시간 (초)")]
        // 무적 유지 시간 초 단위
        public float invincibleTime = 3f;

        // GameOver 차단 플래그  GameManager에서 한 줄로 가드할 때 사용
        public static bool BlockGameOver = false;

        // 한 번만 사용되도록 제어
        private bool used = false;

        // 무적 상태 여부
        private bool isInvincible = false;

        // 필요한 컴포넌트 캐싱
        private Rigidbody2D rigid;
        private Animator anim;
        private SpriteRenderer sr;

        void Awake()
        {
            var player = FindObjectOfType<Player>();
            rigid = player.GetComponent<Rigidbody2D>();
            anim = player.GetComponent<Animator>();
            sr = player.GetComponent<SpriteRenderer>();
        }

        // Player의 onHit 이벤트에 연결
        public void OnPlayerHit()
        {
            // 미장착(비활성) 상태면 바로 무시
            if (!this.enabled) return;

            // 이미 한 번 썼으면 무시
            if (used) return;

            used = true;

            // 다음 GameOver 호출을 차단
            BlockGameOver = true;

            // 부활 처리 코루틴 시작
            StartCoroutine(ReviveRoutine());
        }

        private IEnumerator ReviveRoutine()
        {
            // 로그 출력
            Debug.Log("[ReviveItem] 부활 발동");

            // 물리 다시 켜기
            rigid.simulated = true;

            // 속도 초기화로 튕김 방지
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0f;

            // 애니메이션 Run 상태로
            anim.SetInteger("State", (int)Player.State.Run);

            // 무적 시작
            isInvincible = true;

            // 깜빡임 연출 시작
            float t = 0f;
            // 깜빡임 간격 설정
            float blink = 0.2f;

            // 무적 구간 반복
            while (t < invincibleTime)
            {
                // 스프라이트 토글
                if (sr != null) sr.enabled = !sr.enabled;
                // 대기
                yield return new WaitForSeconds(blink);
                // 시간 누적
                t += blink;
            }

            // 스프라이트 원상 복구
            if (sr != null) sr.enabled = true;

            // 무적 종료
            isInvincible = false;

            // 더 이상 GameOver를 막지 않음
            BlockGameOver = false;

            // 한 번만 쓰고 스크립트 비활성화
            this.enabled = false;

            // 로그로 확인
            Debug.Log("[ReviveItem] revive end");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 무적 중 적과 충돌이면 무시
            if (isInvincible && collision.CompareTag("Enemy"))
            {
                // 로그로 확인
                Debug.Log("[ReviveItem] invincible ignore hit");
                // 아무 처리도 하지 않음
                return;
            }
        }
    }
}
