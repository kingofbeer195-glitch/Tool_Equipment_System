USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[Withdraw_Control_List]    Script Date: 10/17/2026 4:07:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Withdraw_Control_List](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[issue_no] [int] NULL,
	[Doc_No] [nvarchar](15) NULL,
	[status_id] [int] NULL,
	[issue_empid] [nvarchar](10) NULL,
	[dept_code] [nvarchar](10) NULL,
	[reason_id] [int] NULL,
	[issue_date] [datetime] NULL,
	[update_date] [datetime] NULL,
	[send_date] [datetime] NULL,
	[confirm_empid] [nvarchar](10) NULL,
	[confirm_date] [datetime] NULL,
	[cancel_empid] [nvarchar](10) NULL,
	[cancel_date] [datetime] NULL,
	[reason_remark] [nvarchar](1000) NULL,
	[phone] [nvarchar](10) NULL,
	[dept_code_approve] [nvarchar](10) NULL
) ON [PRIMARY]
GO


