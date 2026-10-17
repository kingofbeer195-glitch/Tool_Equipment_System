USE [tool_eduipment_system]
GO

/****** History_Change_Location : เพิ่ม Doc_No (เลขเอกสารที่ทำให้เกิดการเปลี่ยน Location) ******/
/*  NULL       = แก้ไขโดยตรงจากหน้า Edit Location
    AAD3-xx-.. = เบิกอุปกรณ์ (withdraw2 : บันทึกตอน AAD Confirm → Wait Update)
    ประเภทเอกสาร = LEFT(Doc_No, 4) → Master_Document.document_id                       */

IF COL_LENGTH('dbo.History_Change_Location', 'Doc_No') IS NULL
    ALTER TABLE [dbo].[History_Change_Location] ADD [Doc_No] [nvarchar](30) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_History_Change_Location_Doc_No')
    CREATE INDEX [IX_History_Change_Location_Doc_No] ON [dbo].[History_Change_Location]([Doc_No]);
GO
