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
    public partial class Community : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCommunityList();
                // 커뮤니티는 모든 등급이 글쓰기 가능하므로 버튼 항상 표시
                btnWrite.Visible = true;
            }
        }

        private void LoadCommunityList()
        {
            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT C.PostID AS No,
                           C.Title,
                           M.NickName AS Author,
                           C.UploadTime,
                           C.Hits,
                           C.CommentCount,
                           C.LikeCount
                    FROM Community C
                    JOIN Members M ON C.Author = M.UserID
                    WHERE C.IsDeleted = 0
                    ORDER BY C.PostID DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCommunity.DataSource = dt;
                gvCommunity.DataBind();
            }
        }

        // 제목 클릭 시 상세 페이지로 이동
        protected void gvCommunity_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetail")
            {
                string postId = e.CommandArgument.ToString();
                Response.Redirect("CommunityDetail.aspx?PostID=" + postId);
            }
        }

        // 글쓰기 버튼 클릭 시 글쓰기 페이지로 이동
        protected void btnWrite_Click(object sender, EventArgs e)
        {
            // 로그인 여부 판단
            if (Session["UserID"] == null)
            {
                // 로그인 후 원래 가려던 곳으로 되돌아오게 returnUrl 추가
                string returnUrl = Server.UrlEncode("CommunityWrite.aspx");
                string loginUrl = "Login.aspx?returnUrl=" + returnUrl;

                string script = $"alert('로그인 후 이용해 주세요.'); window.location.href='{loginUrl}';";

                // ScriptManager가 있으면 그걸로, 없으면 ClientScript로
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "loginRedirect", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "loginRedirect", script, true);

                return;
            }

            // 로그인 상태면 글쓰기 페이지로
            Response.Redirect("CommunityWrite.aspx");
        }
    }
}