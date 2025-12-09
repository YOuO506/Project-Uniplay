<%@ Page Title="관리자 상품 등록/삭제" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminItems.aspx.cs" Inherits="UniPlayWebSite.AdminItems" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .grid {
            width: 100%;
            border-collapse: collapse;
        }

            .grid th, .grid td {
                border: 1px solid rgba(255,255,255,.2);
                padding: 6px 8px;
            }

            .grid th {
                background: rgba(255,255,255,.08);
            }

        .admin-toolbar {
            display: flex;
            align-items: center;
            flex-wrap: wrap;
            gap: 10px 14px;
            margin: 8px 0 12px;
        }

        .ctrl {
            height: 34px;
            padding: 0 10px;
            border-radius: 6px;
            border: 1px solid rgba(255,255,255,.35);
            background: rgba(255,255,255,.06);
            color: inherit;
        }

        .admin-toolbar select.ctrl {
            color: #fff; /* 닫힌 상태 글자색 */
        }

            .admin-toolbar select.ctrl option {
                color: #111; /* 펼친 목록 글자색 */
                background: #fff; /* 펼친 목록 배경 */
            }

        .lab {
            opacity: .9;
        }

        .w-100 {
            width: 100px
        }

        .w-160 {
            width: 160px
        }

        .w-200 {
            width: 200px
        }

        .w-220 {
            width: 220px
        }

        .prefix {
            position: relative;
        }

            .prefix .pre {
                position: absolute;
                left: 10px;
                top: 50%;
                transform: translateY(-50%);
                opacity: .8;
                pointer-events: none;
            }

        .with-prefix {
            padding-left: 130px;
        }

        .btn.primary {
            height: 34px;
            padding: 0 14px;
            border-radius: 6px;
            border: 1px solid rgba(255,255,255,.45);
            background: rgba(255,255,255,.08);
            cursor: pointer;
        }

            .btn.primary:hover {
                filter: brightness(1.1);
            }
    </style>

    <h2>아이템 관리</h2>

    <!-- 등록 -->
    <div class="admin-toolbar">
        <span class="lab">Game : </span>
        <asp:DropDownList ID="ddlGame" runat="server" CssClass="ctrl w-200" />

        <span class="lab">Type : </span>
        <asp:DropDownList ID="ddlType" runat="server" CssClass="ctrl w-160" />

        <span class="lab">Name : </span>
        <asp:TextBox ID="txtName" runat="server" CssClass="ctrl w-220" Placeholder="Item name" />

        <span class="lab">Price : </span>
        <asp:TextBox ID="txtPrice" runat="server" CssClass="ctrl w-100" Placeholder="100" />

        <span class="lab">Image : </span>
        <div class="prefix">
            <asp:TextBox ID="txtImg" runat="server" CssClass="ctrl w-220 with-prefix" Placeholder="laser.png" />
        </div>

        <asp:FileUpload ID="fuImage" runat="server" />
        <asp:Button ID="btnUpload" runat="server" CssClass="btn primary" Text="업로드" OnClick="btnUpload_Click" />

        <asp:Button ID="btnAdd" runat="server" Text="추가" CssClass="btn primary" OnClick="btnAdd_Click" />
    </div>

    <hr />

    <!-- 목록/삭제 -->
    <asp:GridView ID="gvItems" runat="server"
        AutoGenerateColumns="False"
        DataKeyNames="GameID,ItemID"
        CssClass="grid"
        OnRowDeleting="gvItems_RowDeleting">

        <Columns>
            <asp:BoundField DataField="GameName" ItemStyle-CssClass="font-normal" HeaderText="Game" />
            <asp:BoundField DataField="ItemTypeName" ItemStyle-CssClass="font-normal" HeaderText="Type" />
            <asp:BoundField DataField="ItemID" ItemStyle-CssClass="font-normal" HeaderText="ItemID" />
            <asp:BoundField DataField="ItemName" ItemStyle-CssClass="font-normal" HeaderText="Name" />
            <asp:BoundField DataField="Price" ItemStyle-CssClass="font-normal" HeaderText="Price" />

            <%-- 이미지 미리보기 (없으면 placeholder) --%>
            <asp:TemplateField HeaderText="Image">
                <ItemTemplate>
                    <asp:Image ID="imgItem" runat="server"
                        Width="32" Height="32"
                        ImageUrl='<%# ResolveUrl((Eval("ImagePath") as string) ?? "~/Images/Items/item_placeholder.png") %>' />
                </ItemTemplate>
            </asp:TemplateField>

            <%-- 삭제 버튼 --%>
            <asp:TemplateField HeaderText="Delete">
                <ItemTemplate>
                    <asp:Button ID="btnDelete" runat="server"
                        Text="삭제"
                        CssClass="font-normal"
                        CommandName="Delete"
                        OnClientClick='<%# "return confirm(\"정말 삭제할까요?\\n[" 
                                    + Eval("GameName") + "] " + Eval("ItemName") 
                                    + " (#" + Eval("ItemID") + ")\");" %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
