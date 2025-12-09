using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game_Run
{
    public class ChnageObject : MonoBehaviour
    {
        public GameObject[] objs;

        // 장애물 오브젝트 랜덤 스프라이트
        public void Change()
        {
            int ran = Random.Range(0, objs.Length);

            for (int index = 0; index < objs.Length; index++)
            {
                transform.GetChild(index).gameObject.SetActive(ran == index);
            }
        }
    }
}

