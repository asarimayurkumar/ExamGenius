<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ExamGenius.Default" ValidateRequest="false" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ExamGenius - Assessment & Exam Predictor Arena</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;600;700&display=swap" rel="stylesheet" />

    <style>
        :root {
            --bg-dark: #040814;
            --glass-card: rgba(15, 23, 42, 0.65);
            --glass-border: rgba(255, 255, 255, 0.1);
            --accent-cyan: #38bdf8;
            --accent-indigo: #818cf8;
            --text-main: #f1f5f9;
            --text-sub: #cbd5e1;
            --text-muted: #94a3b8;
        }

        * { 
            box-sizing: border-box; 
            font-family: 'Plus Jakarta Sans', sans-serif; 
            -webkit-font-smoothing: antialiased; 
        }

        body { 
            margin: 0; 
            padding: 30px 20px; 
            background-color: var(--bg-dark); 
            color: var(--text-main); 
            min-height: 100vh; 
            position: relative; 
            overflow-x: hidden; 
        }

        .ambient-glow-1 { 
            position: fixed; 
            top: -140px; 
            left: -120px; 
            width: 600px; 
            height: 600px; 
            background: radial-gradient(circle, rgba(99, 102, 241, 0.22), transparent 70%); 
            border-radius: 50%; 
            filter: blur(120px); 
            pointer-events: none; 
            z-index: 0; 
        }
        .ambient-glow-2 { 
            position: fixed; 
            bottom: -160px; 
            right: -120px; 
            width: 650px; 
            height: 650px; 
            background: radial-gradient(circle, rgba(56, 189, 248, 0.18), transparent 70%); 
            border-radius: 50%; 
            filter: blur(130px); 
            pointer-events: none; 
            z-index: 0; 
        }

        .container { 
            max-width: 1020px; 
            margin: 0 auto; 
            position: relative; 
            z-index: 10; 
        }

        .glass-panel { 
            background: var(--glass-card); 
            backdrop-filter: blur(28px); 
            -webkit-backdrop-filter: blur(28px); 
            border: 1px solid var(--glass-border); 
            border-radius: 22px; 
            padding: 26px 30px; 
            margin-bottom: 22px; 
            box-shadow: 0 20px 45px -10px rgba(0, 0, 0, 0.65), inset 0 1px 0 rgba(255, 255, 255, 0.1); 
        }

        .top-header { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
            margin-bottom: 20px; 
            flex-wrap: wrap; 
            gap: 14px; 
        }
        .brand { 
            font-size: 24px; 
            font-weight: 800; 
            background: linear-gradient(135deg, #38bdf8 0%, #a5b4fc 100%); 
            -webkit-background-clip: text; 
            -webkit-text-fill-color: transparent; 
            letter-spacing: -0.4px; 
            display: flex; 
            align-items: center; 
            gap: 10px; 
        }
        .brand-badge { 
            font-size: 11px; 
            background: rgba(56, 189, 248, 0.14); 
            border: 1px solid rgba(56, 189, 248, 0.35); 
            color: #38bdf8; 
            -webkit-text-fill-color: #38bdf8; 
            padding: 3px 10px; 
            border-radius: 20px; 
            font-weight: 700; 
            text-transform: uppercase; 
        }

        .stats-wrap { 
            display: flex; 
            gap: 10px; 
            flex-wrap: wrap; 
        }
        .stat-item { 
            background: rgba(15, 23, 42, 0.65); 
            backdrop-filter: blur(12px); 
            border: 1px solid rgba(255, 255, 255, 0.08); 
            color: var(--text-muted); 
            padding: 7px 16px; 
            border-radius: 24px; 
            font-size: 13px; 
            font-weight: 600; 
        }
        .stat-item span { 
            color: #38bdf8; 
            font-weight: 700; 
        }
        .timer-warning { 
            color: #f87171 !important; 
            animation: pulse 1s infinite; 
        }
        @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.35; } }

        /* Modern Transparent Tab Switcher */
        .mode-nav-box {
            display: flex;
            background: rgba(2, 6, 23, 0.6);
            border: 1px solid rgba(255, 255, 255, 0.08);
            border-radius: 14px;
            padding: 6px;
            margin-bottom: 22px;
            gap: 8px;
        }
        .mode-tab-btn {
            flex: 1;
            background: transparent;
            border: 1px solid transparent;
            color: var(--text-muted);
            padding: 12px 18px;
            border-radius: 10px;
            font-size: 13px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.28s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 9px;
        }
        .mode-tab-btn:hover {
            color: #f1f5f9;
            background: rgba(255, 255, 255, 0.03);
        }
        .mode-tab-btn.active {
            background: linear-gradient(135deg, rgba(37, 99, 235, 0.35), rgba(79, 70, 229, 0.45));
            border-color: rgba(56, 189, 248, 0.4);
            color: #ffffff;
            box-shadow: 0 4px 18px rgba(56, 189, 248, 0.2);
        }
        .ui-icon {
            width: 16px;
            height: 16px;
            stroke: currentColor;
            stroke-width: 2;
            fill: none;
            stroke-linecap: round;
            stroke-linejoin: round;
            opacity: 0.85;
            transition: transform 0.2s ease;
        }
        .mode-tab-btn.active .ui-icon {
            opacity: 1;
            transform: scale(1.08);
        }

        /* Smooth Tab Transition Animations */
        .tab-content-area {
            animation: tabFadeSlide 0.32s cubic-bezier(0.4, 0, 0.2, 1);
            will-change: transform, opacity;
        }
        @keyframes tabFadeSlide {
            0% {
                opacity: 0;
                transform: translateY(8px);
            }
            100% {
                opacity: 1;
                transform: translateY(0);
            }
        }

        label { 
            display: block; 
            font-size: 11px; 
            text-transform: uppercase; 
            letter-spacing: 0.06em; 
            color: var(--text-muted); 
            margin-bottom: 7px; 
            font-weight: 700; 
        }

        .file-box { 
            border: 1px dashed rgba(255, 255, 255, 0.16); 
            background: rgba(2, 6, 23, 0.35); 
            border-radius: 12px; 
            padding: 14px 18px; 
            margin-bottom: 14px; 
            transition: border-color 0.2s;
        }
        .file-box:hover {
            border-color: var(--accent-cyan);
        }
        .file-list-shelf { 
            display: flex; 
            gap: 8px; 
            flex-wrap: wrap; 
            margin-bottom: 16px; 
        }
        .file-chip { 
            background: rgba(30, 41, 59, 0.85); 
            border: 1px solid rgba(56, 189, 248, 0.35); 
            color: #38bdf8; 
            padding: 6px 14px; 
            border-radius: 20px; 
            font-size: 12.5px; 
            font-weight: 600; 
            display: flex; 
            align-items: center; 
            gap: 8px; 
        }
        .file-chip-btn { 
            background: none; 
            border: none; 
            color: #f87171; 
            font-weight: 800; 
            cursor: pointer; 
            padding: 0 3px; 
        }

        .buckets-grid { 
            display: grid; 
            grid-template-columns: repeat(4, minmax(0, 1fr)); 
            gap: 12px; 
            margin-bottom: 18px; 
            width: 100%;
        }
        @media(max-width: 900px) { .buckets-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
        @media(max-width: 500px) { .buckets-grid { grid-template-columns: 1fr; } }

        .bucket-card { 
            background: rgba(2, 6, 23, 0.5); 
            border: 1px dashed rgba(255, 255, 255, 0.16); 
            border-radius: 12px; 
            padding: 12px 10px; 
            min-width: 0;
            overflow: hidden;
            transition: border-color 0.2s;
        }
        .bucket-card:hover { border-color: var(--accent-cyan); }
        .bucket-title { 
            font-size: 11px; 
            font-weight: 700; 
            color: #38bdf8; 
            margin-bottom: 8px; 
            white-space: nowrap; 
            overflow: hidden; 
            text-overflow: ellipsis; 
            display: flex;
            align-items: center;
            gap: 6px;
        }
        .bucket-card input[type="file"] {
            width: 100%;
            font-size: 11px;
            color: #94a3b8;
            margin: 0;
            padding: 0;
            background: transparent;
            border: none;
            cursor: pointer;
        }
        .bucket-card input[type="file"]::-webkit-file-upload-button {
            background: rgba(30, 41, 59, 0.9);
            border: 1px solid rgba(255, 255, 255, 0.15);
            color: #f1f5f9;
            padding: 4px 8px;
            border-radius: 6px;
            font-size: 11px;
            font-weight: 600;
            cursor: pointer;
        }

        .matrix-grid { 
            display: grid; 
            grid-template-columns: repeat(4, minmax(0, 1fr)); 
            gap: 12px; 
            margin-bottom: 16px; 
            width: 100%;
        }
        @media(max-width: 700px) { .matrix-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
        .matrix-box { 
            background: rgba(2, 6, 23, 0.45); 
            border: 1px solid rgba(255, 255, 255, 0.1); 
            border-radius: 12px; 
            padding: 10px 14px; 
            min-width: 0;
        }

        input[type="text"], input[type="number"], textarea, select { 
            width: 100%; 
            padding: 12px 14px; 
            background: rgba(2, 6, 23, 0.65); 
            backdrop-filter: blur(14px); 
            border: 1px solid rgba(255, 255, 255, 0.1); 
            color: var(--text-main); 
            border-radius: 10px; 
            margin-bottom: 12px; 
            font-size: 13.5px; 
            outline: none; 
        }
        input[type="text"]:focus, input[type="number"]:focus, textarea:focus { 
            border-color: var(--accent-indigo); 
            box-shadow: 0 0 16px rgba(99, 102, 241, 0.35); 
            background: rgba(2, 6, 23, 0.85); 
        }

        .btn-primary { 
            width: 100%; 
            background: linear-gradient(135deg, #2563eb 0%, #4f46e5 50%, #6366f1 100%); 
            color: #ffffff; 
            border: 1px solid rgba(255, 255, 255, 0.18); 
            padding: 15px; 
            border-radius: 12px; 
            font-size: 14px; 
            font-weight: 700; 
            letter-spacing: 0.04em; 
            cursor: pointer; 
            transition: transform 0.15s, box-shadow 0.2s; 
            box-shadow: 0 8px 25px rgba(79, 70, 229, 0.45); 
        }
        .btn-primary:hover { 
            transform: translateY(-2px); 
            box-shadow: 0 12px 30px rgba(99, 102, 241, 0.6); 
        }

        .btn-secondary { 
            background: rgba(30, 41, 59, 0.7); 
            color: var(--text-muted); 
            border: 1px solid rgba(255, 255, 255, 0.1); 
            padding: 8px 16px; 
            border-radius: 10px; 
            font-size: 12.5px; 
            font-weight: 600; 
            cursor: pointer; 
            display: inline-flex; 
            align-items: center; 
            gap: 6px; 
        }
        .btn-secondary:hover { 
            color: #ffffff; 
            border-color: var(--accent-cyan); 
            background: rgba(56, 189, 248, 0.15); 
        }

        .btn-download {
            background: linear-gradient(135deg, #0284c7 0%, #2563eb 100%);
            color: #ffffff;
            border: 1px solid rgba(56, 189, 248, 0.4);
            box-shadow: 0 4px 15px rgba(37, 99, 235, 0.35);
        }

        .predictor-box { 
            background: rgba(15, 23, 42, 0.85); 
            border: 1px solid rgba(56, 189, 248, 0.4); 
            border-radius: 16px; 
            padding: 20px; 
            margin-bottom: 22px; 
        }
        .metric-pill { 
            display: inline-flex; 
            align-items: center; 
            gap: 6px; 
            background: rgba(16, 185, 129, 0.15); 
            border: 1px solid rgba(16, 185, 129, 0.4); 
            color: #34d399; 
            padding: 5px 12px; 
            border-radius: 18px; 
            font-size: 12px; 
            font-weight: 700; 
            margin-right: 8px; 
            margin-bottom: 8px; 
        }
        .prob-high { 
            background: rgba(244, 63, 94, 0.18); 
            border-color: rgba(244, 63, 94, 0.45); 
            color: #fb7185; 
        }

        .question-card { 
            background: rgba(10, 16, 32, 0.65); 
            backdrop-filter: blur(18px); 
            border: 1px solid rgba(255, 255, 255, 0.08); 
            border-radius: 16px; 
            padding: 22px 24px; 
            margin-bottom: 20px; 
        }
        .q-meta { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
            font-size: 12px; 
            color: var(--accent-indigo); 
            font-weight: 700; 
            margin-bottom: 12px; 
        }
        .q-num-badge { 
            background: rgba(56, 189, 248, 0.12); 
            border: 1px solid rgba(56, 189, 248, 0.3); 
            color: #38bdf8; 
            padding: 2px 10px; 
            border-radius: 8px; 
            font-weight: 700; 
            font-size: 12px; 
        }
        .predict-badge { 
            background: rgba(245, 158, 11, 0.18); 
            border: 1px solid rgba(245, 158, 11, 0.4); 
            color: #fbbf24; 
            padding: 2px 8px; 
            border-radius: 6px; 
            font-size: 11px; 
            font-weight: 700; 
        }
        
        .terminal-codeblock { 
            background: #020617; 
            border: 1px solid rgba(56, 189, 248, 0.25); 
            border-radius: 10px; 
            padding: 12px 16px; 
            margin-bottom: 14px; 
            font-family: 'JetBrains Mono', monospace; 
            font-size: 13px; 
            color: #38bdf8; 
            line-height: 1.5; 
            overflow-x: auto; 
        }
        .mcq-grid { 
            display: grid; 
            grid-template-columns: 1fr 1fr; 
            gap: 12px; 
            margin-top: 14px; 
        }
        @media(max-width: 600px) { .mcq-grid { grid-template-columns: 1fr; } }
        .btn-option { 
            background: rgba(15, 23, 42, 0.7); 
            border: 1px solid rgba(255, 255, 255, 0.1); 
            color: var(--text-sub); 
            padding: 14px 18px; 
            border-radius: 12px; 
            font-size: 14px; 
            text-align: left; 
            cursor: pointer; 
            width: 100%; 
            transition: all 0.2s; 
        }
        .btn-option:hover { 
            border-color: var(--accent-indigo); 
            background: rgba(99, 102, 241, 0.18); 
            color: #ffffff; 
        }

        .feedback-state-box { 
            margin-top: 16px; 
            padding: 16px 20px; 
            border-radius: 12px; 
            font-size: 14px; 
            line-height: 1.5; 
        }
        .state-correct { 
            background: rgba(16, 185, 129, 0.12) !important; 
            border: 1px solid #10b981 !important; 
            color: #34d399 !important; 
        }
        .state-wrong { 
            background: rgba(239, 68, 68, 0.12) !important; 
            border: 1px solid #ef4444 !important; 
            color: #f87171 !important; 
        }
        
        .resume-banner { 
            background: rgba(37, 99, 235, 0.2); 
            border: 1px solid var(--accent-cyan); 
            border-radius: 12px; 
            padding: 12px 18px; 
            margin-bottom: 18px; 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
        }

        .action-bar { 
            display: flex; 
            gap: 10px; 
            flex-wrap: wrap; 
        }
    </style>

    <script>
        let countdownTimer = null;
        let totalSecondsLeft = 0;

        function cacheStateLocally(remainingSec) {
            localStorage.setItem("ExamGenius_TimerSeconds", remainingSec);
            localStorage.setItem("ExamGenius_LastActiveTimestamp", new Date().getTime());
        }

        function initializeTimer(durationSeconds) {
            if (countdownTimer) clearInterval(countdownTimer);
            totalSecondsLeft = parseInt(durationSeconds) > 0 ? parseInt(durationSeconds) : 3600;

            let cachedSec = localStorage.getItem("ExamGenius_TimerSeconds");
            if (cachedSec && parseInt(cachedSec) > 0 && parseInt(cachedSec) < totalSecondsLeft) {
                totalSecondsLeft = parseInt(cachedSec);
            }

            countdownTimer = setInterval(() => {
                totalSecondsLeft--;
                cacheStateLocally(totalSecondsLeft);

                let m = Math.floor(totalSecondsLeft / 60);
                let s = totalSecondsLeft % 60;
                let display = (m < 10 ? "0" + m : m) + ":" + (s < 10 ? "0" + s : s);
                let el = document.getElementById("lblLiveTimer");

                if (el) {
                    el.innerText = display;
                    if (totalSecondsLeft <= 120) el.classList.add("timer-warning");
                }

                if (totalSecondsLeft <= 0) {
                    clearInterval(countdownTimer);
                    localStorage.removeItem("ExamGenius_TimerSeconds");
                    alert("Time Expired! Automatically submitting assessment.");
                    let finishBtn = document.getElementById('<%= btnCompleteTest.ClientID %>');
                    if (finishBtn) finishBtn.click();
                }
            }, 1000);
        }

        function clearExamCache() {
            localStorage.removeItem("ExamGenius_TimerSeconds");
            localStorage.removeItem("ExamGenius_LastActiveTimestamp");
        }

        function switchMode(mode) {
            let pnlClassic = document.getElementById("pnlClassicUpload");
            let pnlExam = document.getElementById("pnlExamBuckets");
            let tabClassic = document.getElementById("tabClassic");
            let tabExam = document.getElementById("tabExam");
            let hfMode = document.getElementById('<%= hfActiveMode.ClientID %>');

            if (mode === 'EXAM') {
                pnlClassic.style.display = 'none';
                pnlExam.style.display = 'block';
                pnlExam.classList.remove('tab-content-area');
                void pnlExam.offsetWidth; // Force reflow for smooth animation
                pnlExam.classList.add('tab-content-area');

                tabClassic.classList.remove('active');
                tabExam.classList.add('active');
                hfMode.value = 'EXAM';
            } else {
                pnlClassic.style.display = 'block';
                pnlClassic.classList.remove('tab-content-area');
                void pnlClassic.offsetWidth; // Force reflow for smooth animation
                pnlClassic.classList.add('tab-content-area');

                pnlExam.style.display = 'none';
                tabClassic.classList.add('active');
                tabExam.classList.remove('active');
                hfMode.value = 'CLASSIC';
            }
        }
    </script>
</head>
<body>
    <div class="ambient-glow-1"></div>
    <div class="ambient-glow-2"></div>

    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
        <asp:HiddenField ID="hfActiveMode" runat="server" Value="CLASSIC" />

        <div class="container">
            <asp:UpdatePanel ID="upStats" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="top-header">
                        <div class="brand">
                            ExamGenius <span class="brand-badge">Arena Suite</span>
                        </div>
                        <div class="stats-wrap">
                            <div class="stat-item">XP: <span><asp:Label ID="lblXP" runat="server" Text="0"></asp:Label></span> (Lvl <span><asp:Label ID="lblLevel" runat="server" Text="1"></asp:Label></span>)</div>
                            <div class="stat-item">Streak: <span><asp:Label ID="lblStreak" runat="server" Text="0"></asp:Label></span></div>
                            <div class="stat-item">Accuracy: <span><asp:Label ID="lblAccuracy" runat="server" Text="100%"></asp:Label></span></div>
                            <div class="stat-item">Timer: <span id="lblLiveTimer">60:00</span></div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <!-- Mode Switcher Tabs (Transparent UI Vector Icons) -->
            <div class="mode-nav-box">
                <button type="button" id="tabClassic" class="mode-tab-btn active" onclick="switchMode('CLASSIC')">
                    <svg class="ui-icon" viewBox="0 0 24 24"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"></path></svg>
                    Standard Upload Quest
                </button>
                <button type="button" id="tabExam" class="mode-tab-btn" onclick="switchMode('EXAM')">
                    <svg class="ui-icon" viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                    EXAM Mode (Syllabus + PYQ + 10 CGPA Predictor)
                </button>
            </div>

            <!-- Auto-Resume Banner -->
            <asp:Panel ID="pnlResumeBanner" runat="server" Visible="false" CssClass="resume-banner">
                <div>
                    <strong style="color:#38bdf8;">Previous Study Session Restored!</strong>
                    <div style="font-size:12px; color:var(--text-sub); margin-top:2px;">Your active corpus, timer, and unanswered questions were auto-saved.</div>
                </div>
                <asp:Button ID="btnDiscardResume" runat="server" Text="Start Fresh" CssClass="btn-secondary" OnClick="btnResetSession_Click" OnClientClick="clearExamCache();" />
            </asp:Panel>

            <div class="glass-panel">
                <!-- 1. Original Classic Upload View -->
                <div id="pnlClassicUpload" class="tab-content-area">
                    <label>Upload Document / Code / Slides / Images (.PDF, .DOCX, .PPTX, Code, Text):</label>
                    <div class="file-box">
                        <asp:FileUpload ID="fileUpload" runat="server" AllowMultiple="true" />
                    </div>

                    <!-- Persistent Staged Files Shelf -->
                    <asp:UpdatePanel ID="upFileShelf" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Panel ID="pnlActiveFiles" runat="server" Visible="false">
                                <label>Currently Loaded Sources:</label>
                                <div class="file-list-shelf">
                                    <asp:Repeater ID="rptFiles" runat="server" OnItemCommand="rptFiles_ItemCommand">
                                        <ItemTemplate>
                                            <div class="file-chip">
                                                <svg class="ui-icon" style="width:14px; height:14px;" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline></svg>
                                                <%# Container.DataItem %>
                                                <asp:Button ID="btnRemoveFile" runat="server" Text="✕" CommandName="RemoveFile" CommandArgument='<%# Container.DataItem %>' CssClass="file-chip-btn" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>

                <!-- 2. EXAM Mode View (Clean Transparent Vectors & Smooth Fade) -->
                <div id="pnlExamBuckets" style="display:none;">
                    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:14px; flex-wrap:wrap; gap:10px;">
                        <div>
                            <span style="font-size:15px; font-weight:800; color:#38bdf8; display:flex; align-items:center; gap:7px;">
                                <svg class="ui-icon" style="width:18px; height:18px;" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 14 14"></polyline></svg>
                                Examination Intelligence & Multi-Corpus Ingestion
                            </span>
                            <div style="font-size:12px; color:var(--text-muted); margin-top:2px;">Cross-analyzes Syllabus, PYQ frequency trends, slides, and notes for 10 CGPA target.</div>
                        </div>
                        <div style="display:flex; align-items:center; gap:8px;">
                            <label style="margin:0;">Exam Duration (Minutes):</label>
                            <asp:TextBox ID="txtExamMinutes" runat="server" TextMode="Number" Text="60" min="1" max="360" style="width:75px; margin:0; padding:6px 10px; font-weight:700; text-align:center;"></asp:TextBox>
                        </div>
                    </div>

                    <div class="buckets-grid">
                        <div class="bucket-card">
                            <span class="bucket-title">
                                <svg class="ui-icon" style="width:14px; height:14px;" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline></svg>
                                1. Syllabus File
                            </span>
                            <asp:FileUpload ID="fuSyllabus" runat="server" />
                        </div>
                        <div class="bucket-card">
                            <span class="bucket-title">
                                <svg class="ui-icon" style="width:14px; height:14px;" viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
                                2. Past Papers (PYQ)
                            </span>
                            <asp:FileUpload ID="fuPYQ" runat="server" AllowMultiple="true" />
                        </div>
                        <div class="bucket-card">
                            <span class="bucket-title">
                                <svg class="ui-icon" style="width:14px; height:14px;" viewBox="0 0 24 24"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="8" y1="21" x2="16" y2="21"></line><line x1="12" y1="17" x2="12" y2="21"></line></svg>
                                3. Lecture PPTs
                            </span>
                            <asp:FileUpload ID="fuPPT" runat="server" AllowMultiple="true" />
                        </div>
                        <div class="bucket-card">
                            <span class="bucket-title">
                                <svg class="ui-icon" style="width:14px; height:14px;" viewBox="0 0 24 24"><polyline points="16 18 22 12 16 6"></polyline><polyline points="8 6 2 12 8 18"></polyline></svg>
                                4. Notes / Code Docs
                            </span>
                            <asp:FileUpload ID="fuNotes" runat="server" AllowMultiple="true" />
                        </div>
                    </div>
                </div>

                <label>Set Target Assessment Distribution:</label>
                <div class="matrix-grid">
                    <div class="matrix-box">
                        <label>MCQs (1 Mark)</label>
                        <asp:TextBox ID="txtCountMCQ" runat="server" TextMode="Number" Text="5" min="0" max="50"></asp:TextBox>
                    </div>
                    <div class="matrix-box">
                        <label>True / False (1 Mark)</label>
                        <asp:TextBox ID="txtCountTF" runat="server" TextMode="Number" Text="5" min="0" max="50"></asp:TextBox>
                    </div>
                    <div class="matrix-box">
                        <label>Short Conceptual (2 Marks)</label>
                        <asp:TextBox ID="txtCountShort" runat="server" TextMode="Number" Text="3" min="0" max="50"></asp:TextBox>
                    </div>
                    <div class="matrix-box">
                        <label>University Long (5 Marks)</label>
                        <asp:TextBox ID="txtCountLong" runat="server" TextMode="Number" Text="2" min="0" max="50"></asp:TextBox>
                    </div>
                </div>

                <label>Or Paste Raw Notes / Syllabus Text Here:</label>
                <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="3" Placeholder="Paste syllabus notes, documentation, or topic text here..."></asp:TextBox>

                <asp:Button ID="btnGenerate" runat="server" Text="PROCESS MATERIAL & START QUEST" CssClass="btn-primary" OnClick="btnGenerate_Click" />
                <asp:Label ID="lblStatus" runat="server" ForeColor="#38bdf8" Font-Size="13px" style="display:block; margin-top:10px; font-weight:600;"></asp:Label>
            </div>

            <!-- Quiz Practice & Prediction Arena -->
            <asp:UpdatePanel ID="upQuizArena" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnlArena" runat="server" Visible="false" CssClass="glass-panel">
                        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom: 18px; flex-wrap:wrap; gap:10px;">
                            <div style="font-size: 13px; font-weight: 600; color: #38bdf8;">
                                Active Corpus: <asp:Label ID="lblActiveSource" runat="server" Text="Multi-Document Corpus" ForeColor="#f1f5f9"></asp:Label>
                            </div>
                            <div class="action-bar">
                                <asp:Button ID="btnDownloadPdf" runat="server" Text="⬇ Download University Paper (PDF)" CssClass="btn-secondary btn-download" OnClick="btnDownloadPdf_Click" />
                                <asp:Button ID="btnCompleteTest" runat="server" Text="📊 Finish Exam" CssClass="btn-secondary" OnClick="btnCompleteTest_Click" OnClientClick="clearExamCache();" />
                                <asp:Button ID="btnShuffle" runat="server" Text="Reshuffle" CssClass="btn-secondary" OnClick="btnShuffle_Click" />
                                <asp:Button ID="btnResetSession" runat="server" Text="Clear & New" CssClass="btn-secondary" OnClick="btnResetSession_Click" OnClientClick="clearExamCache();" />
                            </div>
                        </div>

                        <!-- EXAM Mode Insights Heatmap -->
                        <asp:Panel ID="pnlPredictorInsights" runat="server" Visible="false" CssClass="predictor-box">
                            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:10px;">
                                <span style="font-size: 15px; font-weight: 800; color: #38bdf8;">Recurring Exam Concepts</span>
                                <span style="font-size:11px; color:#94a3b8; font-family:'JetBrains Mono';">Algorithm: Overlap Indexing</span>
                            </div>
                            <asp:Literal ID="litTrendingTopics" runat="server"></asp:Literal>
                        </asp:Panel>

                        <!-- Scorecard Summary Banner -->
                        <asp:Panel ID="pnlSummaryCard" runat="server" Visible="false" style="background: rgba(30, 41, 59, 0.85); border: 1px solid #38bdf8; border-radius: 14px; padding: 18px; margin-bottom: 20px;">
                            <div style="font-size: 16px; font-weight: 800; color: #38bdf8; margin-bottom: 8px;">Final Exam Diagnostic Scorecard</div>
                            <div style="font-size: 14px; line-height: 1.6;">
                                <asp:Literal ID="litSummaryDetails" runat="server"></asp:Literal>
                            </div>
                        </asp:Panel>

                        <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand">
                            <ItemTemplate>
                                <div class="question-card">
                                    <div class="q-meta">
                                        <div style="display:flex; align-items:center; gap:8px;">
                                            <span class="q-num-badge">Q.<%# Container.ItemIndex + 1 %></span>
                                            <span><%# Eval("QuestionType") %> • <%# Eval("Marks") %> MARKS</span>
                                        </div>
                                        <span class="predict-badge">
                                            <%# Eval("QuestionType").ToString() == "5Mark" ? "PYQ Repeat Probability (High)" : "High Exam Frequency" %>
                                        </span>
                                    </div>

                                    <%# FormatQuestionBody(Eval("QuestionText").ToString()) %>

                                    <asp:HiddenField ID="hfQuestionId" runat="server" Value='<%# Eval("QuestionId") %>' />
                                    <asp:HiddenField ID="hfCorrectAnswer" runat="server" Value='<%# Server.HtmlEncode(Eval("CorrectAnswer").ToString()) %>' />

                                    <!-- 4 MCQ Options -->
                                    <asp:Panel ID="pnlMCQ" runat="server" Visible='<%# Eval("QuestionType").ToString() == "MCQ" %>'>
                                        <div class="mcq-grid">
                                            <asp:Button ID="btnOptA" runat="server" Text='<%# "A) " + Eval("OptionA") %>' CommandName="VerifyAnswer" CommandArgument='<%# Eval("OptionA") %>' CssClass="btn-option" />
                                            <asp:Button ID="btnOptB" runat="server" Text='<%# "B) " + Eval("OptionB") %>' CommandName="VerifyAnswer" CommandArgument='<%# Eval("OptionB") %>' CssClass="btn-option" />
                                            <asp:Button ID="btnOptC" runat="server" Text='<%# "C) " + Eval("OptionC") %>' CommandName="VerifyAnswer" CommandArgument='<%# Eval("OptionC") %>' CssClass="btn-option" />
                                            <asp:Button ID="btnOptD" runat="server" Text='<%# "D) " + Eval("OptionD") %>' CommandName="VerifyAnswer" CommandArgument='<%# Eval("OptionD") %>' CssClass="btn-option" />
                                        </div>
                                    </asp:Panel>

                                    <!-- True / False -->
                                    <asp:Panel ID="pnlTF" runat="server" Visible='<%# Eval("QuestionType").ToString() == "TF" %>'>
                                        <div style="display: flex; gap: 12px; margin-top: 10px;">
                                            <asp:Button ID="btnTrue" runat="server" Text="True" CommandName="VerifyAnswer" CommandArgument="True" CssClass="btn-option" style="text-align:center; font-weight:700;" />
                                            <asp:Button ID="btnFalse" runat="server" Text="False" CommandName="VerifyAnswer" CommandArgument="False" CssClass="btn-option" style="text-align:center; font-weight:700;" />
                                        </div>
                                    </asp:Panel>

                                    <!-- Descriptive Questions -->
                                    <asp:Panel ID="pnlText" runat="server" Visible='<%# Eval("QuestionType").ToString() != "MCQ" && Eval("QuestionType").ToString() != "TF" %>'>
                                        <div style="display: flex; gap: 10px; margin-top: 10px;">
                                            <asp:TextBox ID="txtStudentAnswer" runat="server" Placeholder="Type answer explanation or key syntax here..."></asp:TextBox>
                                            <asp:Button ID="btnVerifyText" runat="server" Text="Evaluate" CommandName="CheckDescriptive" CssClass="btn-primary" style="width: 120px; padding: 10px;" />
                                        </div>
                                    </asp:Panel>

                                    <asp:Panel ID="pnlFeedback" runat="server" Visible="false">
                                        <div>
                                            <strong><asp:Literal ID="litFeedbackTitle" runat="server"></asp:Literal></strong>
                                        </div>
                                        <div style="margin-top: 4px; font-size: 13px; opacity: 0.9;">
                                            Exam Reference Solution: <%# Eval("CorrectAnswer") %>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:PostBackTrigger ControlID="btnDownloadPdf" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>