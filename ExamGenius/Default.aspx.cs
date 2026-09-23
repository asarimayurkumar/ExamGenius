using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace ExamGenius
{
    public partial class Default : System.Web.UI.Page
    {
        private string connStr = ConfigurationManager.ConnectionStrings["ExamGeniusConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStats();
                LoadQuestions();
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            string unitTitle = txtUnitTitle.Text.Trim();
            string content = txtContent.Text.Trim();

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            int unitId = 0;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO SyllabusUnits (SubjectId, UnitNumber, UnitTitle, RawContent) OUTPUT INSERTED.UnitId VALUES (1, 1, @Title, @Content)", con);
                cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(unitTitle) ? "General Unit" : unitTitle);
                cmd.Parameters.AddWithValue("@Content", content);
                unitId = (int)cmd.ExecuteScalar();

                string[] sentences = content.Split(new char[] { '.', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in sentences)
                {
                    string rawText = s.Trim();
                    if (rawText.Length < 20)
                    {
                        continue;
                    }

                    string lower = rawText.ToLower();

                    if (lower.Contains("is defined as") || lower.Contains("is used to") || lower.Contains("refers to") || lower.Contains("stands for"))
                    {
                        InsertQuestion(con, unitId, "2 Mark Question", "Define and explain the primary functionality of: " + rawText.Substring(0, Math.Min(40, rawText.Length)) + "...", rawText, 2);
                        InsertQuestion(con, unitId, "Flashcard", "Flashcard Front: What is " + rawText.Substring(0, Math.Min(30, rawText.Length)) + "?", rawText, 1);
                    }
                    else if (lower.Contains("difference") || lower.Contains("versus") || lower.Contains("types of") || lower.Contains("advantages"))
                    {
                        InsertQuestion(con, unitId, "3 Mark Question", "Explain in detail: " + rawText, rawText, 3);
                    }
                    else if (lower.Contains("architecture") || lower.Contains("lifecycle") || lower.Contains("workflow") || lower.Contains("explain the process"))
                    {
                        InsertQuestion(con, unitId, "5 Mark Question", "Provide a comprehensive breakdown and architecture diagram for: " + rawText, rawText, 5);
                    }
                    else
                    {
                        string[] words = rawText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length >= 6)
                        {
                            string targetWord = words[words.Length / 2];
                            string blankSentence = rawText.Replace(targetWord, "___________");
                            InsertQuestion(con, unitId, "Fill in the Blank", blankSentence, targetWord, 1);
                        }
                    }
                }

                SqlCommand updateXp = new SqlCommand("UPDATE UserProgress SET TotalXP = TotalXP + 100, CurrentLevel = ((TotalXP + 100) / 300) + 1 WHERE ProgressId = 1", con);
                updateXp.ExecuteNonQuery();
            }

            txtUnitTitle.Text = "";
            txtContent.Text = "";
            LoadStats();
            LoadQuestions();
        }

        private void InsertQuestion(SqlConnection con, int unitId, string type, string qText, string ans, int marks)
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO GeneratedQuestions (UnitId, QuestionType, QuestionText, CorrectAnswer, Marks) VALUES (@UnitId, @Type, @QText, @Ans, @Marks)", con);
            cmd.Parameters.AddWithValue("@UnitId", unitId);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@QText", qText);
            cmd.Parameters.AddWithValue("@Ans", ans);
            cmd.Parameters.AddWithValue("@Marks", marks);
            cmd.ExecuteNonQuery();
        }

        private void LoadQuestions()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 25 * FROM GeneratedQuestions ORDER BY QuestionId DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptQuestions.DataSource = dt;
                rptQuestions.DataBind();
            }
        }

        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT TotalXP, CurrentLevel FROM UserProgress WHERE ProgressId = 1", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    lblXP.Text = dr["TotalXP"].ToString();
                    lblLevel.Text = dr["CurrentLevel"].ToString();
                }
            }
        }

        public string GetBadgeClass(string type)
        {
            if (type.Contains("2 Mark")) return "badge-short";
            if (type.Contains("3 Mark")) return "badge-medium";
            if (type.Contains("5 Mark")) return "badge-long";
            if (type.Contains("Flashcard")) return "badge-flash";
            return "badge-fill";
        }
    }
}