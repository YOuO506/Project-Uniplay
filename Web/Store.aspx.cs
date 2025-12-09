using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite
{

    /// <summary>
    /// 상점 페이지
    /// - 게임/아이템 유형/아이템 목록 바인딩
    /// - 구매 처리 (코인 차감 + 보유 등록)
    /// - 관리자 전용 : 아이템 삭제 링크/ 버튼 표시 및 삭제 처리
    /// </summary>

    public partial class Store : System.Web.UI.Page
    {

        // DB 연결
        string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

        // 세션 사용자가 관리자인지 확인
        bool IsAdmin() => Session["UGrade"]?.ToString() == "1";

        // 최초 페이지 로드 시 실행
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                btnAdminItems.Visible = IsAdmin();   // ← 관리자만 상단 패널 보이기

                BindGames();
                BindTypes();

                // 첫 로드시 기본값: GameID=1(Shooting), ItemTypeID=1(Action)
                hidGameID.Value = "1";
                hidTypeID.Value = "1";

                BindItems();
            }

        }

        // 좌측 세로 탭 : 게임 목록 바인딩 (GameID 오름차순)
        void BindGames()
        {
            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT GameID, GameName FROM Games ORDER BY GameID ASC", con))
            {
                con.Open();
                rptGames.DataSource = cmd.ExecuteReader();
                rptGames.DataBind();
            }
        }

        // 상단 가로 탭 : 아이템 유형 목록 바인딩 (ItemTypeID 오름차순)
        void BindTypes()
        {
            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT ItemTypeID, ItemTypeName FROM ItemTypes ORDER BY ItemTypeID ASC", con))
            {
                con.Open();
                rptTypes.DataSource = cmd.ExecuteReader();
                rptTypes.DataBind();
            }
        }

        /// <summary>
        /// 본문 카드 그리드 : 아이템 목록 바인딩
        /// - 선택된 Game/Type 필터 적용
        /// - 현재 유저(UserItems)와 LEFT JOIN해 Owned(보유 여부) 계산
        /// - ImagePath 없으면 placeholder로 대체
        /// </summary>
        void BindItems()
        {
            string gameId = hidGameID.Value;
            string typeId = hidTypeID.Value;
            string userId = Session["UserID"] as string ?? ""; // 로그인 안했으면 빈문자

            string sql = @"
                SELECT I.GameID, I.ItemID, I.ItemName, I.Price,
                       COALESCE(NULLIF(I.ImagePath,''),'Images/Items/item_placeholder.png') AS ImagePath,
                       G.GameName, T.ItemTypeName,
                       CAST(CASE WHEN UI.UserID IS NULL THEN 0 ELSE 1 END AS bit) AS Owned
                FROM Items I
                JOIN Games G      ON G.GameID      = I.GameID
                JOIN ItemTypes T  ON T.ItemTypeID  = I.ItemTypeID
                LEFT JOIN UserItems UI
                  ON UI.UserID = @UserID AND UI.GameID = I.GameID AND UI.ItemID = I.ItemID
                WHERE (@GameID = '' OR I.GameID = @GameID)
                  AND (@TypeID = '' OR I.ItemTypeID = @TypeID)
                ORDER BY G.GameName, T.ItemTypeName, I.ItemName;";

            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@GameID", (object)gameId ?? "");
                cmd.Parameters.AddWithValue("@TypeID", (object)typeId ?? "");
                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                rptItems.DataSource = dt;
                rptItems.DataBind();
            }
        }

        // 좌측 게임 탭 클릭 시 : 선택 GameID 저장 후 아이템 재바인딩
        protected void rptGames_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "FilterGame")
            {
                hidGameID.Value = e.CommandArgument.ToString();
                BindItems();
            }
        }

        // 상단 유형 탭 클릭 시 : 선택 TypeID 저장 후 아이템 재바인딩
        protected void rptTypes_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "FilterType")
            {
                hidTypeID.Value = e.CommandArgument.ToString();
                BindItems();
            }
        }

        // 현재 유저가 특정 (GameID, ItemID)를 이미 보유했는지, 중복 구매 최종 차단용 서버 검증
        bool AlreadyOwned(string userId, int gameId, int itemId)
        {
            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM UserItems WHERE UserID=@U AND GameID=@G AND ItemID=@I", con))
            {
                cmd.Parameters.AddWithValue("@U", userId);
                cmd.Parameters.AddWithValue("@G", gameId);
                cmd.Parameters.AddWithValue("@I", itemId);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        /// <summary>
        /// 아이템 카드 내 버튼 동작
        /// - Buy : 로그인/보유/가격 확인 -> 트랜잭션 구매 -> 새로고침
        /// - DeleteItem : 관리자만, UserItems 정리 후 Items 삭제
        /// </summary>
        protected void rptItems_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Buy")
            {
                if (Session["UserID"] == null) { Alert("로그인 후 이용해주세요."); return; }

                var parts = e.CommandArgument.ToString().Split('|');
                int gameId = int.Parse(parts[0]);
                int itemId = int.Parse(parts[1]);
                string userId = Session["UserID"].ToString();

                if (AlreadyOwned(userId, gameId, itemId))
                {
                    Alert("이미 보유한 아이템입니다.");
                    return;
                }

                // 가격 가져오기
                var price = GetItemPrice(gameId, itemId);
                if (price == null) { Alert("아이템 정보를 찾을 수 없습니다."); return; }

                var result = Purchase(userId, gameId, itemId, price.Value);
                if (result == "OK") 
                { 
                    Alert("구매 완료!");
                    // 새로고침(마스터 코인 표시 갱신)
                    Response.Redirect(Request.RawUrl, false);
                }

                else if (result == "COIN") 
                {
                    Alert("코인이 부족합니다."); 
                }

                else 
                { 
                    Alert("오류가 발생했습니다."); 
                }

            }

        }

        // 각 카드가 그려질 때 삭제 버튼(Admin 전용)
        protected void rptItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var btnDel = e.Item.FindControl("btnDelete") as Button;
            if (btnDel != null)
            {
                btnDel.Visible = IsAdmin();                         // 관리자만 Delete 보이기
                btnDel.OnClientClick = "return confirm('정말 삭제하시겠습니까?');";
            }
        }

        // 개별 아이템 가격 조회(Items.Price)
        int? GetItemPrice(int gameId, int itemId)
        {
            using (var con = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT Price FROM Items WHERE GameID=@G AND ItemID=@I", con))
            {
                cmd.Parameters.AddWithValue("@G", gameId);
                cmd.Parameters.AddWithValue("@I", itemId);
                con.Open();
                var o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? (int?)null : Convert.ToInt32(o);
            }
        }

        /// <summary>
        /// 구매 트랜잭션
        /// - 코인 확인 (부족 시 COIN)
        /// - 코인 차감 (실패 시 ERR)
        /// - UserItems 보유 등록 (실패 시 ERR)
        /// - 성공 시 OK
        /// </summary>
        string Purchase(string userId, int gameId, int itemId, int price)
        {
            using (var con = new SqlConnection(connStr))
            {
                con.Open();
                using (var tr = con.BeginTransaction())
                {
                    try
                    {
                        // 코인 확인
                        var getCoin = new SqlCommand("SELECT Coin FROM Members WHERE UserID=@U", con, tr);
                        getCoin.Parameters.AddWithValue("@U", userId);
                        int coin = Convert.ToInt32(getCoin.ExecuteScalar() ?? 0);
                        if (coin < price) { tr.Rollback(); return "COIN"; }

                        // 차감
                        var upd = new SqlCommand("UPDATE Members SET Coin = Coin - @P WHERE UserID=@U", con, tr);
                        upd.Parameters.AddWithValue("@P", price);
                        upd.Parameters.AddWithValue("@U", userId);
                        if (upd.ExecuteNonQuery() != 1) { tr.Rollback(); return "ERR"; }

                        // 보유 등록 (코스튬 중복 방지는 나중에 정책 정하면 추가)
                        var ins = new SqlCommand(@"
                            INSERT INTO UserItems(UserID, GameID, ItemID, ItemTypeID, PurchaseDate) 
                            SELECT @U, @G, @I, I.ItemTypeID, GETDATE() 
                            FROM Items I 
                            WHERE I.GameID = @G AND I.ItemID = @I;
                        ", con, tr);
                        ins.Parameters.AddWithValue("@U", userId);
                        ins.Parameters.AddWithValue("@G", gameId);
                        ins.Parameters.AddWithValue("@I", itemId);
                        if (ins.ExecuteNonQuery() != 1) { tr.Rollback(); return "ERR"; }

                        tr.Commit();
                        return "OK";
                    }
                    catch { tr.Rollback(); return "ERR"; }
                }
            }
        }

        // 알림창(브라우저 alert) 헬퍼
        void Alert(string msg)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "a", $"alert('{msg}');", true);
        }

        protected void btnAdminItems_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminItems.aspx");
        }
    }
}