USE [tool_eduipment_system]
GO

/****** Withdraw_List : เก็บ Status / ผู้รับผิดชอบ (เดิม → ใหม่) ตอน AAD Confirm (withdraw2.aspx) ******/
/*  Old_* = snapshot จาก Tool_Equipment_List_View ตอนกดยืนยัน
    New_* = ค่าที่ AAD เลือกใน Modal
    Responsible เก็บเป็น emp_id (ตรงกับ rsp_empID ของอุปกรณ์ / Responsible_Dept.emp_id)  */

IF COL_LENGTH('dbo.Withdraw_List', 'Old_Status_Id') IS NULL
    ALTER TABLE [dbo].[Withdraw_List] ADD [Old_Status_Id] [int] NULL;
GO
IF COL_LENGTH('dbo.Withdraw_List', 'New_Status_Id') IS NULL
    ALTER TABLE [dbo].[Withdraw_List] ADD [New_Status_Id] [int] NULL;
GO
IF COL_LENGTH('dbo.Withdraw_List', 'Old_Responsible') IS NULL
    ALTER TABLE [dbo].[Withdraw_List] ADD [Old_Responsible] [nvarchar](50) NULL;
GO
IF COL_LENGTH('dbo.Withdraw_List', 'New_Responsible') IS NULL
    ALTER TABLE [dbo].[Withdraw_List] ADD [New_Responsible] [nvarchar](50) NULL;
GO
