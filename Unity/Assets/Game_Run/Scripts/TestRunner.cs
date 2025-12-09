using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        PlayerPrefs.SetInt("UsingRunCostume", 0); // 0=Dino, 1=Fox, 2=Uni
        PlayerPrefs.Save();
        Debug.Log(">>> 현재 코스튬 인덱스: " + PlayerPrefs.GetInt("UsingRunCostume"));
    }

    
}
