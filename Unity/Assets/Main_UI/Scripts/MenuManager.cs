using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace MenuManager
{
    public class MenuManager : MonoBehaviour
    {
        [Header("----------------[ GameButton ]")]
        public Button btnShootingGame;
        public Button btnRunGame;
        public Button btnPuzzleGame;

        [Header("----------------[ WebButton ]")]
        public Button btnUniPlay;
        public Button btnNotice;
        public Button btnStore;
        public Button btnRanking;
        public Button btnSignup;

        [Header("----------------[ UnityButton ]")]
        public Button btnLogin;
        public Button btnMyItem;

        [Header("----------------[ UI ]")]
        public GameObject loginMenu;
        public GameObject itemMenu;
        public GameObject gameList;
        public Text loginMessageText;

        [Header("----------------[ Manager ]")]
        public LoginManager loginManager;
        public InventoryManager inventoryManager;

        void Start()
        {
            // -------------------- GameButton 해당 게임으로 이동 --------------------
            btnShootingGame.onClick.AddListener(() =>
            {
                // 로그인 안된 상태
                if (!GameSession.IsLoggedIn)
                {
                    loginMessageText.gameObject.SetActive(true);
                    loginMessageText.text = "Please Login !";
                    // 5초 후 자동으로 비활성화
                    StartCoroutine(HideLoginMessage());
                    return;
                }
                // 로그인 된 경우만 씬 전환
                SceneManager.LoadScene("Shooting_Stage");
            });

            btnRunGame.onClick.AddListener(() =>
            {
                if (!GameSession.IsLoggedIn)
                {
                    loginMessageText.gameObject.SetActive(true);
                    loginMessageText.text = "Please Login !";
                    // 5초 후 자동으로 비활성화
                    StartCoroutine(HideLoginMessage());
                    return;
                }
                SceneManager.LoadScene("Run_Stage");
            });

            btnPuzzleGame.onClick.AddListener(() =>
            {
                if (!GameSession.IsLoggedIn)
                {
                    loginMessageText.gameObject.SetActive(true);
                    loginMessageText.text = "Please Login !";
                    // 5초 후 자동으로 비활성화
                    StartCoroutine(HideLoginMessage());
                    return;
                }
                SceneManager.LoadScene("Puzzle_Stage");
            });

            // -------------------- WebButton 웹 페이지로 이동 --------------------
            btnUniPlay.onClick.AddListener(() =>
            {
                // 유니플레이 메인 페이지 (시작화면)
                Application.OpenURL(ApiConfig.BaseUrlWeb + "Default.aspx");
            });

            btnNotice.onClick.AddListener(() =>
            {
                // 공지사항 페이지
                Application.OpenURL(ApiConfig.BaseUrlWeb + "Notice.aspx");
            });

            btnStore.onClick.AddListener(() =>
            {
                // 상점 페이지
                Application.OpenURL(ApiConfig.BaseUrlWeb + "Store.aspx");
            });

            btnRanking.onClick.AddListener(() =>
            {
                // 랭킹 페이지
                Application.OpenURL(ApiConfig.BaseUrlWeb + "Ranking.aspx");
            });

            btnSignup.onClick.AddListener(() =>
            {
                // 회원가입 페이지
                Application.OpenURL(ApiConfig.BaseUrlWeb + "Signup.aspx");
            });


            // 로그인 버튼 클릭 시 로그인 패널 띄움
            btnLogin.onClick.AddListener(() =>
            {
                bool isActive = !loginMenu.activeSelf;
                loginMenu.SetActive(isActive);

                // 만약 로그인 메뉴를 키는 중이라면 → 다른 메뉴 끄기
                if (isActive)
                {
                    itemMenu.SetActive(false);      // MyItem 메뉴 꺼주기
                    gameList.SetActive(true);       // 게임 리스트 다시 켜주기
                }
            });

            // 인벤토리 버튼 클릭 시 인벤토리 패널 띄움
            btnMyItem.onClick.AddListener(() =>
            {
                // 로그인 안된 상태
                if (!GameSession.IsLoggedIn)
                {
                    loginMessageText.gameObject.SetActive(true);
                    loginMessageText.text = "Please Login !";
                    // 5초 후 자동으로 비활성화
                    StartCoroutine(HideLoginMessage());
                    return;
                }

                // 로그인 된 경우만
                
                bool isActive = !itemMenu.activeSelf;
                itemMenu.SetActive(isActive);

                loginMenu.SetActive(false);
                gameList.SetActive(!isActive);

                // 패널 열 때만 인벤토리 불러오기
                if (isActive)
                {
                    inventoryManager.ShowBoard(1);

                    // parent 인자 제거. 게임ID만 넘김 (예: 기본 Shooting=1)
                    inventoryManager.LoadInventory(1);

                    // 기본으로 Action 탭 보이게
                    inventoryManager.ShowShootingAction();

                    // 패널 열자마자 숫자 반영
                    inventoryManager.RefreshCoinUI();
                }
                else
                {
                    // 인벤토리 패널 숨기기
                    inventoryManager.inventoryPanel.SetActive(false);
                }
            });
        }

        // 로그인 상태를 UI에 반영하는 공통 함수
        private void SyncLoginUI()                                     
        {
            // 로그인 여부 확인
            bool loggedIn = GameSession.IsLoggedIn;
            // 로그인 상태면 로그인 패널 숨김
            if (loginMenu != null) loginMenu.SetActive(false);
            // 게임 리스트는 항상 표시(상황에 맞게 유지)
            if (gameList != null) gameList.SetActive(true);

            // 인벤토리 패널은 버튼으로만 토글하므로 여기서 건드리지 않음
        }

        private IEnumerator HideLoginMessage()
        {
            // 5초 대기
            yield return new WaitForSeconds(5f);
            loginMessageText.gameObject.SetActive(false);
        }
    }
}
