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
    public partial class NoticeEdit : System.Web.UI.Page
    {

        string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
        string noticeNo;

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["UGrade"] == null || Session["UGrade"].ToString() != "1")
            {
                Response.Redirect("Notice.aspx");
            }


            noticeNo = Request.QueryString["No"];

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(noticeNo))
                {
                    Response.Redirect("Notice.aspx");
                    return;
                }

                LoadNotice(noticeNo);
            }
        }

        private void LoadNotice(string no)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Title, Contents FROM Notice WHERE No = @No";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@No", no);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtTitle.Text = reader["Title"].ToString();
                    txtContents.Text = reader["Contents"].ToString();
                }
                reader.Close();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE Notice SET Title = @Title, Contents = @Contents WHERE No = @No";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@Contents", txtContents.Text.Trim());
                cmd.Parameters.AddWithValue("@No", noticeNo);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            Response.Redirect("NoticeDetail.aspx?No=" + noticeNo);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("NoticeDetail.aspx?No=" + noticeNo);
        }
    }
}