using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite
{
    public partial class NoticeWrite : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 관리자만 접근 가능
            if (Session["UGrade"] == null || Session["UGrade"].ToString() != "1")
            {
                Response.Redirect("Notice.aspx");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string contents = txtContents.Text.Trim();
            string author = Session["UserID"]?.ToString();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(contents))
            {
                lblMessage.Text = "제목과 내용을 모두 입력해주세요.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO Notice (Title, Contents, Author, UploadTime, Hits) 
                               VALUES (@title, @contents, @author, GETDATE(), 0)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@contents", contents);
                    cmd.Parameters.AddWithValue("@author", author);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        Response.Redirect("Notice.aspx");
                    }
                    else
                    {
                        lblMessage.Text = "글 저장에 실패했습니다.";
                    }
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Notice.aspx");
        }
    }
}