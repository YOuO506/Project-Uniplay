<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="Community.aspx.cs" Inherits="UniPlayWebSite.Community" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* 기본 레이아웃 통일 */
        .gv {
            table-layout: fixed;
            width: 100%;
        }

            .gv th, .gv td {
                overflow: hidden;
                white-space: nowrap;
            }

            .gv th {
                white-space: nowrap;
            }

            .gv .col-no {
                width: 8% !important;
                text-align: center;
            }

            .gv .col-title {
                width: 44% !important;
            }

            .gv .col-author {
                width: 18% !important;
                white-space: nowrap;
                text-overflow: ellipsis;
            }

            .gv .col-date {
                width: 20% !important;
                white-space: nowrap;
            }

            .gv .col-hits {
                width: 10% !important;
                text-align: center;
                white-space: nowrap;
            }

            /* 제목 말줄임 */
            .gv .gv-title-link {
                display: -webkit-box;
                -webkit-box-orient: vertical;
                -webkit-line-clamp: 1;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: normal;
                line-height: 1.25;
            }

        /* 모바일: 조회수 숨김, 동일 비율 유지 */
        @media (max-width:640px) {
            .gv {
                font-size: 13px;
            }

                .gv th, .gv td {
                    padding: 6px 4px;
                }

                    .gv .col-hits, .gv th.col-hits {
                        display: none;
                    }

                .gv .col-no {
                    width: 10% !important;
                }

                .gv .col-title {
                    width: auto !important;
                }

                .gv .col-author {
                    width: 20% !important;
                }

                .gv .col-date {
                    width: 25% !important;
                }

                .gv .gv-title-link {
                    -webkit-line-clamp: 2;
                }
        }
    </style>

    <h2 class="pixel-Head" style="padding-left: 40px;">커뮤니티 게시판</h2>

    <div class="page-wrapper">

        <!-- GridView 내용 -->

        <asp:GridView ID="gvCommunity" runat="server"
            Width="100%"
            CssClass="pixel-text gv"
            AutoGenerateColumns="False"
            OnRowCommand="gvCommunity_RowCommand"
            Style="table-layout: fixed;">
            <Columns>
                <asp:BoundField DataField="No" HeaderText="번호">
                    <HeaderStyle Width="8%" CssClass="col-center col-no" />
                    <ItemStyle Width="8%" HorizontalAlign="Center" CssClass="col-no" />
                </asp:BoundField>

                <asp:TemplateField HeaderText="제목">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkTitle" runat="server"
                            Text='<%# Eval("Title") %>'
                            CommandName="ViewDetail"
                            CommandArgument='<%# Eval("No") %>'
                            CssClass="gv-title-link"
                            Style="color: #ffcc00; font-weight: bold;" />
                    </ItemTemplate>
                    <HeaderStyle Width="40%" CssClass="col-center col-title" />
                    <ItemStyle Width="40%" HorizontalAlign="Center" CssClass="col-title" />
                </asp:TemplateField>

                <asp:BoundField DataField="Author" HeaderText="작성자">
                    <HeaderStyle Width="12%" CssClass="col-center col-author" />
                    <ItemStyle Width="12%" HorizontalAlign="Center" CssClass="col-author" />
                </asp:BoundField>

                <asp:BoundField DataField="UploadTime" HeaderText="작성일"
                    DataFormatString="{0:yyyy-MM-dd}" HtmlEncode="false">
                    <HeaderStyle Width="12%" CssClass="col-center col-date" />
                    <ItemStyle Width="12%" HorizontalAlign="Center" CssClass="col-date" />
                </asp:BoundField>

                <asp:BoundField DataField="Hits" HeaderText="조회수">
                    <HeaderStyle Width="8%" CssClass="col-center col-hits" />
                    <ItemStyle Width="8%" HorizontalAlign="Center" CssClass="col-hits" />
                </asp:BoundField>

                <asp:BoundField DataField="CommentCount" HeaderText="댓글">
                    <HeaderStyle Width="8%" CssClass="col-center col-comments" />
                    <ItemStyle Width="8%" HorizontalAlign="Center" CssClass="col-comments" />
                </asp:BoundField>

                <asp:BoundField DataField="LikeCount" HeaderText="좋아요">
                    <HeaderStyle Width="8%" CssClass="col-center col-likes" />
                    <ItemStyle Width="8%" HorizontalAlign="Center" CssClass="col-likes" />
                </asp:BoundField>
            </Columns>
        </asp:GridView>


    </div>

    <asp:Button ID="btnWrite" runat="server" Text="글쓰기" CssClass="pixel-button" OnClick="btnWrite_Click" />
</asp:Content>
