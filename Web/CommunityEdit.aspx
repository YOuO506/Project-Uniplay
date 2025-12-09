<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="CommunityEdit.aspx.cs" Inherits="UniPlayWebSite.CommunityEdit" %>

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

        /* 미리보기 이미지 */
        .img-preview {
            max-width: 100%;
            height: auto;
            border-radius: 8px;
            display: block;
            margin: 6px 0;
        }

        /* 안내문 */
        .form-note {
            display: block;
            font-size: 12px;
            opacity: .85;
            margin-top: 6px;
        }

        /* 버튼 영역 */
        .form-actions {
            margin-top: 20px;
            display: flex;
            gap: 8px;
            justify-content: flex-end;
            flex-wrap: wrap;
        }

        .breakable {
            display: block;
            max-width: 100%;
            white-space: normal; /* pre/nowrap 방지 */
            overflow-wrap: anywhere; /* 표준: 어디서든 줄바꿈 */
            word-break: break-all; /* 레거시 브라우저 보강 */
            font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace, inherit !important;
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
            }
        }
    </style>


    <h2 class="pixel-Head" style="padding-left: 40px;">글 내용 수정</h2>

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

            <tr>
                <td class="form-label">현재 이미지</td>
                <td>
                    <asp:Image ID="imgCurrent" runat="server" Visible="false" CssClass="img-preview" />
                    <asp:Label ID="lblCurrentImage" runat="server" CssClass="pixel-label breakable" />
                </td>
            </tr>

            <tr>
                <td class="form-label">새 이미지</td>
                <td>
                    <asp:FileUpload ID="fuNewImage" runat="server" CssClass="form-input" />
                    <span class="form-note">※ 파일을 선택하면 저장 시 기존 이미지는 새 이미지로 교체됩니다.</span>
                </td>
            </tr>

            <tr>
                <td class="form-label">이미지 없이 저장</td>
                <td>
                    <asp:CheckBox ID="chkNoImage" runat="server"
                        Text="모든 이미지 제거하고 텍스트만 저장 (위 파일 선택은 무시됩니다)" />
                </td>
            </tr>
        </table>

        <div class="form-actions">
            <asp:Button ID="btnUpdate" runat="server" Text="수정 완료" CssClass="pixel-button"
                OnClick="btnUpdate_Click" CausesValidation="false" />
            <asp:Button ID="btnCancel" runat="server" Text="취소" CssClass="pixel-button"
                OnClick="btnCancel_Click" CausesValidation="false" />
        </div>

        <!-- 키/작성자/이미지 경로 보관 -->
        <asp:HiddenField ID="hdnPostID" runat="server" />
        <asp:HiddenField ID="hdnAuthorID" runat="server" />
        <asp:HiddenField ID="hdnImagePath" runat="server" />
    </div>


</asp:Content>
