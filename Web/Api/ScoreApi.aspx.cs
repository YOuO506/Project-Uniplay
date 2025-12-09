using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite.Api
{
    public partial class ScoreApi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Charset = "utf-8";

            try
            {
                // 1) 요청 JSON 읽기
                string body = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
                var serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<ScoreRequest>(body);

                if (string.IsNullOrEmpty(data.userId) || string.IsNullOrEmpty(data.password))
                {
                    WriteError("missing_param", "userId와 password는 필수입니다.");
                    return;
                }

                string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    // 2) 유저 검증
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Members WHERE UserID=@UID AND PassWd=@PW", con))
                    {
                        cmd.Parameters.AddWithValue("@UID", data.userId);
                        cmd.Parameters.AddWithValue("@PW", data.password);

                        int cnt = (int)cmd.ExecuteScalar();
                        if (cnt == 0)
                        {
                            WriteError("auth_failed", "아이디 또는 비밀번호가 올바르지 않습니다.");
                            return;
                        }
                    }

                    // 3) 점수 갱신
                    int newMaxScore = data.score;

                    // 먼저 현재 기록 확인
                    using (var cmd = new SqlCommand(
                        "SELECT MaxScore FROM GameScores WHERE UserID=@UID AND GameID=@GID", con))
                    {
                        cmd.Parameters.AddWithValue("@UID", data.userId);
                        cmd.Parameters.AddWithValue("@GID", data.gameId);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int oldScore = Convert.ToInt32(result);
                            if (data.score > oldScore)
                            {
                                // UPDATE
                                using (var upCmd = new SqlCommand(
                                    "UPDATE GameScores SET MaxScore=@S, RecordTime=GETDATE() WHERE UserID=@UID AND GameID=@GID", con))
                                {
                                    upCmd.Parameters.AddWithValue("@S", data.score);
                                    upCmd.Parameters.AddWithValue("@UID", data.userId);
                                    upCmd.Parameters.AddWithValue("@GID", data.gameId);
                                    upCmd.ExecuteNonQuery();
                                }
                                newMaxScore = data.score;
                            }
                            else
                            {
                                newMaxScore = oldScore; // 더 낮으면 유지
                            }
                        }
                        else
                        {
                            // INSERT
                            using (var insCmd = new SqlCommand(
                                "INSERT INTO GameScores(UserID, GameID, MaxScore, RecordTime) VALUES(@UID, @GID, @S, GETDATE())", con))
                            {
                                insCmd.Parameters.AddWithValue("@UID", data.userId);
                                insCmd.Parameters.AddWithValue("@GID", data.gameId);
                                insCmd.Parameters.AddWithValue("@S", data.score);
                                insCmd.ExecuteNonQuery();
                            }
                            newMaxScore = data.score;
                        }
                    }

                    // 4) 성공 응답
                    var res = new
                    {
                        success = true,
                        userId = data.userId,
                        gameId = data.gameId,
                        newMaxScore = newMaxScore
                    };
                    Response.Write(serializer.Serialize(res));
                }
            }
            catch (Exception ex)
            {
                WriteError("server_error", ex.Message);
            }
            finally
            {
                Response.End();
            }
        }

        // 요청 DTO
        private class ScoreRequest
        {
            public string userId { get; set; }
            public string password { get; set; }
            public int gameId { get; set; }
            public int score { get; set; }
        }

        // 에러 JSON 출력
        private void WriteError(string code, string message)
        {
            var serializer = new JavaScriptSerializer();
            var err = new { success = false, error = code, message = message };
            Response.StatusCode = 400;
            Response.Write(serializer.Serialize(err));
        }
    }
}