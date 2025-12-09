using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserID"] != null) // 로그인 상태
                {
                    lblWelcome.Text = $"{Session["NickName"]}";
                    lblCoin.Text = GetUserCoin(Session["UserID"].ToString()).ToString();

                    btnSignup.Visible = false;
                    btnLogin.Visible = false;
                    btnLogout.Visible = true;

                    btnAdmin.Visible = (Session["UGrade"]?.ToString() == "1"); //관리자면 Admin버튼 표시
                }
                else // 비로그인 상태
                {
                    lblWelcome.Text = "";
                    lblCoin.Text = "0";

                    btnSignup.Visible = true;
                    btnLogin.Visible = true;
                    btnLogout.Visible = false;
                    btnAdmin.Visible = false;
                }
            }
        }

        protected void btnLogout_Click(object sender, ImageClickEventArgs e)
        {
            Session.Clear();
            Response.Redirect("Default.aspx");
        }

        protected void btnAdmin_Click(object sender, ImageClickEventArgs e)
        {

        }

        private int GetUserCoin(string userId)
        {
            int coin = 0;
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
            string sql = "SELECT Coin FROM Members WHERE UserID = @UserID";

            using (var con = new System.Data.SqlClient.SqlConnection(connStr))
            using (var cmd = new System.Data.SqlClient.SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    coin = Convert.ToInt32(result);
            }
            return coin;
        }
    }
}