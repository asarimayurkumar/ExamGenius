<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ExamGenius.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ExamGenius - Smart Exam Prep Engine</title>
    <style>
        body { 
            background-color: #0b0f19; 
            color: #f1f5f9; 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            margin: 0; 
            padding: 25px; 
        }
        .container { 
            max-width: 950px; 
            margin: auto; 
        }
        .card { 
            background: #111827; 
            border-radius: 10px; 
            padding: 24px; 
            margin-bottom: 24px; 
            border: 1px solid #1f2937; 
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.4); 
        }
        .header { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
            border-bottom: 1px solid #1f2937; 
            padding-bottom: 15px; 
            margin-bottom: 20px; 
        }
        .stats-badge { 
            background: #0284c7; 
            color: #ffffff; 
            padding: 8px 16px; 
            border-radius: 20px; 
            font-weight: bold; 
            font-size: 14px; 
        }
        input[type="text"], textarea, select { 
            width: 100%; 
            padding: 12px; 
            margin-top: 6px; 
            margin-bottom: 16px; 
            background: #030712; 
            border: 1px solid #374151; 
            color: #ffffff; 
            border-radius: 6px; 
            box-sizing: border-box; 
            font-size: 14px; 
        }
        .btn { 
            background: #4f46e5; 
            color: white; 
            border: none; 
            padding: 12px 24px; 
            border-radius: 6px; 
            cursor: pointer; 
            font-weight: bold; 
            font-size: 15px; 
        }
        .btn:hover { 
            background: #4338ca; 
        }
        .grid-item { 
            background: #030712; 
            padding: 18px; 
            border-radius: 8px; 
            margin-bottom: 14px; 
            border-left: 5px solid #4f46e5; 
            border-top: 1px solid #1f2937; 
            border-right: 1px solid #1f2937; 
            border-bottom: 1px solid #1f2937; 
        }
        .badge { 
            display: inline-block; 
            padding: 4px 10px; 
            border-radius: 4px; 
            font-size: 12px; 
            font-weight: bold; 
            margin-bottom: 10px; 
        }
        .badge-short { background: #059669; }
        .badge-medium { background: #d97706; }
        .badge-long { background: #dc2626; }
        .badge-flash { background: #7c3aed; }
        .badge-fill { background: #0891b2; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h2>⚡ ExamGenius: 9 CGPA Prep Engine</h2>
                <div class="stats-badge">
                    Total XP: <asp:Label ID="lblXP" runat="server" Text="0"></asp:Label> | Level: <asp:Label ID="lblLevel" runat="server" Text="1"></asp:Label>
                </div>
            </div>

            <div class="card">
                <h3>Input Syllabus / Chapter Content</h3>
                <label>Target Subject:</label>
                <asp:DropDownList ID="ddlSubjects" runat="server">
                    <asp:ListItem Text="ASP.NET with C#" Value="1"></asp:ListItem>
                </asp:DropDownList>

                <label>Unit / Chapter Title:</label>
                <asp:TextBox ID="txtUnitTitle" runat="server" Placeholder="e.g., Unit 2: Controls, Validation &amp; State Management"></asp:TextBox>

                <label>Paste Chapter Text / Syllabus Topics / PYQ Data:</label>
                <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="6" Placeholder="Paste syllabus concepts, definitions, architecture descriptions here..."></asp:TextBox>

                <asp:Button ID="btnGenerate" runat="server" Text="Generate Questions &amp; Flashcards" CssClass="btn" OnClick="btnGenerate_Click" />
            </div>

            <div class="card">
                <h3>Generated Question Bank &amp; Active Recall Cards</h3>
                <asp:Repeater ID="rptQuestions" runat="server">
                    <ItemTemplate>
                        <div class="grid-item">
                            <span class='badge <%# GetBadgeClass(Eval("QuestionType").ToString()) %>'>
                                <%# Eval("QuestionType") %> (<%# Eval("Marks") %> Marks)
                            </span>
                            <h4 style="margin: 6px 0 10px 0;"><%# Eval("QuestionText") %></h4>
                            <p style="color: #9ca3af; margin: 0; font-size: 14px;"><strong>Answer Reference:</strong> <%# Eval("CorrectAnswer") %></p>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </form>
</body>
</html>