using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dongle Destroy Hammer
// 원하는 동글 하나를 파괴하는 아이템

public class Hammer : MonoBehaviour
{
    [Header("UI")]
    public Button itemButton;        // 하이어라키의 ItemButton
    public Text itemCountText;       // 남은 수량 텍스트

    [Header("Input Blocker")]
    public GameObject inputBlocker;

    public static bool IsAiming { get; private set; } = false;
    public static bool BlockClick = false;
    private bool isDestroyMode = false;
    private int itemCount = 3; // 임시 (DB 반영 필요)

    void OnEnable()
    {
        // 버튼동작
        if (itemButton != null)
            itemButton.onClick.AddListener(OnClickItemButton);
        UpdateUI();
    }

    void OnDisable()
    {
        if (itemButton != null)
            itemButton.onClick.RemoveListener(OnClickItemButton);

        EndDestroyMode();
    }

    void OnClickItemButton()
    {
        if (itemCount <= 0) return;

        isDestroyMode = true;
        IsAiming = true;
        if (itemButton) itemButton.interactable = false;          // ★ 조준 중 버튼 잠금
        if (inputBlocker) inputBlocker.SetActive(true);           // ★ UI 클릭을 가로채서 TouchDown/Up 전달 차단

        // ★ 이미 집게가 조각을 ‘잡고’ 있었다면 드래그 상태만 강제 해제(드롭 아님)
        var gm = FindObjectOfType<Game_Puzzle.GameManager>();
        if (gm != null && gm.lastDongle != null)
        {
            gm.lastDongle.isDrag = false;                         // ★ 마우스 따라 움직이지 않게만
            // Drop()는 호출하지 않으므로 실제 드롭/낙하 없음
        }
    }

    void Update()
    {
        if (!isDestroyMode) return;

        // ★ 우클릭/ESC로 조준 취소
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndDestroyMode();                                     // ★
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Dongle"))
            {
                // 이펙트 출력 (풀링된 effect 사용)
                var dongle = hit.collider.GetComponent<Game_Puzzle.Dongle>();
                if (dongle != null && dongle.effect != null)
                {
                    dongle.effect.transform.position = dongle.transform.position;
                    dongle.effect.transform.localScale = dongle.transform.localScale;
                    dongle.effect.Play();
                }

                // 동글 제거
                hit.collider.gameObject.SetActive(false);

                // GameManager가 TouchUp을 실행하지 못하게 막음
                BlockClick = true;

                // 수량 차감
                itemCount--;
                UpdateUI();

                // 파괴 모드 종료
                // isDestroyMode = false;
                EndDestroyMode();
            }
        }
    }

    public void ResetCount()
    {
        itemCount = 3;
        UpdateUI();
        EndDestroyMode();
    }

    void EndDestroyMode()
    {
        if (!isDestroyMode)
        {
            // 그래도 안전하게 UI만 원복
            if (inputBlocker) inputBlocker.SetActive(false);
            if (itemButton) itemButton.interactable = itemCount > 0;
            IsAiming = false;
            return;
        }

        isDestroyMode = false;
        IsAiming = false;
        if (inputBlocker) inputBlocker.SetActive(false);          // UI 이벤트 다시 통과
        if (itemButton) itemButton.interactable = itemCount > 0;  // 남은 수량에 따라 버튼 상태
    }

    void UpdateUI()
    {
        if (itemCountText != null)
            itemCountText.text = "Hammer x" + itemCount;

        if (!isDestroyMode && itemButton != null)                 // 조준 중엔 잠금 유지
            itemButton.interactable = itemCount > 0;
    }
}
