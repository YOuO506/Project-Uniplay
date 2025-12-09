<%@ Page Title="공지사항 상세보기" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="NoticeDetail.aspx.cs" Inherits="UniPlayWebSite.NoticeDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* 컨테이너 폭 제한 + 가운데 배치 */
        .page-wrapper.narrow {
            max-width: 900px;
            margin: 0 auto;
        }

        /* 상단 제목 */
        .post-header {
            font-size: 28px;
            font-weight: 800;
            margin: 8px 0 4px;
        }

        /* 작성자·작성일·조회수 */
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
            white-space: pre-line; /* 개행 유지, 앞/연속 공백은 접기 */
            word-break: break-word;
        }

        #txtContents {
            pointer-events: none; /* 클릭해도 반응 없음 */
            caret-color: transparent; /* 커서 안보이게 */
        }

        /* 모바일 보정 */
        @media (max-width:640px) {
            .post-header {
                font-size: 22px;
            }

            .post-body {
                font-size: 15px;
            }
        }
    </style>

    <h2 class="pixel-Head" style="padding-left: 40px;">공지사항 상세보기</h2>

    <div class="page-wrapper narrow">
        <!-- 제목 -->
        <div class="post-header">
            <asp:Label ID="lblTitle" runat="server"></asp:Label>
        </div>

        <!-- 메타 -->
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

        <!-- 본문 -->
        <asp:TextBox ID="txtContents" runat="server" TextMode="MultiLine"
            Rows="10" CssClass="pixel-input" ReadOnly="true"
            Style="width: 100%; max-width: 100%; box-sizing: border-box; pointer-events: none; caret-color: transparent;" />

    </div>

    <asp:Panel ID="pnlEditButtons" runat="server" Visible="false"
        Style="margin: 16px auto 0; max-width: 900px; text-align: right;">
        <asp:Button ID="btnEdit" runat="server" Text="수정" CssClass="pixel-button" OnClick="btnEdit_Click" />
        <asp:Button ID="btnDelete" runat="server" Text="삭제" CssClass="pixel-button"
            OnClientClick="return confirm('정말 삭제하시겠습니까?');"
            OnClick="btnDelete_Click" />
    </asp:Panel>

</asp:Content>
