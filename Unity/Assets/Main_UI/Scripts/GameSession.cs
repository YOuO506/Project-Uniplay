using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    public static string UserId;
    public static string NickName;
    public static int UGrade;
    public static int Coins;
    public static string Password;

    public static int ShootingHighScore;
    public static int RunHighScore;
    public static int PuzzleHighScore;


    public static bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

    /*
    // PlayerPrefs 저장
    public static void SaveToPrefs()                               
    {
        if (!string.IsNullOrEmpty(UserId))                         // 유저ID가 있으면
        {
            PlayerPrefs.SetString("UserID", UserId);               // 유저ID 저장
            PlayerPrefs.SetString("NickName", NickName ?? "");     // 닉네임 저장(널 방지)
            PlayerPrefs.SetInt("UGrade", UGrade);                  // 등급 저장
            PlayerPrefs.Save();                                    // 디스크에 반영
        }
    }

    // PlayerPrefs 복원
    public static void LoadFromPrefs()                             
    {
        if (PlayerPrefs.HasKey("UserID"))                          // 키가 존재하면
        {
            UserId = PlayerPrefs.GetString("UserID", "");        // 유저ID 로드
            NickName = PlayerPrefs.GetString("NickName", "");      // 닉네임 로드
            UGrade = PlayerPrefs.GetInt("UGrade", 0);            // 등급 로드
        }
        else                                                       // 키가 없으면
        {
            Clear();                                               // 완전 초기화
        }
    }
    */

    public static void Clear()
    {
        UserId = null;
        NickName = null;
        UGrade = 0;
        Coins = 0;
        Password = null;

        ShootingHighScore = 0;
        RunHighScore = 0;
        PuzzleHighScore = 0;

        // PlayerPrefs 캐시 초기화 (계정 전환 시 아이템 흔적 제거)
        PlayerPrefs.DeleteKey("UsingShootingCostume");
        PlayerPrefs.DeleteKey("UsingRunCostume");
        PlayerPrefs.DeleteKey("UsingPuzzleCostume");
        PlayerPrefs.Save();

        //PlayerPrefs.DeleteKey("UserID");                           // 저장된 유저ID 삭제
        //PlayerPrefs.DeleteKey("NickName");                         // 저장된 닉네임 삭제
        //PlayerPrefs.DeleteKey("UGrade");                           // 저장된 등급 삭제
        //PlayerPrefs.Save();                                        // 디스크 반영
    }
}

