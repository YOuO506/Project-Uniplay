using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite
{
    public partial class Ranking : System.Web.UI.Page
    {

        string cs = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGames();                         // 게임 드롭다운 채우기
                if (ddlGame.Items.Count > 0)
                {
                    // 첫 진입: 첫 번째 게임 랭킹 표시
                    BindRanking(int.Parse(ddlGame.Items[0].Value));
                }
            }
        }

        void BindGames()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT GameID, GameName FROM Games ORDER BY GameID", con))
            {
                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                ddlGame.DataSource = dt;
                ddlGame.DataValueField = "GameID";
                ddlGame.DataTextField = "GameName";
                ddlGame.DataBind();
            }
        }

        void BindRanking(int gameId)
        {
            // ERD: GameScores(UserID, GameID, MaxScore, RecordTime)
            string sql = @"
                SELECT
                       ROW_NUMBER() OVER (ORDER BY GS.MaxScore DESC, GS.RecordTime ASC) AS Rank,
                       M.NickName,
                       GS.MaxScore,
                       GS.RecordTime
                FROM GameScores GS
                JOIN Members M ON M.UserID = GS.UserID
                WHERE GS.GameID = @G
                ORDER BY GS.MaxScore DESC, GS.RecordTime ASC;";

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@G", gameId);
                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvRank.DataSource = dt;
                gvRank.DataBind();
            }
        }

        protected void ddlGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindRanking(int.Parse(ddlGame.SelectedValue));
        }
    }
}