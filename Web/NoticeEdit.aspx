<%@ Page Title="공지사항 수정" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="NoticeEdit.aspx.cs" Inherits="UniPlayWebSite.NoticeEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* 중앙 폭 제한 + 좌우 패딩 */
        .page-wrapper.narrow {
            max-width: 900px;
            margin: 0 auto;
            padding: 0 12px;
        }

        /* 폼 테이블 기본 */
        .form-table {
            width: 100%;
            color: white;
            table-layout: fixed;
            border-collapse: separate;
            border-spacing: 0;
        }

            .form-table td {
                vertical-align: top;
                padding: 6px 8px;
            }

        .form-label {
            white-space: nowrap;
            width: 120px;
            font-weight: 700;
        }

        /* 입력 공통: 넓게, 박스계산 */
        .form-input {
            width: 100% !important;
            max-width: 100% !important;
            box-sizing: border-box;
        }

        /* 버튼 영역 */
        .form-actions {
            margin-top: 20px;
            display: flex;
            gap: 8px;
            justify-content: flex-end;
        }

        /* 모바일: 라벨 위, 입력 아래로 스택 */
        @media (max-width: 640px) {
            .form-table, .form-table tr, .form-table td {
                display: block;
                width: 100%;
            }

            .form-label {
                width: auto;
                margin-bottom: 6px;
            }

            .form-actions {
                justify-content: flex-start;
                flex-wrap: wrap;
            }
        }
    </style>

    <h2 class="pixel-Head" style="padding-left: 40px;">공지사항 수정</h2>

    <div class="page-wrapper narrow">
        <table class="form-table">
            <tr>
                <td class="form-label">제목</td>
                <td>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="pixel-input form-input" />
                </td>
            </tr>
            <tr>
                <td class="form-label">내용</td>
                <td>
                    <asp:TextBox ID="txtContents" runat="server" TextMode="MultiLine" Rows="10"
                        CssClass="pixel-input form-input" />
                </td>
            </tr>
        </table>

        <div class="form-actions">
            <asp:Button ID="btnUpdate" runat="server" Text="수정 완료" CssClass="pixel-button" OnClick="btnUpdate_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="취소" CssClass="pixel-button" OnClick="btnCancel_Click" />
        </div>
    </div>

</asp:Content>
