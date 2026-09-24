using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ExamGenius
{
    public partial class Default : System.Web.UI.Page
    {
        private static string activeWorkingConn = null;
        private static Random rng = new Random();

        private string GetWorkingConnectionString()
        {
            if (!string.IsNullOrEmpty(activeWorkingConn)) return activeWorkingConn;

            var cfg = ConfigurationManager.ConnectionStrings["ExamGeniusConn"];
            string[] candidateStrings = new string[]
            {
                (cfg != null && !string.IsNullOrEmpty(cfg.ConnectionString)) ? cfg.ConnectionString : "",
                "Data Source=.;Initial Catalog=ExamGeniusDB;Integrated Security=True;",
                "Data Source=.\\SQLEXPRESS;Initial Catalog=ExamGeniusDB;Integrated Security=True;",
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExamGeniusDB;Integrated Security=True;"
            };

            foreach (string cs in candidateStrings)
            {
                if (string.IsNullOrWhiteSpace(cs)) continue;
                try
                {
                    using (SqlConnection testCon = new SqlConnection(cs))
                    {
                        testCon.Open();
                        activeWorkingConn = cs;
                        return cs;
                    }
                }
                catch { }
            }
            return "Data Source=.;Initial Catalog=ExamGeniusDB;Integrated Security=True;";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ScriptManager1 != null && btnDownloadPdf != null)
            {
                ScriptManager1.RegisterPostBackControl(btnDownloadPdf);
            }

            if (!IsPostBack)
            {
                Session["Streak"] = 0;
                Session["TotalAttempts"] = 0;
                Session["CorrectAttempts"] = 0;
                Session["WrongTopicsList"] = new List<string>();
                Session["UploadedFilesMap"] = new Dictionary<string, string>();
                LoadStats();
                CheckAndRestoreSession();
            }
        }

        private void CheckAndRestoreSession()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    con.Open();
                    // Ensure table exists safely
                    string initSql = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentSessionCache')
                                       BEGIN
                                           CREATE TABLE StudentSessionCache (
                                               SessionId INT IDENTITY(1,1) PRIMARY KEY,
                                               UserId INT DEFAULT 1,
                                               RemainingSeconds INT,
                                               ActiveDocCorpus NVARCHAR(MAX),
                                               LastUpdated DATETIME DEFAULT GETDATE()
                                           );
                                       END";
                    using (SqlCommand cmdInit = new SqlCommand(initSql, con))
                    {
                        cmdInit.ExecuteNonQuery();
                    }

                    string q = "SELECT TOP 1 RemainingSeconds, ActiveDocCorpus FROM StudentSessionCache WHERE UserId = 1 ORDER BY SessionId DESC";
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int rem = Convert.ToInt32(dr["RemainingSeconds"]);
                            string corpus = dr["ActiveDocCorpus"].ToString();
                            if (rem > 10 && !string.IsNullOrEmpty(corpus))
                            {
                                pnlResumeBanner.Visible = true;
                                Session["ActiveDocName"] = corpus;
                                lblActiveSource.Text = corpus;
                                LoadCurrentQuestions(50);
                                if (pnlArena.Visible)
                                {
                                    string scriptTimer = string.Format("initializeTimer({0});", rem);
                                    ScriptManager.RegisterStartupScript(this, GetType(), "ResumeTimer", scriptTimer, true);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            int reqMCQ = 0, reqTF = 0, reqShort = 0, reqLong = 0;
            int.TryParse(txtCountMCQ.Text.Trim(), out reqMCQ);
            int.TryParse(txtCountTF.Text.Trim(), out reqTF);
            int.TryParse(txtCountShort.Text.Trim(), out reqShort);
            int.TryParse(txtCountLong.Text.Trim(), out reqLong);

            int totalTarget = reqMCQ + reqTF + reqShort + reqLong;
            if (totalTarget <= 0)
            {
                lblStatus.Text = "Please specify question counts in the target matrix.";
                return;
            }

            string activeMode = hfActiveMode.Value;
            StringBuilder masterCorpus = new StringBuilder();
            StringBuilder pyqCorpus = new StringBuilder();
            string documentTitle = "Pasted Notes";

            // Mode 1: Classic Multi-File Staging
            if (activeMode == "CLASSIC")
            {
                Dictionary<string, string> filesMap = Session["UploadedFilesMap"] as Dictionary<string, string>;
                if (filesMap == null) filesMap = new Dictionary<string, string>();

                if (fileUpload.HasFiles)
                {
                    foreach (HttpPostedFile f in fileUpload.PostedFiles)
                    {
                        string fn = System.IO.Path.GetFileName(f.FileName);
                        if (!filesMap.ContainsKey(fn))
                        {
                            string t = ExtractFileContent(f.InputStream, fn);
                            if (!string.IsNullOrWhiteSpace(t)) filesMap[fn] = t;
                        }
                    }
                    Session["UploadedFilesMap"] = filesMap;
                    BindFilesShelf();
                }

                foreach (var kvp in filesMap) masterCorpus.AppendLine(kvp.Value);
                documentTitle = filesMap.Count > 0 ? string.Format("{0} Uploaded Files", filesMap.Count) : "Standard Material";
            }
            // Mode 2: EXAM Mode
            else
            {
                int docCount = 0;
                if (fuSyllabus.HasFile)
                {
                    masterCorpus.AppendLine(ExtractFileContent(fuSyllabus.FileContent, fuSyllabus.FileName));
                    docCount++;
                }
                if (fuPYQ.HasFiles)
                {
                    foreach (HttpPostedFile f in fuPYQ.PostedFiles)
                    {
                        string p = ExtractFileContent(f.InputStream, f.FileName);
                        masterCorpus.AppendLine(p);
                        pyqCorpus.AppendLine(p);
                        docCount++;
                    }
                }
                if (fuPPT.HasFiles)
                {
                    foreach (HttpPostedFile f in fuPPT.PostedFiles)
                    {
                        masterCorpus.AppendLine(ExtractFileContent(f.InputStream, f.FileName));
                        docCount++;
                    }
                }
                if (fuNotes.HasFiles)
                {
                    foreach (HttpPostedFile f in fuNotes.PostedFiles)
                    {
                        masterCorpus.AppendLine(ExtractFileContent(f.InputStream, f.FileName));
                        docCount++;
                    }
                }
                documentTitle = string.Format("{0} Ingested Sources (Syllabus + PYQ + Slides)", docCount);
                GenerateFrequencyHeatmap(masterCorpus.ToString(), pyqCorpus.ToString());
                pnlPredictorInsights.Visible = true;
            }

            if (!string.IsNullOrEmpty(txtContent.Text.Trim()))
            {
                masterCorpus.AppendLine(txtContent.Text.Trim());
            }

            string rawNotes = masterCorpus.ToString().Trim();
            if (string.IsNullOrEmpty(rawNotes))
            {
                lblStatus.Text = "Please upload files or paste text notes first.";
                return;
            }

            Session["CurrentDocumentContent"] = rawNotes;
            Session["ActiveDocName"] = documentTitle;
            lblActiveSource.Text = documentTitle;
            pnlResumeBanner.Visible = false;

            List<string> processedSentences = ExtractMeaningfulSentences(rawNotes);
            if (processedSentences.Count == 0)
            {
                lblStatus.Text = "Could not parse sufficient concepts. Please ensure document has readable text.";
                return;
            }

            int unitId = 0;
            string connStr = GetWorkingConnectionString();

            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand clearCmd = new SqlCommand("DELETE FROM GeneratedQuestions", con);
                    clearCmd.ExecuteNonQuery();

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO SyllabusUnits (SubjectId, UnitNumber, UnitTitle, RawContent) OUTPUT INSERTED.UnitId VALUES (1, 1, @Title, @Content)", con))
                    {
                        cmd.Parameters.AddWithValue("@Title", documentTitle);
                        cmd.Parameters.AddWithValue("@Content", rawNotes);
                        unitId = (int)cmd.ExecuteScalar();
                    }

                    GenerateQuestionsOfType(con, unitId, "MCQ", reqMCQ, processedSentences);
                    GenerateQuestionsOfType(con, unitId, "TF", reqTF, processedSentences);
                    GenerateQuestionsOfType(con, unitId, "SHORT", reqShort, processedSentences);
                    GenerateQuestionsOfType(con, unitId, "5Mark", reqLong, processedSentences);

                    // Safe Table Cache (Crash-Proof)
                    try
                    {
                        string cacheTableCheck = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentSessionCache')
                                                   BEGIN
                                                       CREATE TABLE StudentSessionCache (
                                                           SessionId INT IDENTITY(1,1) PRIMARY KEY,
                                                           UserId INT DEFAULT 1,
                                                           RemainingSeconds INT,
                                                           ActiveDocCorpus NVARCHAR(MAX),
                                                           LastUpdated DATETIME DEFAULT GETDATE()
                                                       );
                                                   END";
                        using (SqlCommand chk = new SqlCommand(cacheTableCheck, con)) chk.ExecuteNonQuery();

                        int durationMinutes = 60;
                        int.TryParse(txtExamMinutes.Text.Trim(), out durationMinutes);
                        if (durationMinutes <= 0) durationMinutes = 60;

                        using (SqlCommand cacheCmd = new SqlCommand("INSERT INTO StudentSessionCache (RemainingSeconds, ActiveDocCorpus) VALUES (@Rem, @Doc)", con))
                        {
                            cacheCmd.Parameters.AddWithValue("@Rem", durationMinutes * 60);
                            cacheCmd.Parameters.AddWithValue("@Doc", documentTitle);
                            cacheCmd.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "DB Warning: " + ex.Message;
                return;
            }

            txtContent.Text = "";
            lblStatus.Text = string.Format("Quest Ready! Successfully generated {0} custom questions.", totalTarget);
            pnlArena.Visible = true;
            LoadCurrentQuestions(totalTarget);

            // Timer Initialization
            int examMins = 60;
            int.TryParse(txtExamMinutes.Text.Trim(), out examMins);
            if (examMins <= 0) examMins = 60;
            int totalSeconds = examMins * 60;
            string startTimerScript = string.Format("initializeTimer({0});", totalSeconds);
            ScriptManager.RegisterStartupScript(this, GetType(), "StartExamTimer", startTimerScript, true);

            upQuizArena.Update();
            upFileShelf.Update();
        }

        private void GenerateFrequencyHeatmap(string fullCorpus, string pyqCorpus)
        {
            char[] splitters = new char[] { ' ', '\r', '\n', '\t', ',', '.', ';', ':', '(', ')', '"', '\'', '-', '/' };
            string[] allWords = fullCorpus.ToLower().Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> stopWords = new HashSet<string> { "the", "and", "for", "with", "this", "that", "from", "have", "into", "which", "your", "what", "none", "true", "false", "about" };

            var topKeywords = allWords
                .Where(w => w.Length > 4 && !stopWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var item in topKeywords)
            {
                string pillClass = item.Count >= 3 ? "metric-pill prob-high" : "metric-pill";
                sb.Append(string.Format("<span class='{0}'>🔥 {1} ({2}x Repeated)</span>", pillClass, item.Word.ToUpper(), item.Count));
            }

            litTrendingTopics.Text = sb.Length > 0 ? sb.ToString() : "<span class='metric-pill'>Core Framework Concepts</span>";
        }

        private void GenerateQuestionsOfType(SqlConnection con, int unitId, string type, int count, List<string> sentences)
        {
            if (count <= 0) return;
            int generated = 0;
            int idx = 0;
            int pass = 0;

            while (generated < count && pass < 6)
            {
                string text = sentences[idx % sentences.Count];
                string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length >= 3)
                {
                    if (type == "MCQ")
                    {
                        int targetIdx = (words.Length / 2 + pass) % words.Length;
                        string correctWord = CleanWord(words[targetIdx]);
                        if (correctWord.Length < 3)
                        {
                            for (int i = 0; i < words.Length; i++)
                            {
                                string tw = CleanWord(words[i]);
                                if (tw.Length >= 3) { correctWord = tw; break; }
                            }
                        }

                        if (correctWord.Length >= 2)
                        {
                            string prompt = ReplaceFirstWord(text, correctWord, "___________");
                            string opt1 = correctWord;
                            string opt2 = CleanWord(words[0]);
                            string opt3 = CleanWord(words[words.Length - 1]);
                            string opt4 = "None of the above";

                            if (string.IsNullOrEmpty(opt2) || opt2.Equals(opt1, StringComparison.OrdinalIgnoreCase)) opt2 = "Cryptographic Primitive";
                            if (string.IsNullOrEmpty(opt3) || opt3.Equals(opt1, StringComparison.OrdinalIgnoreCase)) opt3 = "Standard Protocol";

                            List<string> optionsList = new List<string> { opt1, opt2, opt3, opt4 };
                            for (int i = optionsList.Count - 1; i > 0; i--)
                            {
                                int k = rng.Next(i + 1);
                                string temp = optionsList[i];
                                optionsList[i] = optionsList[k];
                                optionsList[k] = temp;
                            }

                            InsertQuestion(con, unitId, "MCQ", prompt, optionsList[0], optionsList[1], optionsList[2], optionsList[3], correctWord, 1);
                            generated++;
                        }
                    }
                    else if (type == "TF")
                    {
                        bool makeTrue = (rng.Next(0, 2) == 0);
                        string tfPrompt;
                        string tfAns;

                        if (makeTrue)
                        {
                            tfPrompt = string.Format("Is this concept strictly valid according to material: \"{0}\"?", text);
                            tfAns = "True";
                        }
                        else
                        {
                            string alteredText = text;
                            if (alteredText.IndexOf(" is ", StringComparison.OrdinalIgnoreCase) >= 0)
                                alteredText = Regex.Replace(alteredText, @"\bis\b", "is NOT", RegexOptions.IgnoreCase);
                            else if (alteredText.IndexOf(" are ", StringComparison.OrdinalIgnoreCase) >= 0)
                                alteredText = Regex.Replace(alteredText, @"\bare\b", "are NOT", RegexOptions.IgnoreCase);
                            else
                                alteredText = "NEVER " + alteredText;

                            tfPrompt = string.Format("Is this concept strictly valid according to material: \"{0}\"?", alteredText);
                            tfAns = "False";
                        }

                        InsertQuestion(con, unitId, "TF", tfPrompt, "True", "False", null, null, tfAns, 1);
                        generated++;
                    }
                    else if (type == "SHORT")
                    {
                        string shortQ = "Explain the fundamental mechanism of: " + (text.Length > 50 ? text.Substring(0, 50) + "..." : text);
                        InsertQuestion(con, unitId, "2Mark", shortQ, null, null, null, null, text, 2);
                        generated++;
                    }
                    else if (type == "5Mark")
                    {
                        string longQ = "Analyze and elaborate with technical diagrams/steps: " + text;
                        InsertQuestion(con, unitId, "5Mark", longQ, null, null, null, null, text, 5);
                        generated++;
                    }
                }

                idx++;
                if (idx >= sentences.Count) { idx = 0; pass++; }
            }
        }

        private string ExtractFileContent(Stream fileStream, string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            StringBuilder sb = new StringBuilder();

            try
            {
                if (ext == ".pdf")
                {
                    using (PdfReader reader = new PdfReader(fileStream))
                    {
                        for (int page = 1; page <= reader.NumberOfPages; page++)
                        {
                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                            sb.AppendLine(PdfTextExtractor.GetTextFromPage(reader, page, strategy));
                        }
                    }
                }
                else if (ext == ".pptx")
                {
                    using (ZipArchive zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (entry.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                using (Stream s = entry.Open())
                                using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                                {
                                    string slideXml = sr.ReadToEnd();
                                    MatchCollection textNodes = Regex.Matches(slideXml, @"<a:t[^>]*>(.*?)</a:t>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                    foreach (Match m in textNodes)
                                    {
                                        string val = m.Groups[1].Value.Trim();
                                        if (val.Length > 2) sb.AppendLine(val);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ext == ".docx")
                {
                    using (ZipArchive zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        ZipArchiveEntry entry = zip.GetEntry("word/document.xml");
                        if (entry != null)
                        {
                            using (Stream s = entry.Open())
                            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                            {
                                string docXml = sr.ReadToEnd();
                                MatchCollection docNodes = Regex.Matches(docXml, @"<w:t[^>]*>(.*?)</w:t>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                foreach (Match m in docNodes)
                                {
                                    string val = m.Groups[1].Value.Trim();
                                    if (!string.IsNullOrEmpty(val)) sb.Append(val + " ");
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8, true))
                    {
                        sb.AppendLine(sr.ReadToEnd());
                    }
                }
            }
            catch { return ""; }
            return sb.ToString();
        }

        private void BindFilesShelf()
        {
            Dictionary<string, string> filesMap = Session["UploadedFilesMap"] as Dictionary<string, string>;
            if (filesMap != null && filesMap.Count > 0)
            {
                pnlActiveFiles.Visible = true;
                rptFiles.DataSource = filesMap.Keys;
                rptFiles.DataBind();
            }
            else
            {
                pnlActiveFiles.Visible = false;
            }
        }

        protected void rptFiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "RemoveFile")
            {
                string targetFile = e.CommandArgument.ToString();
                Dictionary<string, string> filesMap = Session["UploadedFilesMap"] as Dictionary<string, string>;
                if (filesMap != null && filesMap.ContainsKey(targetFile))
                {
                    filesMap.Remove(targetFile);
                    Session["UploadedFilesMap"] = filesMap;
                    BindFilesShelf();
                    upFileShelf.Update();
                }
            }
        }

        private List<string> ExtractMeaningfulSentences(string rawContent)
        {
            List<string> result = new List<string>();
            string[] rawLines = rawContent.Split(new char[] { '\n', '\r', '.', ';', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in rawLines)
            {
                string cleaned = CleanText(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;

                string lower = cleaned.ToLower();
                if (lower.Contains("student name") || lower.Contains("roll number") ||
                    lower.Contains("academic presentation") || lower.Contains("semester / div") ||
                    lower.Contains("schemas.") || lower.Length < 18)
                {
                    continue;
                }

                if (cleaned.Length > 160)
                {
                    string[] subParts = cleaned.Split(new char[] { ',', ':', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string part in subParts)
                    {
                        string subClean = part.Trim();
                        if (subClean.Length >= 20 && subClean.Length <= 140)
                            result.Add(subClean);
                    }
                }
                else
                {
                    result.Add(cleaned);
                }
            }
            return result;
        }

        private string ReplaceFirstWord(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (pos < 0) return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private void InsertQuestion(SqlConnection con, int unitId, string type, string qText, string optA, string optB, string optC, string optD, string ans, int marks)
        {
            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO GeneratedQuestions 
                (UnitId, QuestionType, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, Marks, IsCorrect, IsEvaluated) 
                VALUES (@UnitId, @Type, @QText, @OptA, @OptB, @OptC, @OptD, @Ans, @Marks, 0, 0)", con))
            {
                cmd.Parameters.AddWithValue("@UnitId", unitId);
                cmd.Parameters.AddWithValue("@Type", type ?? "General");
                cmd.Parameters.AddWithValue("@QText", qText ?? "");
                cmd.Parameters.AddWithValue("@OptA", (object)TruncateString(optA, 150) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OptB", (object)TruncateString(optB, 150) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OptC", (object)TruncateString(optC, 150) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OptD", (object)TruncateString(optD, 150) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ans", ans ?? "");
                cmd.Parameters.AddWithValue("@Marks", marks);
                cmd.ExecuteNonQuery();
            }
        }

        public string FormatQuestionBody(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Contains("=") || text.Contains("{") || text.Contains("def ") || text.Contains("import ") || text.Contains("print("))
            {
                return string.Format("<div class='terminal-codeblock'>{0}</div>", Server.HtmlEncode(text));
            }
            return string.Format("<div style='font-size:15px; font-weight:600; line-height:1.6; margin-bottom:14px; color:#f1f5f9;'>{0}</div>", Server.HtmlEncode(text));
        }

        private string CleanText(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= ' ' && c <= '~') || c == '\n' || c == '\r' || c == '\t') sb.Append(c);
            }
            return sb.ToString().Trim().TrimStart('•', '-', '*', ' ', '\t', '"', '\'').TrimEnd('"', '\'');
        }

        private string CleanWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return "";
            return CleanText(word).Trim(',', '.', ';', ':', '(', ')', '"', '[', ']', '{', '}', '\'');
        }

        private string TruncateString(string val, int maxLen)
        {
            if (string.IsNullOrEmpty(val)) return val;
            return val.Length <= maxLen ? val : val.Substring(0, maxLen);
        }

        protected void btnShuffle_Click(object sender, EventArgs e)
        {
            LoadCurrentQuestions(50);
            upQuizArena.Update();
        }

        protected void btnResetSession_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    con.Open();
                    SqlCommand clearCmd = new SqlCommand("DELETE FROM GeneratedQuestions; IF OBJECT_ID('StudentSessionCache', 'U') IS NOT NULL DELETE FROM StudentSessionCache WHERE UserId = 1;", con);
                    clearCmd.ExecuteNonQuery();
                }
            }
            catch { }

            Session["TotalAttempts"] = 0;
            Session["CorrectAttempts"] = 0;
            Session["Streak"] = 0;
            Session["WrongTopicsList"] = new List<string>();
            lblStreak.Text = "0";
            lblAccuracy.Text = "100%";
            pnlArena.Visible = false;
            pnlSummaryCard.Visible = false;
            pnlPredictorInsights.Visible = false;
            pnlResumeBanner.Visible = false;
            lblStatus.Text = "Corpus and questions reset successfully.";
            upStats.Update();
            upQuizArena.Update();
        }

        protected void btnCompleteTest_Click(object sender, EventArgs e)
        {
            int total = Session["TotalAttempts"] != null ? (int)Session["TotalAttempts"] : 0;
            int correct = Session["CorrectAttempts"] != null ? (int)Session["CorrectAttempts"] : 0;
            int wrong = total - correct;
            string acc = total > 0 ? ((correct * 100) / total) + "%" : "0%";

            string grade = "Needs Systematic Revision";
            if (total > 0)
            {
                double pct = (correct * 100.0) / total;
                if (pct >= 90) grade = "10.0 CGPA Candidate (Outstanding A+)";
                else if (pct >= 80) grade = "9.0+ CGPA (First Class Distinction)";
                else if (pct >= 65) grade = "7.5+ CGPA (Good Understanding)";
            }

            litSummaryDetails.Text = string.Format("• Total Answered: <strong>{0}</strong> | Correct: <strong style='color:#34d399;'>{1}</strong> | Mistakes: <strong style='color:#f87171;'>{2}</strong><br/>" +
                                     "• Exam Accuracy: <strong>{3}</strong><br/>" +
                                     "• University Standing: <strong style='color:#38bdf8;'>{4}</strong>", total, correct, wrong, acc, grade);

            pnlSummaryCard.Visible = true;
            upQuizArena.Update();
        }

        protected void btnDownloadPdf_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    string query = @"SELECT QuestionId, QuestionType, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, Marks, 
                                     ISNULL(UserAnswer, '') AS UserAnswer, ISNULL(IsEvaluated, 0) AS IsEvaluated, ISNULL(IsCorrect, 0) AS IsCorrect 
                                     FROM GeneratedQuestions ORDER BY QuestionId ASC";
                    using (SqlDataAdapter da = new SqlDataAdapter(query, con)) da.Fill(dt);
                }
            }
            catch { return; }

            if (dt.Rows.Count == 0) return;

            using (MemoryStream ms = new MemoryStream())
            {
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 38f, 38f, 40f, 40f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);
                writer.PageEvent = new UniversityPdfWatermark();
                pdfDoc.Open();

                Font uniFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15, new BaseColor(15, 23, 42));
                Font subHead = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                Font bodyMeta = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                Font qFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                Font optFont = FontFactory.GetFont(FontFactory.HELVETICA, 9.5f, BaseColor.DARK_GRAY);
                Font greenFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9.5f, new BaseColor(16, 185, 129));
                Font redFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9.5f, new BaseColor(239, 68, 68));

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 65f, 35f });

                PdfPCell cellLeft = new PdfPCell();
                cellLeft.Border = iTextSharp.text.Rectangle.BOX;
                cellLeft.Padding = 8f;
                cellLeft.AddElement(new Paragraph("PARUL UNIVERSITY - FACULTY OF IT & COMPUTER APPLICATIONS", uniFont));
                cellLeft.AddElement(new Paragraph("SEMESTER ASSESSMENT EXAMINATION", subHead));
                cellLeft.AddElement(new Paragraph(string.Format("Source Corpus: {0}", Session["ActiveDocName"] ?? "Exam Assessment Suite"), bodyMeta));

                PdfPCell cellRight = new PdfPCell();
                cellRight.Border = iTextSharp.text.Rectangle.BOX;
                cellRight.Padding = 8f;
                cellRight.AddElement(new Paragraph("Max Marks: 100", bodyMeta));
                cellRight.AddElement(new Paragraph(string.Format("Date: {0:dd-MMM-yyyy}", DateTime.Now), bodyMeta));
                cellRight.AddElement(new Paragraph("Roll No: __________________", bodyMeta));
                cellRight.AddElement(new Paragraph("Sign: _____________________", bodyMeta));

                headerTable.AddCell(cellLeft);
                headerTable.AddCell(cellRight);
                pdfDoc.Add(headerTable);

                pdfDoc.Add(new Paragraph("\n"));

                int qNum = 1;
                foreach (DataRow r in dt.Rows)
                {
                    string type = r["QuestionType"].ToString();
                    string text = r["QuestionText"].ToString();
                    string marks = r["Marks"].ToString();
                    bool isEvaluated = Convert.ToBoolean(r["IsEvaluated"]);
                    bool isCorrect = Convert.ToBoolean(r["IsCorrect"]);
                    string userAns = r["UserAnswer"].ToString();
                    string correctAns = r["CorrectAnswer"].ToString();

                    pdfDoc.Add(new Paragraph(string.Format("Q.{0}  [{1} - {2} Mark(s)]  {3}", qNum, type, marks, text), qFont));

                    if (type == "MCQ")
                    {
                        pdfDoc.Add(new Paragraph(string.Format("     A) {0}       B) {1}", r["OptionA"], r["OptionB"]), optFont));
                        pdfDoc.Add(new Paragraph(string.Format("     C) {0}       D) {1}", r["OptionC"], r["OptionD"]), optFont));
                    }
                    else if (type == "TF")
                    {
                        pdfDoc.Add(new Paragraph("     [   ] True        [   ] False", optFont));
                    }

                    if (isEvaluated)
                    {
                        if (isCorrect)
                            pdfDoc.Add(new Paragraph(string.Format("     [✓ Correct] Candidate Response: {0}", userAns), greenFont));
                        else
                        {
                            pdfDoc.Add(new Paragraph(string.Format("     [✗ Incorrect] Candidate Response: {0}", userAns), redFont));
                            pdfDoc.Add(new Paragraph(string.Format("     Correct Key: {0}", correctAns), greenFont));
                        }
                    }
                    else
                    {
                        pdfDoc.Add(new Paragraph(string.Format("     Official Reference Key: {0}", correctAns), greenFont));
                    }

                    pdfDoc.Add(new Paragraph("\n"));
                    qNum++;
                }

                pdfDoc.Close();
                byte[] bytes = ms.ToArray();

                Response.Clear();
                Response.ClearHeaders();
                Response.Buffer = true;
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=ExamGenius_Assessment_Paper.pdf");
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        private void LoadCurrentQuestions(int topCount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    string query = string.Format(@"SELECT TOP ({0}) QuestionId, QuestionType, QuestionText, OptionA, OptionB, OptionC, OptionD, 
                                     CorrectAnswer, Marks, ISNULL(IsCorrect, 0) AS IsCorrect 
                                     FROM GeneratedQuestions ORDER BY QuestionId ASC", topCount);

                    using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            pnlArena.Visible = true;
                            rptQuestions.DataSource = dt;
                            rptQuestions.DataBind();
                        }
                        else
                        {
                            pnlArena.Visible = false;
                        }
                    }
                }
            }
            catch { pnlArena.Visible = false; }
        }

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            HiddenField hfAns = (HiddenField)e.Item.FindControl("hfCorrectAnswer");
            HiddenField hfQId = (HiddenField)e.Item.FindControl("hfQuestionId");
            Panel pnlFeedback = (Panel)e.Item.FindControl("pnlFeedback");
            Literal litTitle = (Literal)e.Item.FindControl("litFeedbackTitle");

            int qId = Convert.ToInt32(hfQId.Value);
            string expected = Server.HtmlDecode(hfAns.Value).Trim();
            bool isCorrect = false;
            string studentAnswer = "";

            int total = Session["TotalAttempts"] != null ? (int)Session["TotalAttempts"] + 1 : 1;
            int correct = Session["CorrectAttempts"] != null ? (int)Session["CorrectAttempts"] : 0;
            int streak = Session["Streak"] != null ? (int)Session["Streak"] : 0;

            if (e.CommandName == "VerifyAnswer")
            {
                studentAnswer = (e.CommandArgument != null) ? e.CommandArgument.ToString().Trim() : "";
                isCorrect = string.Equals(studentAnswer, expected, StringComparison.OrdinalIgnoreCase);
            }
            else if (e.CommandName == "CheckDescriptive")
            {
                TextBox txt = (TextBox)e.Item.FindControl("txtStudentAnswer");
                studentAnswer = (txt != null) ? txt.Text.Trim() : "";

                if (!string.IsNullOrEmpty(studentAnswer))
                {
                    string[] words = studentAnswer.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int match = 0;
                    string expLower = expected.ToLower();
                    foreach (string w in words)
                    {
                        if (w.Length > 3 && expLower.Contains(w)) match++;
                    }
                    if (match >= 1 || expLower.Contains(studentAnswer.ToLower())) isCorrect = true;
                }
            }

            if (isCorrect)
            {
                correct++;
                streak++;
            }
            else
            {
                streak = 0;
                List<string> wrongTopics = Session["WrongTopicsList"] as List<string>;
                if (wrongTopics == null) wrongTopics = new List<string>();
                string topicKey = expected.Length > 25 ? expected.Substring(0, 25) + "..." : expected;
                if (!wrongTopics.Contains(topicKey)) wrongTopics.Add(topicKey);
                Session["WrongTopicsList"] = wrongTopics;
            }

            Session["TotalAttempts"] = total;
            Session["CorrectAttempts"] = correct;
            Session["Streak"] = streak;

            lblStreak.Text = streak.ToString();
            lblAccuracy.Text = ((correct * 100) / total) + "%";

            int xpEarned = isCorrect ? 50 : 0;

            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE GeneratedQuestions SET UserAnswer = @UAns, IsEvaluated = 1, IsCorrect = @IsCorrect WHERE QuestionId = @QId", con))
                    {
                        cmd.Parameters.AddWithValue("@UAns", studentAnswer);
                        cmd.Parameters.AddWithValue("@IsCorrect", isCorrect);
                        cmd.Parameters.AddWithValue("@QId", qId);
                        cmd.ExecuteNonQuery();
                    }

                    if (isCorrect)
                    {
                        using (SqlCommand xpCmd = new SqlCommand("UPDATE UserProgress SET TotalXP = TotalXP + @Xp, CurrentLevel = ((TotalXP + @Xp) / 250) + 1 WHERE ProgressId = 1", con))
                        {
                            xpCmd.Parameters.AddWithValue("@Xp", xpEarned);
                            xpCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }

            pnlFeedback.Visible = true;
            pnlFeedback.CssClass = isCorrect ? "feedback-state-box state-correct" : "feedback-state-box state-wrong";
            if (litTitle != null) litTitle.Text = isCorrect ? "✓ Correct Answer! (+50 XP)" : "✗ Incorrect Answer";

            LoadStats();
            upStats.Update();
            upQuizArena.Update();
        }

        private void LoadStats()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetWorkingConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TotalXP, CurrentLevel FROM UserProgress WHERE ProgressId = 1", con))
                    {
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                lblXP.Text = dr["TotalXP"].ToString();
                                lblLevel.Text = dr["CurrentLevel"].ToString();
                            }
                        }
                    }
                }
            }
            catch
            {
                lblXP.Text = "0";
                lblLevel.Text = "1";
            }
        }
    }

    public class UniversityPdfWatermark : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, iTextSharp.text.Document targetDoc)
        {
            PdfContentByte cb = writer.DirectContentUnder;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.BeginText();
            cb.SetColorFill(new BaseColor(240, 243, 246));
            cb.SetFontAndSize(bf, 42);
            cb.ShowTextAligned(iTextSharp.text.Element.ALIGN_CENTER, "EXAMGENIUS SECURE ASSESSMENT", targetDoc.PageSize.Width / 2, targetDoc.PageSize.Height / 2, 45f);
            cb.EndText();
        }
    }
}