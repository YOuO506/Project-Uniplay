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
    public partial class Notice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadNoticeList();

                // 관리자만 글쓰기 버튼 보이게
                if (Session["UGrade"] != null && Session["UGrade"].ToString() == "1")
                    btnWrite.Visible = true;
                else
                    btnWrite.Visible = false;
            }
        }

        private void LoadNoticeList()
        {
            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT N.No, N.Title, M.NickName AS Author, N.UploadTime, N.Hits
                    FROM Notice N
                    JOIN Members M ON N.Author = M.UserID
                    ORDER BY N.No DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvNotice.DataSource = dt;
                gvNotice.DataBind();
            }
        }

        protected void gvNotice_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetail")
            {
                string no = e.CommandArgument.ToString();
                Response.Redirect("NoticeDetail.aspx?No=" + no);
            }
        }

        protected void btnWrite_Click(object sender, EventArgs e)
        {
            Response.Redirect("NoticeWrite.aspx");
        }
    }
}