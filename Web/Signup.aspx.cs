using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniPlayWebSite
{
    public partial class Signup : System.Web.UI.Page
    {
        protected void btnSignup_Click(object sender, EventArgs e)
        {
            string id = txtID.Text.Trim();
            string pw = txtPWD.Text.Trim();
            string nick = txtNick.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(nick))
            {
                lblResult.Text = "모든 항목을 입력해주세요.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["GameDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // ID 중복 체크
                string checkIDSql = "SELECT COUNT(*) FROM Members WHERE UserID = @id";
                using (SqlCommand cmd = new SqlCommand(checkIDSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        lblResult.Text = "이미 사용 중인 ID입니다.";
                        return;
                    }
                }

                // NickName 중복 체크
                string checkNickSql = "SELECT COUNT(*) FROM Members WHERE NickName = @nick";
                using (SqlCommand cmd = new SqlCommand(checkNickSql, conn))
                {
                    cmd.Parameters.AddWithValue("@nick", nick);
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        lblResult.Text = "이미 사용 중인 닉네임입니다.";
                        return;
                    }
                }

                // 회원가입 INSERT (기본값 UGrade=0, Coin=0)
                string insertSql = @"INSERT INTO Members (UserID, PassWd, NickName, UGrade, Coin)
                                     VALUES (@id, @pw, @nick, 0, 0)";
                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pw", pw);
                    cmd.Parameters.AddWithValue("@nick", nick);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {// === 기본 아이템 지급 로직 추가 ===
                        string giveItemsSql = @"
                        INSERT INTO UserItems (UserID, GameID, ItemID, ItemTypeID, IsEquipped, PurchaseDate)
                        SELECT
                            @U,
                            I.GameID,
                            I.ItemID,
                            I.ItemTypeID,
                            CASE 
                                WHEN ROW_NUMBER() OVER (PARTITION BY I.GameID, I.ItemTypeID ORDER BY I.ItemID) = 1 THEN 1
                                ELSE 0
                            END AS IsEquipped,
                            GETDATE()
                        FROM Items I
                        WHERE I.Price = 0;   -- 기본 지급 기준(네가 0원으로 표시한 것들)
                        ";

                        /*
                        string giveItemsSql = @"
                            -- 슈팅(1)
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 1, 1, 1);   -- 기능형
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 1, 2, 1);   -- 치장형 (Shooting_Default)

                            -- 런(2)
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 2, 1, 1);   -- 기능형
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 2, 2, 1);   -- 치장형 (Run_Default)

                            -- 수박(3)
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 3, 1, 1);   -- 기능형
                            INSERT INTO UserItems (UserID, GameID, ItemID, IsEquipped)
                            VALUES (@U, 3, 2, 1);   -- 치장형 (Puzzle_Default)
                        ";
                        */

                        using (SqlCommand giveCmd = new SqlCommand(giveItemsSql, conn))
                        {
                            giveCmd.Parameters.AddWithValue("@U", id);
                            giveCmd.ExecuteNonQuery();
                        }

                        lblResult.ForeColor = System.Drawing.Color.LimeGreen;
                        lblResult.Text = "회원가입이 완료되었습니다! 기본 아이템이 지급되었습니다.";
                    }
                    else
                    {
                        lblResult.Text = "회원가입에 실패했습니다.";
                    }
                }
            }
        }
    }
}