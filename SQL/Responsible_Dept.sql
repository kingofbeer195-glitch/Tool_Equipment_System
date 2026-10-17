USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[Responsible_Dept]    Script Date: 10/17/2026 5:52:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Responsible_Dept](
	[dept_code] [int] NULL,
	[pic_dept] [nvarchar](50) NULL,
	[dept_id] [int] NULL,
	[div_id] [int] NULL,
	[emp_id] [nvarchar](10) NULL,
	[dept_code_real] [int] NULL
) ON [PRIMARY]
GO
