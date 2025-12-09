using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game_Run
{
    public class Scroller : MonoBehaviour
    {
        public int count;
        public float speedRate;

        void Start()
        {
            count = transform.childCount; // 그룹 안의 자식들 몇개있는지 저장
        }

        // 자연스럽게 보이기 위한 속도 개별 지정 기능
        void Update()
        {
            if (!GameManager.isLive)
                return;

            float totalSpeed = GameManager.globalSpeed * speedRate * Time.deltaTime * -1f;
            transform.Translate(totalSpeed, 0, 0);   // x,y,z 축 이동
        }
    }
}

