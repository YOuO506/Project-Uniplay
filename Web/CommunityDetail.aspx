<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="CommunityDetail.aspx.cs" Inherits="UniPlayWebSite.CommunityDetail" UnobtrusiveValidationMode="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        #header {
            position: relative;
            z-index: 9999;
        }
        /* 헤더를 항상 맨 위로 */
        .page-wrapper {
            position: relative;
            z-index: 1;
        }
        /* 본문은 아래로 */

        .pixel-detail {
            font-family: 'NeoDunggeunmoPro', sans-serif;
            font-size: 32px;
            text-align: right;
            white-space: nowrap;
            word-break: keep-all;
            vertical-align: top;
            padding-right: 10px;
        }

        .meta-line {
            display: flex;
            align-items: center;
            gap: 8px;
        }

            .meta-line .pixel-detail {
                text-align: left;
            }

        /* 상세 상단 */

        .post-header {
            font-size: 28px;
            font-weight: 800;
            margin: 8px 0 4px;
        }

        .post-meta {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            align-items: center;
            font-size: 13px;
            opacity: .9;
        }

            .post-meta .meta-item {
                display: flex;
                gap: 6px;
                align-items: center;
                white-space: nowrap;
            }

            .post-meta .sep {
                opacity: .5;
            }

        /* 본문 */
        .post-body {
            margin-top: 16px;
            font-size: 16px;
            line-height: 1.6;
            white-space: pre-line;
            word-break: break-word;
            text-indent: 0;
            margin-left: 0;
        }

        /* 이미지 */
        .post-image {
            margin-top: 12px;
        }

            .post-image img {
                max-width: 100%;
                height: auto;
                border-radius: 8px;
                box-shadow: 0 2px 8px #00000040;
            }

        /* 컨테이너 폭 제한(선택) */
        .page-wrapper.narrow {
            max-width: 900px;
            margin: 0 auto;
        }

        /* 댓글 3열 레이아웃 */

        .comment-row {
            display: grid;
            grid-template-columns: 160px 1fr auto; /* 작성자 | 내용(최대) | 작성일(좁게) */
            gap: 10px;
            padding: 12px;
            border: 1px solid #ffffff33;
            border-radius: 8px;
            margin-bottom: 12px;
            background: #00000022;
        }

        .comment-author {
            font-weight: 700;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .comment-body {
            white-space: pre-wrap; /* 줄바꿈/개행 유지 */
            word-break: break-word; /* 긴 단어도 줄바꿈 */
        }

        .comment-time {
            font-size: 12px;
            opacity: .8;
            white-space: nowrap; /* 줄바꿈 금지 */
            text-align: right;
        }

        /* 댓글 작성 블록 */

        .comment-write {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }

        .comment-write-title {
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .comment-write-box {
            width: 100%;
            min-height: 96px;
        }
        /* 넓고 크게 */
        .comment-write-actions {
            display: flex;
            gap: 8px;
        }

        /* 모바일(좁은 화면)에서는 2행: [작성자 | 작성일] / [내용] */
        @media (max-width: 640px) {
            .comment-row {
                grid-template-columns: 1fr auto;
                grid-template-areas:
                    "author time"
                    "body   body";
            }

            .comment-author {
                grid-area: author;
            }

            .comment-body {
                grid-area: body;
            }

            .comment-time {
                grid-area: time;
            }
        }
    </style>

    <h2 class="pixel-Head" style="padding-left: 40px;">게시판 상세보기</h2>

    <div class="page-wrapper narrow">
        <!-- 제목 -->
        <div class="post-header">
            <asp:Label ID="lblTitle" runat="server"></asp:Label>
        </div>

        <!-- 메타: 작성자 · 작성일 · 조회수 (작게, 한 줄) -->
        <div class="post-meta">
            <span class="meta-item">작성자 <strong>
                <asp:Label ID="lblAuthor" runat="server"></asp:Label></strong></span>
            <span class="sep">•</span>
            <span class="meta-item">
                <asp:Label ID="lblDate" runat="server"></asp:Label></span>
            <span class="sep">•</span>
            <span class="meta-item">조회수
                <asp:Label ID="lblHits" runat="server"></asp:Label></span>
        </div>

        <!-- 본문(라벨 없음) -->
        <div class="post-body">
            <asp:Label ID="lblContents" runat="server" Style="white-space: pre-wrap;"></asp:Label>
        </div>

        <!-- 이미지(있을 때만 Visible) -->
        <div class="post-image">
            <asp:Image ID="imgPost" runat="server" Visible="false" />
        </div>
    </div>


    <asp:Panel ID="pnlEditButtons" runat="server" Visible="true" Style="margin-top: 20px; text-align: right;">
        <asp:Button ID="btnEdit" runat="server" Text="수정" CssClass="pixel-button" OnClick="btnEdit_Click" CausesValidation="false" />
        <asp:Button ID="btnDelete" runat="server" Text="삭제" CssClass="pixel-button"
            OnClientClick="return confirm('정말 삭제하시겠습니까?');"
            OnClick="btnDelete_Click" />
    </asp:Panel>

    <!-- 키 보관 -->
    <asp:HiddenField ID="hdnPostID" runat="server" />
    <asp:HiddenField ID="hdnUserID" runat="server" />
    <asp:HiddenField ID="hdnAuthorID" runat="server" />

    <!-- 좋아요 -->
    <div class="page-wrapper">
        <div class="meta-line">
            <span class="pixel-detail">좋아요 :</span>
            <asp:Label ID="lblLikeCount" runat="server" Text="0" CssClass="pixel-input" />
            <asp:Button ID="btnLike" runat="server" Text="좋아요" CssClass="pixel-button" OnClick="btnLike_Click" />
        </div>
    </div>

    <!-- 댓글 헤더 -->
    <div class="page-wrapper">
        <div class="meta-line">
            <span class="pixel-detail">댓글 :</span>
            <asp:Label ID="lblCommentCount" runat="server" Text="(0)" CssClass="pixel-label" />
        </div>
    </div>

    <!-- 댓글 목록 -->
    <div class="page-wrapper">
        <asp:Repeater ID="rptComments" runat="server"
            OnItemDataBound="rptComments_ItemDataBound"
            OnItemCommand="rptComments_ItemCommand">
            <ItemTemplate>
                <!-- 3열: 작성자 | 내용 | 작성일 -->
                <div class="comment-row">
                    <div class="comment-author"><%# Eval("NickName") %></div>
                    <div class="comment-body"><%# Eval("Contents") %></div>
                    <div class="comment-time"><%# Eval("UploadTime", "{0:yyyy-MM-dd HH:mm}") %></div>
                </div>

                <!-- 관리자만 노출(기존 로직 유지). 위치는 댓글 박스 아래 여백에 -->
                <asp:Panel ID="pnlCommentAdmin" runat="server" Visible="false" Style="text-align: right; margin: -6px 0 12px;">
                    <asp:LinkButton ID="btnCommentDelete" runat="server"
                        Text="댓글 삭제"
                        CssClass="pixel-button"
                        CommandName="DeleteComment"
                        CommandArgument='<%# Eval("CommentID") %>'
                        CausesValidation="false"
                        UseSubmitBehavior="true" />
                </asp:Panel>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- 댓글 작성 -->
    <div class="page-wrapper">
        <div class="comment-write">
            <div class="comment-write-title">
                <span class="pixel-detail">댓글 작성</span>
            </div>
            <asp:TextBox ID="txtNewComment" runat="server"
                TextMode="MultiLine" Rows="5"
                CssClass="pixel-input comment-write-box" />
            <div class="comment-write-actions">
                <asp:Button ID="btnAddComment" runat="server" Text="등록"
                    CssClass="pixel-button"
                    OnClick="btnAddComment_Click" CausesValidation="false" />
            </div>
        </div>
    </div>



</asp:Content>
