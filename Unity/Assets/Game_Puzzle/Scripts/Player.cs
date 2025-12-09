using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Costume Set")]
    public CostumeElement[] costumes;
    public int equippedItemId;

    [Header("Action Set")]
    public ActionElement[] actions;
    public int equippedActionId;

    // 오브젝트 참조
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // PlayerPrefs에서 로그인 시 저장된 장착 아이템 ID 읽기
        equippedItemId = PlayerPrefs.GetInt("UsingPuzzleCostume", 0);
        ApplyCostume();

        // 액션 아이템 적용
        equippedActionId = PlayerPrefs.GetInt("UsingPuzzleAction", 0);
        ApplyAction();
    }

    private void Update()
    {
        FollowMouse();
    }

    void ApplyCostume()
    {
        foreach (var element in costumes)
        {
            if (element.itemId == equippedItemId)
            {
                spriteRenderer.sprite = element.sprite;
                break;
            }
        }
    }

    void ApplyAction()
    {
        // 모든 액션 스크립트 비활성화
        foreach (var element in actions)
        {
            if (element.script != null)
            {
                element.script.enabled = false;
                // 버튼 꺼주기
                var hammer = element.script as Hammer;
                if (hammer != null && hammer.itemButton != null)
                    hammer.itemButton.gameObject.SetActive(false);
            }
        }

        // 장착된 ItemID와 일치하는 스크립트만 활성화
        foreach (var element in actions)
        {
            if (element.itemId == equippedActionId && element.script != null)
            {
                element.script.enabled = true;
                // 버튼 켜기
                var hammer = element.script as Hammer;
                if (hammer != null && hammer.itemButton != null)
                    hammer.itemButton.gameObject.SetActive(true);
                break;
            }
        }
    }

    void FollowMouse()
    {
        // 마우스 위치 가져오기
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // X축 경계 설정 (Dongle.cs 참고)
        float leftBorder = -5.5f + transform.localScale.x / 2f;
        float rightBorder = +5.5f - transform.localScale.x / 2f;

        if (mousePos.x < leftBorder) mousePos.x = leftBorder;
        else if (mousePos.x > rightBorder) mousePos.x = rightBorder;

        // Y, Z 고정
        mousePos.y = 8.2f;   // 씬에 배치한 Player 오브젝트 Y 좌표
        mousePos.z = 0;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
    }

    [System.Serializable]
    public class CostumeElement
    {
        // 아이템 ID (DB와 매칭)
        public int itemId;
        // 교체할 스프라이트
        public Sprite sprite;
    }

    [System.Serializable]
    public class ActionElement
    {
        public int itemId;
        public string actionName;
        public MonoBehaviour script;
    }

}
