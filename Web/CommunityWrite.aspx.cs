using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace UniPlayWebSite
{
    public partial class CommunityWrite : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                {
                    string returnUrl = Server.UrlEncode("CommunityWrite.aspx");
                    Response.Redirect("Login.aspx?returnUrl=" + returnUrl);
                    return;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // 서버단 입력 검증 (제목/내용 필수)
            string title = txtTitle.Text?.Trim();
            string contents = txtContents.Text?.Trim();
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(contents))
            {
                lblMessage.Text = "제목과 내용을 모두 입력해 주세요.";
                return;
            }

            // 로그인 사용자 ID 확인 (작성자)
            if (Session["UserID"] == null)
            {
                lblMessage.Text = "로그인 세션이 만료되었습니다. 다시 로그인해 주세요.";
                return;
            }
            string author = Session["UserID"].ToString();

            // 이미지 경로 변수 (기본 빈 문자열)
            string imagePath = string.Empty;

            // '이미지 없이 등록' 체크가 안 되어 있고, 실제 파일이 올라왔다면 처리
            if (!chkNoImage.Checked && fuImage.HasFile)
            {
                // 확장자/용량 검증
                string ext = Path.GetExtension(fuImage.FileName).ToLower();
                string[] allow = { ".jpg", ".jpeg", ".png", ".gif" };
                if (Array.IndexOf(allow, ext) < 0)
                {
                    lblMessage.Text = "이미지 파일은 jpg, jpeg, png, gif만 가능합니다.";
                    return;
                }
                // 5MB 용량 제한 (필요 시 조정)
                if (fuImage.PostedFile.ContentLength > 5 * 1024 * 1024)
                {
                    lblMessage.Text = "이미지 파일 용량은 5MB 이하만 가능합니다.";
                    return;
                }

                // 저장 폴더(년/월) 구성 및 생성
                string y = DateTime.Now.ToString("yyyy");
                string m = DateTime.Now.ToString("MM");
                string relativeFolder = $"/CommunityImages/{y}/{m}/";
                string physicalFolder = Server.MapPath("~" + relativeFolder);
                if (!Directory.Exists(physicalFolder))
                {
                    Directory.CreateDirectory(physicalFolder);
                }

                // 파일명은 GUID로 유니크 생성
                string fileName = Guid.NewGuid().ToString("N") + ext;
                string physicalPath = Path.Combine(physicalFolder, fileName);

                // 실제 저장
                fuImage.SaveAs(physicalPath);

                // DB에는 웹에서 접근 가능한 상대 경로 저장
                imagePath = relativeFolder + fileName;
            }

            // DB INSERT (Hits/LikeCount/CommentCount는 0으로 시작, IsDeleted=0)
            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
            string sql = @"
                INSERT INTO Community
                (Title, Contents, Author, UploadTime, UpdateTime, Hits, LikeCount, CommentCount, Category, IsDeleted, ImagePath)
                VALUES
                (@Title, @Contents, @Author, GETDATE(), NULL, 0, 0, 0, NULL, 0, @ImagePath);";

            try
            {
                // 연결/명령/파라미터 설정 후 실행
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Contents", contents);
                    cmd.Parameters.AddWithValue("@Author", author);
                    cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? string.Empty);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // 저장 성공 → 목록으로 이동
                Response.Redirect("Community.aspx");
            }
            catch (Exception ex)
            {
                // 오류 메시지 노출 (개발 중에는 상세, 운영 시 로그만 남기고 사용자 메시지는 일반화 권장)
                lblMessage.Text = "저장 중 오류가 발생했습니다: " + ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // 목록으로 이동
            Response.Redirect("Community.aspx");
        }
    }
}