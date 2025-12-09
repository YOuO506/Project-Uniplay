using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;

namespace UniPlayWebSite
{
    public partial class Login : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            // 엔터키 누르면 btnLogin 클릭되게 설정
            this.Form.DefaultButton = btnLogin.UniqueID;

            if (!IsPostBack)
            {
                // 초기화용 코드
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string id = txtID.Text.Trim();
            string pw = txtPWD.Text.Trim();

            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT NickName, UGrade FROM Members WHERE UserID = @id AND PassWd = @pw";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pw", pw);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // 로그인 성공 → 세션 저장
                    Session["UserID"] = id;
                    Session["NickName"] = reader["NickName"].ToString();
                    Session["UGrade"] = reader["UGrade"].ToString();

                    Response.Redirect("Default.aspx");
                }
                else
                {
                    lblResult.Text = "아이디 또는 비밀번호가 틀렸습니다.";
                    Session.Clear();  // ← 이거 반드시 추가해
                }

                reader.Close();
            }
        }
    }
}