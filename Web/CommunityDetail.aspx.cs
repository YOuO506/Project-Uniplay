using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UniPlayWebSite
{
    public partial class CommunityDetail : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;
        private string CurrentUserID => Session["UserID"] as string;
        private int CurrentUGrade => Session["UGrade"] == null ? 0 : Convert.ToInt32(Session["UGrade"]);


        protected void Page_Load(object sender, EventArgs e)
        {
            // PostID 유효성
            int postId;
            if (!int.TryParse(Request.QueryString["PostID"], out postId))
            {
                Response.Redirect("Community.aspx");
                return;
            }

            if (!IsPostBack)
            {
                hdnPostID.Value = postId.ToString();
                hdnUserID.Value = CurrentUserID ?? string.Empty;

                // 조회수 +1 (세션 키로 중복 방지)
                string viewKey = "ViewedPost_" + postId;
                if (Session[viewKey] == null)
                {
                    using (var conn = new SqlConnection(ConnStr))
                    using (var cmd = new SqlCommand("UPDATE Community SET Hits = Hits + 1 WHERE PostID=@PostID AND IsDeleted=0", conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    Session[viewKey] = true;
                }

                // 본문/메타/댓글 최초 로드
                LoadDetail(postId);
                LoadComments(postId);
            }

            SetupEditButtons();
        }

        private void LoadDetail(int postId)
        {
            string sql = @"
        SELECT C.PostID, C.Title, C.Contents, C.Author,
               LTRIM(RTRIM(M.NickName)) AS NickName,
               C.UploadTime, C.UpdateTime, C.Hits, C.LikeCount, C.CommentCount, C.ImagePath,
               CASE WHEN @UID IS NOT NULL AND EXISTS(SELECT 1 FROM CommunityLikes L WHERE L.PostID=C.PostID AND L.UserID=@UID)
                    THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS LikedByMe
        FROM Community C
        JOIN Members M ON LTRIM(RTRIM(C.Author)) = LTRIM(RTRIM(M.UserID))
        WHERE C.PostID=@PostID AND C.IsDeleted=0";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PostID", postId);
                cmd.Parameters.AddWithValue("@UID", (object)CurrentUserID ?? DBNull.Value);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        // 삭제되었거나 없음
                        Response.Redirect("Community.aspx");
                        return;
                    }

                    lblTitle.Text = r["Title"].ToString();
                    lblAuthor.Text = r["NickName"].ToString();
                    lblDate.Text = Convert.ToDateTime(r["UploadTime"]).ToString("yyyy-MM-dd HH:mm");
                    lblHits.Text = r["Hits"].ToString();
                    lblContents.Text = r["Contents"].ToString();

                    lblLikeCount.Text = r["LikeCount"].ToString();
                    lblCommentCount.Text = $"({r["CommentCount"]})";

                    // 좋아요 버튼 상태
                    bool liked = Convert.ToBoolean(r["LikedByMe"]);
                    btnLike.Text = liked ? "좋아요 취소" : "좋아요";

                    // 이미지
                    string path = r["ImagePath"]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        imgPost.ImageUrl = path;
                        imgPost.Visible = true;
                    }
                    else
                    {
                        imgPost.Visible = false;
                    }

                    // 작성자 보관(수정/삭제 권한용)
                    string authorUserId = Convert.ToString(r["Author"]).Trim();
                    ViewState["AuthorUserID"] = authorUserId;
                    hdnAuthorID.Value = authorUserId;

                    Response.Write("<script>console.log('AuthorUserID: " + (authorUserId ?? "NULL") + " | CurrentUserID: " + (CurrentUserID ?? "NULL") + " | Grade: " + CurrentUGrade + "');</script>");

                }
            }
        }

        private void LoadComments(int postId)
        {
            string sql = @"
        SELECT CC.CommentID, CC.PostID, 
               LTRIM(RTRIM(CC.Author)) AS Author,
               LTRIM(RTRIM(M.NickName)) AS NickName,
               CC.Contents, CC.UploadTime
        FROM CommunityComments CC
        JOIN Members M ON CC.Author = M.UserID
        WHERE CC.PostID=@PostID AND CC.IsDeleted=0
        ORDER BY CASE WHEN CC.ParentCommentID IS NULL THEN CC.CommentID ELSE CC.ParentCommentID END,
                 CC.ParentCommentID, CC.CommentID";

            using (var conn = new SqlConnection(ConnStr))
            using (var da = new SqlDataAdapter(sql, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@PostID", postId);
                var dt = new DataTable();
                da.Fill(dt);
                rptComments.DataSource = dt;
                rptComments.DataBind();
            }
        }

        private void SetupEditButtons()
        {

            if (btnEdit == null || btnDelete == null || pnlEditButtons == null) return;

            // 작성자/관리자 판정
            string author = !string.IsNullOrWhiteSpace(hdnAuthorID.Value)
                ? hdnAuthorID.Value.Trim()
                : Convert.ToString(ViewState["AuthorUserID"] ?? "").Trim();

            string me = (CurrentUserID ?? "").Trim();
            bool isAuthor = (!string.IsNullOrEmpty(me) && me.Equals(author, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = (CurrentUGrade == 1);

            // - 관리자이면서 작성자 : 수정+삭제 모두 보임
            // - 관리자(작성자 아님) : 삭제만 보임
            // - 일반 사용자 : 작성자만 수정/삭제 보임
            if (isAdmin && isAuthor)
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
            }
            else if (isAdmin)
            {
                btnEdit.Visible = false;
                btnDelete.Visible = true;
            }
            else
            {
                btnEdit.Visible = isAuthor;
                btnDelete.Visible = isAuthor;
            }

            pnlEditButtons.Visible = btnEdit.Visible || btnDelete.Visible;
        }

        protected void btnLike_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUserID))
            {
                string ret = "Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl);
                string script = $"alert('로그인 후 이용해 주세요.'); window.location.href='{ret}';";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "loginRedirect", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "loginRedirect", script, true);
                return;
            }

            int postId = int.Parse(hdnPostID.Value);

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
        BEGIN TRAN;

        IF EXISTS (SELECT 1 FROM CommunityLikes WHERE PostID=@PostID AND UserID=@UID)
        BEGIN
            DELETE FROM CommunityLikes WHERE PostID=@PostID AND UserID=@UID;
            UPDATE Community SET LikeCount = CASE WHEN LikeCount>0 THEN LikeCount-1 ELSE 0 END WHERE PostID=@PostID;
        END
        ELSE
        BEGIN
            INSERT INTO CommunityLikes(PostID, UserID, LikedTime) VALUES(@PostID, @UID, GETDATE());
            UPDATE Community SET LikeCount = LikeCount+1 WHERE PostID=@PostID;
        END

        COMMIT;", conn))
            {
                cmd.Parameters.AddWithValue("@PostID", postId);
                cmd.Parameters.AddWithValue("@UID", CurrentUserID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // 갱신
            LoadDetail(postId);
            SetupEditButtons();
        }


        protected void btnAddComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUserID))
            {
                string ret = "Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl);
                string script = $"alert('로그인 후 이용해 주세요.'); window.location.href='{ret}';";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "loginRedirectCmt", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "loginRedirectCmt", script, true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewComment.Text))
            {
                string script = "alert('댓글 내용을 입력하세요.');";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "emptyCmt", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "emptyCmt", script, true);
                return;
            }

            int postId = int.Parse(hdnPostID.Value);

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
        BEGIN TRAN;

        INSERT INTO CommunityComments(PostID, Author, Contents, UploadTime, UpdateTime, ParentCommentID, IsDeleted)
        VALUES(@PostID, @Author, @Contents, GETDATE(), NULL, NULL, 0);

        UPDATE Community SET CommentCount = CommentCount + 1 WHERE PostID=@PostID;

        COMMIT;", conn))
            {
                cmd.Parameters.AddWithValue("@PostID", postId);
                cmd.Parameters.AddWithValue("@Author", CurrentUserID);
                cmd.Parameters.AddWithValue("@Contents", txtNewComment.Text.Trim());
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtNewComment.Text = string.Empty;
            LoadDetail(postId);
            LoadComments(postId);
            SetupEditButtons();
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            int postId;
            if (!int.TryParse(hdnPostID.Value, out postId))
            {
                if (!int.TryParse(Request.QueryString["PostID"], out postId))
                {
                    Response.Redirect("Community.aspx");
                    return;
                }
            }

            string url = ResolveUrl("~/CommunityEdit.aspx") + "?PostID=" + postId;

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int postId = int.Parse(hdnPostID.Value);

            string author = (hdnAuthorID.Value ?? "").Trim();
            string me = (CurrentUserID ?? "").Trim();
            bool canDelete = (CurrentUGrade == 1) || (!string.IsNullOrEmpty(me) && me.Equals(author, StringComparison.OrdinalIgnoreCase));

            if (!canDelete)
            {
                string script = "alert('삭제 권한이 없습니다.');";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "noDel", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "noDel", script, true);
                return;
            }

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("UPDATE Community SET IsDeleted=1, UpdateTime=GETDATE() WHERE PostID=@PostID", conn))
            {
                cmd.Parameters.AddWithValue("@PostID", postId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            Response.Redirect("Community.aspx");
        }

        protected void rptComments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "DeleteComment") return;

            int postId = int.Parse(hdnPostID.Value);
            int commentId = Convert.ToInt32(e.CommandArgument);

            // DB에서 댓글 작성자 조회
            string commentAuthor = null;
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT Author FROM CommunityComments WHERE CommentID=@cid AND PostID=@pid", conn))
            {
                cmd.Parameters.AddWithValue("@cid", commentId);
                cmd.Parameters.AddWithValue("@pid", postId);
                conn.Open();
                var o = cmd.ExecuteScalar();
                commentAuthor = (o == null ? "" : o.ToString()).Trim();
            }

            string me = (CurrentUserID ?? "").Trim();

            // 권한: 관리자이거나 댓글 작성자 본인
            bool canDeleteComment = (CurrentUGrade == 1) || (!string.IsNullOrEmpty(me) && me.Equals(commentAuthor, StringComparison.OrdinalIgnoreCase));
            if (!canDeleteComment)
            {
                string script = "alert('댓글 삭제 권한이 없습니다.');";
                if (ScriptManager.GetCurrent(this.Page) != null)
                    ScriptManager.RegisterStartupScript(this, GetType(), "noCmtDel", script, true);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "noCmtDel", script, true);
                return;
            }

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
        BEGIN TRAN;

        UPDATE CommunityComments SET IsDeleted=1, UpdateTime=GETDATE()
        WHERE CommentID=@CommentID AND PostID=@PostID;

        UPDATE Community SET CommentCount = CASE WHEN CommentCount>0 THEN CommentCount-1 ELSE 0 END
        WHERE PostID=@PostID;

        COMMIT;", conn))
            {
                cmd.Parameters.AddWithValue("@CommentID", commentId);
                cmd.Parameters.AddWithValue("@PostID", postId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadDetail(postId);
            LoadComments(postId);
            SetupEditButtons();
        }


        protected void rptComments_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var adminPanel = (Panel)e.Item.FindControl("pnlCommentAdmin");
            if (adminPanel == null) return;

            // 바인딩된 데이터에서 댓글 작성자 ID 읽기
            string commentAuthor = (Convert.ToString(DataBinder.Eval(e.Item.DataItem, "Author")) ?? "").Trim();
            string me = (CurrentUserID ?? "").Trim();

            // 관리자이거나, 댓글 작성자 본인이면 버튼 보이기
            ((Panel)e.Item.FindControl("pnlCommentAdmin")).Visible = (CurrentUGrade == 1) || (!string.IsNullOrEmpty(me) && me.Equals(commentAuthor, StringComparison.OrdinalIgnoreCase));
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            SetupEditButtons();
        }

    }
}