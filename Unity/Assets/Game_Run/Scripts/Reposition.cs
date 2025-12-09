using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Game_Run
{
    public class Reposition : MonoBehaviour
    {
        public UnityEvent onMove;

        // 스프라이트 위치가 -? 보다 작아지면 앞으로 땡겨옴
        [Header("Per-Object Settings")]
        [SerializeField] float resetThresholdX = -10f;
        [SerializeField] float jumpDistanceX = 24f;

        void LateUpdate()                          // 프레임 끝에서 위치 보정
        {
            if (transform.position.x > resetThresholdX) // 월드 좌표의 X가 임계값보다 크면
                return;                             // 아직 되돌릴 시점이 아니므로 종료

            onMove.Invoke();                                        // 되돌림 직후 이벤트 호출 (예: 스폰/랜덤 변경 등)
            transform.Translate(jumpDistanceX, 0f, 0f, Space.Self); // 설정한 거리만큼 +X로 이동 (개별 적용)
        }
    }
}

