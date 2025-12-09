using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game_Run
{
    public class ChangeSprite : MonoBehaviour
    {
        public Sprite[] sprites;
        SpriteRenderer spriter;

        void Awake()
        {
            spriter = GetComponent<SpriteRenderer>();
            Change();
        }

        // 배경(구름, 산) 랜덤 스프라이트
        public void Change()
        {
            int ran = Random.Range(0, sprites.Length);
            spriter.sprite = sprites[ran];
        }
    }
}

