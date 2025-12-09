using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiConfig
{
    /*
#if UNITY_EDITOR
    public const string BaseUrlApi = "https://localhost:44357/Api/";
    public const string BaseUrlWeb = "https://localhost:44357/";
#elif UNITY_ANDROID || UNITY_IOS
    public const string BaseUrlApi = "http://192.168.45.251:14898/Api/";
    public const string BaseUrlWeb = "http://192.168.45.251:14898/";
#else
    public const string BaseUrlApi = "https://localhost:44357/Api/";
    public const string BaseUrlWeb = "https://localhost:44357/";
#endif
    */

    public const string BaseUrlApi = "https://192.168.45.251:44357/Api/";
    public const string BaseUrlWeb = "https://192.168.45.251:44357/";
}
