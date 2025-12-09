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
    public partial class NoticeDetail : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            
            // 관리자일 때만 수정/삭제 버튼 표시
            if (Session["UGrade"] != null && Session["UGrade"].ToString() == "1")
            {
                pnlEditButtons.Visible = true;
            }

            if (!IsPostBack)
            {
                string no = Request.QueryString["No"];
                if (!string.IsNullOrEmpty(no))
                {
                    LoadNotice(no);
                    IncreaseHits(no);
                }
            }
        }

        private void LoadNotice(string no)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT N.Title, M.NickName, N.UploadTime, N.Hits, N.Contents
                    FROM Notice N
                    JOIN Members M ON N.Author = M.UserID
                    WHERE N.No = @No";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@No", no);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblTitle.Text = reader["Title"].ToString();
                    lblAuthor.Text = reader["NickName"].ToString();
                    lblDate.Text = Convert.ToDateTime(reader["UploadTime"]).ToString("yyyy-MM-dd");
                    lblHits.Text = reader["Hits"].ToString();
                    txtContents.Text = reader["Contents"].ToString();
                }
                reader.Close();
            }
        }

        private void IncreaseHits(string no)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE Notice SET Hits = Hits + 1 WHERE No = @No";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@No", no);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string no = Request.QueryString["No"];
            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM Notice WHERE No = @No";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@No", no);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            Response.Redirect("Notice.aspx");
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            string no = Request.QueryString["No"];
            Response.Redirect("NoticeEdit.aspx?No=" + no);
        }

    }
}