using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace UniPlayWebSite
{
    public partial class AdminItems : System.Web.UI.Page
    {

        string cs = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 관리자 체크
            if (Session["UGrade"]?.ToString() != "1") { Response.Redirect("Default.aspx"); return; }

            if (!IsPostBack)
            {
                BindGameTypeLists();
                BindGrid();
            }
        }

        void BindGameTypeLists()
        {
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // Games 드롭다운
                var dtG = new DataTable();
                new SqlDataAdapter("SELECT GameID, GameName FROM Games ORDER BY GameID", con)
                    .Fill(dtG);
                ddlGame.DataSource = dtG;
                ddlGame.DataValueField = "GameID";
                ddlGame.DataTextField = "GameName";
                ddlGame.DataBind();

                // ItemTypes 드롭다운
                var dtT = new DataTable();
                new SqlDataAdapter("SELECT ItemTypeID, ItemTypeName FROM ItemTypes ORDER BY ItemTypeID", con)
                    .Fill(dtT);
                ddlType.DataSource = dtT;
                ddlType.DataValueField = "ItemTypeID";
                ddlType.DataTextField = "ItemTypeName";
                ddlType.DataBind();
            }
        }

        void BindGrid()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                SELECT I.GameID, G.GameName, I.ItemID, I.ItemName, I.Price, I.ImagePath, T.ItemTypeName
                FROM Items I
                JOIN Games G ON G.GameID = I.GameID
                JOIN ItemTypes T ON T.ItemTypeID = I.ItemTypeID
                ORDER BY I.GameID, I.ItemID;", con))
            {
                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvItems.DataSource = dt;
                gvItems.DataBind();
            }
        }

        // ItemID 자동 할당(해당 GameID에서 MAX+1)
        int NextItemId(int gameId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT ISNULL(MAX(ItemID),0)+1 FROM Items WHERE GameID=@G", con))
            {
                cmd.Parameters.AddWithValue("@G", gameId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // 파일명만 입력해도 /Images/Items/ 붙여주는 정규화
        private string NormalizeImagePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var s = input.Trim().Replace("\\", "/");

            // ~/, / 제거
            if (s.StartsWith("~/")) s = s.Substring(2);
            if (s.StartsWith("/")) s = s.Substring(1);

            // 이미 폴더가 붙어있지 않다면 붙임
            if (!s.StartsWith("Images/Items/", StringComparison.OrdinalIgnoreCase))
                s = "Images/Items/" + s;

            return s;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            int gameId = int.Parse(ddlGame.SelectedValue);
            int typeId = int.Parse(ddlType.SelectedValue);
            string name = txtName.Text.Trim();
            int price = int.Parse(txtPrice.Text.Trim());

            string img = NormalizeImagePath(txtImg.Text);

            int next = NextItemId(gameId);

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                INSERT INTO Items(GameID, ItemID, ItemName, ItemDescription, ItemTypeID, Price, UploadTime, ImagePath)
                VALUES(@G, @I, @N, N'', @T, @P, GETDATE(), @Img);", con))
            {
                cmd.Parameters.AddWithValue("@G", gameId);
                cmd.Parameters.AddWithValue("@I", next);
                cmd.Parameters.AddWithValue("@N", name);
                cmd.Parameters.AddWithValue("@T", typeId);
                cmd.Parameters.AddWithValue("@P", price);

                cmd.Parameters.Add("@Img", SqlDbType.NVarChar, 255).Value =
                    (object)img ?? DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            txtName.Text = ""; txtPrice.Text = ""; txtImg.Text = "";
            BindGrid();
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (fuImage.HasFile)
            {
                // 원래 파일명
                string fileName = Path.GetFileName(fuImage.FileName);

                // 서버 저장 경로 (/Images/Items/)
                string saveDir = Server.MapPath("~/Images/Items/");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                string savePath = Path.Combine(saveDir, fileName);

                // 파일명 충돌 처리 (_1, _2 붙이기)
                int count = 1;
                while (File.Exists(savePath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    string newFileName = $"{nameWithoutExt}_{count}{ext}";
                    savePath = Path.Combine(saveDir, newFileName);
                    fileName = newFileName;
                    count++;
                }

                // 실제 저장
                fuImage.SaveAs(savePath);

                // DB에 저장될 경로를 TextBox에 세팅
                txtImg.Text = "Images/Items/" + fileName;
            }
        }

        protected void gvItems_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int gameId = Convert.ToInt32(gvItems.DataKeys[e.RowIndex].Values["GameID"]);
            int itemId = Convert.ToInt32(gvItems.DataKeys[e.RowIndex].Values["ItemID"]);

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("DELETE FROM Items WHERE GameID=@G AND ItemID=@I", con))
            {
                cmd.Parameters.AddWithValue("@G", gameId);
                cmd.Parameters.AddWithValue("@I", itemId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            BindGrid();
        }
    }
}