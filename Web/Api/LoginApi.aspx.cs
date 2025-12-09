using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace UniPlayWebSite.Api
{
    public partial class LoginApi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();                                // 출력 버퍼 초기화
            Response.ContentType = "application/json";       // JSON 응답
            Response.Charset = "utf-8";

            try
            {
                // 1) 요청 Body 읽기
                string body = new System.IO.StreamReader(Request.InputStream).ReadToEnd();

                // 2) JSON → 객체로 변환
                var serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<LoginRequest>(body);

                // 3) 필수 값 확인
                if (string.IsNullOrEmpty(data.userId) || string.IsNullOrEmpty(data.password))
                {
                    WriteError("missing_param", "userId와 password는 필수입니다.");
                    return;
                }

                // 4) DB 연결
                string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    if (data.coinOp == "update")
                    {
                        using (var cmd = new SqlCommand(@"
                            UPDATE Members SET Coin = @CurrentCoin WHERE UserID = @UserID;
                            SELECT Coin FROM Members WHERE UserID = @UserID;
                        ", con))
                        {
                            cmd.Parameters.AddWithValue("@UserID", data.userId);
                            cmd.Parameters.AddWithValue("@CurrentCoin", data.currentCoin);

                            object result = cmd.ExecuteScalar();
                            int newCoin = (result != null) ? Convert.ToInt32(result) : 0;

                            var res = new
                            {
                                success = true,
                                userId = data.userId,
                                coin = newCoin
                            };
                            Response.Write(serializer.Serialize(res));
                        }
                        return;
                    }

                    using (var cmd = new SqlCommand(@"
                        SELECT UserID, 
                               NickName, 
                               UGrade, 
                               ISNULL(Coin, 0) AS Coin
                        FROM Members
                        WHERE UserID=@UserID AND PassWd=@PassWd
                    ", con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", data.userId);
                        cmd.Parameters.AddWithValue("@PassWd", data.password);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {

                                // 코인 값 안전 추출
                                int coin = 0;
                                object coinObj = rd["Coin"];
                                if (coinObj != DBNull.Value) coin = Convert.ToInt32(coinObj);

                                // 로그인 성공 시 현재 장착된 아이템들 조회
                                var equippedItems = new List<object>();
                                using (var equipCmd = new SqlCommand(@"
                                    SELECT UI.GameID, UI.ItemID, IT.ItemTypeID
                                    FROM UserItems UI
                                    INNER JOIN Items IT ON UI.GameID = IT.GameID AND UI.ItemID = IT.ItemID
                                    WHERE UI.UserID = @UserID AND UI.IsEquipped = 1
                                ", con))
                                {
                                    equipCmd.Parameters.AddWithValue("@UserID", rd["UserID"].ToString().Trim());
                                    using (var er = equipCmd.ExecuteReader())
                                    {
                                        while (er.Read())
                                        {
                                            equippedItems.Add(new
                                            {
                                                gameId = er.GetInt32(0),
                                                itemId = er.GetInt32(1),
                                                itemTypeId = er.GetInt32(2)
                                            });
                                        }
                                    }
                                }

                                // 로그인 성공 → JSON 응답
                                var res = new
                                {
                                    success = true,
                                    userId = rd["UserID"].ToString().Trim(),
                                    nickName = rd["NickName"].ToString().Trim(),
                                    uGrade = Convert.ToInt32(rd["UGrade"]),
                                    coin = coin,
                                    equipped = equippedItems
                                };
                                Response.Write(serializer.Serialize(res));
                            }
                            else
                            {
                                // 로그인 실패
                                WriteError("login_failed", "아이디 또는 비밀번호가 올바르지 않습니다.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError("server_error", ex.Message);
            }
            finally
            {
                Response.End(); // 응답 종료
            }
        }

        // 요청 JSON 구조
        private class LoginRequest
        {
            public string userId { get; set; }
            public string password { get; set; }

            public string coinOp { get; set; }
            public int currentCoin { get; set; }
        }

        // 에러 JSON 출력 함수
        private void WriteError(string code, string message)
        {
            var serializer = new JavaScriptSerializer();
            var err = new { success = false, error = code, message = message };
            Response.StatusCode = 400;
            Response.Write(serializer.Serialize(err));
        }

    }
}