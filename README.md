# ExamGenius: Smart Assessment & 10 CGPA Target Predictor Engine

Automated ASP.NET WebForms assessment system that ingests academic notes, generates dynamic practice questions, tracks student scores with live timers, and exports official university exam papers in PDF format.

---

## Academic Details
- Student Name: Asari MayurKumar Rameshbhai
- Roll Number: 2405101209004
- Course & Div: B.Sc. IT (Hons.) — Sem 5 Div J
- Subject: ASP.NET Tiny Project
- Faculty Guide: Prof. Divya Vasoya Mam
- University: Parul Institute of Computer Applications, Parul University

---

## System Requirements
- Visual Studio 2022 (ASP.NET and web development workload)
- .NET Framework 4.7.2
- Microsoft SQL Server (LocalDB or SQLEXPRESS)
- SQL Server Management Studio (SSMS)

---

## A to Z Setup & Execution Process

### Step 1: Database Setup
1. Open SQL Server Management Studio (SSMS).
2. Run this query to create the database and required tables:

CREATE DATABASE ExamGeniusDB;
GO
USE ExamGeniusDB;
GO

CREATE TABLE SyllabusUnits (
    UnitID INT IDENTITY(1,1) PRIMARY KEY,
    UnitTitle NVARCHAR(200),
    UnitContent NVARCHAR(MAX),
    UploadedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE GeneratedQuestions (
    QuestionID INT IDENTITY(1,1) PRIMARY KEY,
    UnitID INT NULL,
    QuestionType NVARCHAR(50),
    QuestionText NVARCHAR(MAX),
    OptionA NVARCHAR(MAX),
    OptionB NVARCHAR(MAX),
    OptionC NVARCHAR(MAX),
    OptionD NVARCHAR(MAX),
    CorrectAnswer NVARCHAR(MAX),
    Marks INT,
    UserResponse NVARCHAR(MAX),
    IsEvaluated BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE UserProgress (
    ProgressID INT IDENTITY(1,1) PRIMARY KEY,
    TotalXP INT DEFAULT 0,
    CurrentLevel INT DEFAULT 1,
    CurrentStreak INT DEFAULT 0,
    LastActive DATETIME DEFAULT GETDATE()
);
GO

(Note: The StudentSessionCache table is automatically created by the C# application at runtime.)

### Step 2: Connection String Verification
Check the connection string in Web.config matches your local SQL Server instance:
Data Source=.;Initial Catalog=ExamGeniusDB;Integrated Security=True

### Step 3: Open in Visual Studio & Restore Packages
1. Double-click ExamGenius.slnx (or .sln) to open the project in Visual Studio.
2. In Solution Explorer, right-click Solution 'ExamGenius' and click "Restore NuGet Packages" (installs iTextSharp automatically).
3. Press Ctrl + Shift + B to build the project.

### Step 4: Run the Application
1. Press F5 or Ctrl + F5 in Visual Studio.
2. The browser will open Default.aspx automatically.

---

## How to Use ExamGenius

1. Select Mode:
   - Upload Quest: Multi-file shelf for daily notes, slides, and cheat sheets.
   - EXAM Mode: 4 dedicated buckets (Syllabus, Past Papers, Slides, Notes) for university exam preparation.
2. Set Timer: Choose test duration (1 to 360 minutes).
3. Generate Arena: Click "Generate Assessment Arena" to formulate MCQs, True/False, and descriptive questions.
4. Practice & Score: Submit answers in real time to earn +50 XP per correct response and track streaks.
5. Download University Paper: Click "Download University Paper" to export an official watermarked PDF question paper.

---

## Key Features
- Dual operation modes (Upload Quest & 10 CGPA Target Predictor)
- Multi-format ingestion (.pdf, .docx, .pptx, text/code)
- Real-time question synthesis and instant answer evaluation
- Crash-proof session persistence (LocalStorage + SQL session recovery)
- Official watermarked university PDF export using iTextSharp
