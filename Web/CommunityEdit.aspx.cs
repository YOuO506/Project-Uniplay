using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace UniPlayWebSite
{
    public partial class CommunityEdit : System.Web.UI.Page
    {

        private string ConnStr => ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
        private string CurrentUserID => Session["UserID"] as string;
        private int CurrentUGrade => Session["UGrade"] == null ? 0 : Convert.ToInt32(Session["UGrade"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            // 필수: PostID 확인
            int postId;
            if (!int.TryParse(Request.QueryString["PostID"], out postId))
            {
                Response.Redirect("Community.aspx");
                return;
            }

            // 원문 로드 + 권한 체크 + 화면 바인딩
            hdnPostID.Value = postId.ToString();

            string sql = @"
        SELECT 
            C.Title,
            C.Contents,
            LTRIM(RTRIM(C.Author)) AS Author,
            ISNULL(C.ImagePath, '') AS ImagePath
        FROM Community C
        WHERE C.PostID = @PostID AND C.IsDeleted = 0";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PostID", postId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        Response.Redirect("Community.aspx");
                        return;
                    }

                    // 원래 제목/내용
                    txtTitle.Text = Convert.ToString(r["Title"]);
                    txtContents.Text = Convert.ToString(r["Contents"]);

                    // 작성자/이미지 경로 보관
                    string author = Convert.ToString(r["Author"]).Trim();
                    string img = Convert.ToString(r["ImagePath"]).Trim();

                    hdnAuthorID.Value = author;
                    hdnImagePath.Value = img;

                    // 권한: 관리자 or 작성자만
                    string me = (CurrentUserID ?? "").Trim();
                    bool isAdmin = (CurrentUGrade == 1);
                    bool isAuthor = (!string.IsNullOrEmpty(me) && me.Equals(author, StringComparison.OrdinalIgnoreCase));
                    if (!isAdmin && !isAuthor)
                    {
                        Response.Redirect("CommunityDetail.aspx?PostID=" + postId);
                        return;
                    }

                    // 현재 이미지 표시
                    if (!string.IsNullOrWhiteSpace(img))
                    {
                        imgCurrent.ImageUrl = img;
                        imgCurrent.Visible = true;
                        lblCurrentImage.Text = img;
                    }
                    else
                    {
                        imgCurrent.Visible = false;
                        lblCurrentImage.Text = "이미지 없음";
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            // PostID 확보
            int postId;
            if (!int.TryParse(hdnPostID.Value, out postId))
            {
                if (!int.TryParse(Request.QueryString["PostID"], out postId))
                {
                    Response.Redirect("Community.aspx");
                    return;
                }
            }

            // 권한 재확인
            string me = (CurrentUserID ?? "").Trim();
            string author = (hdnAuthorID.Value ?? "").Trim();
            bool isAdmin = (CurrentUGrade == 1);
            bool isAuthor = (!string.IsNullOrEmpty(me) && me.Equals(author, StringComparison.OrdinalIgnoreCase));
            if (!isAdmin && !isAuthor)
            {
                Response.Redirect("CommunityDetail.aspx?PostID=" + postId);
                return;
            }

            // 입력값
            string newTitle = (txtTitle.Text ?? "").Trim();
            string newContents = (txtContents.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newTitle) || string.IsNullOrWhiteSpace(newContents))
            {
                // 간단 경고 후 머무르기
                string script = "alert('제목과 내용을 입력하세요.');";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "needInput", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "needInput", script, true);
                return;
            }

            // 이미지 경로 결정
            string finalImagePath = hdnImagePath.Value?.Trim(); // 기본: 기존 유지

            // 1) 이미지 제거 체크 → 무조건 제거
            if (chkNoImage.Checked)
            {
                finalImagePath = ""; // DB에는 빈 문자열/NULL 중 하나로 저장할 것
            }
            // 2) 새 파일 업로드 → 교체
            else if (fuNewImage.HasFile)
            {
                // 확장자/용량 검증
                string ext = Path.GetExtension(fuNewImage.FileName).ToLowerInvariant();
                string[] allow = { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allow.Contains(ext))
                {
                    string script = "alert('이미지 파일은 jpg, jpeg, png, gif만 가능합니다.');";
                    if (ScriptManager.GetCurrent(this.Page) != null)
                        ScriptManager.RegisterStartupScript(this, GetType(), "badExt", script, true);
                    else
                        ClientScript.RegisterStartupScript(this.GetType(), "badExt", script, true);
                    return;
                }

                if (fuNewImage.PostedFile.ContentLength > 5 * 1024 * 1024)
                {
                    string script = "alert('이미지 파일 용량은 5MB 이하만 가능합니다.');";
                    if (ScriptManager.GetCurrent(this.Page) != null)
                        ScriptManager.RegisterStartupScript(this, GetType(), "bigImg", script, true);
                    else
                        ClientScript.RegisterStartupScript(this.GetType(), "bigImg", script, true);
                    return;
                }

                // 저장 폴더 : 기존 이미지 폴더 우선, 없으면 Write와 동일(/CommunityImages/yyyy/MM/)
                string folderVirtual = "";
                string old = (hdnImagePath.Value ?? "").Trim(); // 예: /CommunityImages/2025/10/abc.png
                if (!string.IsNullOrWhiteSpace(old))
                {
                    string normalized = old.StartsWith("~") ? ResolveUrl(old) : old; 
                    folderVirtual = VirtualPathUtility.AppendTrailingSlash(
                                        VirtualPathUtility.GetDirectory(normalized)); // -> /CommunityImages/2025/10/
                }
                if (string.IsNullOrWhiteSpace(folderVirtual))
                {
                    string y = DateTime.Now.ToString("yyyy");
                    string m = DateTime.Now.ToString("MM");
                    folderVirtual = $"/CommunityImages/{y}/{m}/";
                }

                // 물리 경로 준비
                string folderPhysical = Server.MapPath("~" + folderVirtual); 
                if (!Directory.Exists(folderPhysical))
                    Directory.CreateDirectory(folderPhysical);

                // 파일 저장 (GUID 파일명)
                string fileName = Guid.NewGuid().ToString("N") + ext;
                string savePath = Path.Combine(folderPhysical, fileName);
                fuNewImage.SaveAs(savePath);

                // DB에는 브라우저가 바로 쓰는 경로 저장
                finalImagePath = folderVirtual + fileName; // 예: /CommunityImages/2025/10/xxxxxxxxxxxx.png
            }
            // 3) 아무 것도 안 하면 → 기존 유지(finalImagePath 그대로)

            // DB 업데이트
            string sql = @"
        UPDATE Community
        SET Title = @Title,
            Contents = @Contents,
            ImagePath = NULLIF(@ImagePath, ''), -- '' 들어오면 NULL 저장
            UpdateTime = GETDATE()
        WHERE PostID = @PostID AND IsDeleted = 0";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Title", newTitle);
                cmd.Parameters.AddWithValue("@Contents", newContents);
                cmd.Parameters.AddWithValue("@ImagePath", (object)(finalImagePath ?? "") ?? "");
                cmd.Parameters.AddWithValue("@PostID", postId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // 수정 완료 → 상세로 복귀
            Response.Redirect("CommunityDetail.aspx?PostID=" + postId);
        }

        // 취소 버튼
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            int postId;
            if (int.TryParse(Request.QueryString["PostID"], out postId))
                Response.Redirect("CommunityDetail.aspx?PostID=" + postId);
            else
                Response.Redirect("Community.aspx");
        }
    }
}
