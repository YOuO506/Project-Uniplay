using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Game_Run
{
    public class Score : MonoBehaviour
    {
        public bool isHighScore;

        float highScore;
        Text uiText;

        void Start()
        {
            uiText = GetComponent<Text>();

            if (isHighScore)
            {
                // highScore = PlayerPrefs.GetFloat("Score");
                highScore = GameSession.RunHighScore;
                uiText.text = highScore.ToString("F0");    // float인 text를 문자열로 변환, F0으로 소숫점 짜름
            }
        }

        void LateUpdate()
        {
            if (!GameManager.isLive)
                return;

            // if (isHighScore && GameManager.score < highScore)
            // return;

            if (isHighScore)
            {
                // 최고기록은 항상 GameSession.HighScore로만 표시
                uiText.text = GameSession.RunHighScore.ToString("F0");
            }
            else
            {
                // 현재 스코어는 기존처럼 갱신
                uiText.text = GameManager.score.ToString("F0");
            }
        }
    }
}

