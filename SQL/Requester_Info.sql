USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[Requester_Info]    Script Date: 10/17/2026 5:52:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Requester_Info](
	[Doc_No] [nvarchar](30) NOT NULL,
	[Doc_Type] [nvarchar](10) NOT NULL,
	[emp_id] [nvarchar](10) NOT NULL,
	[emp_name] [nvarchar](150) NULL,
	[position_name] [nvarchar](150) NULL,
	[band] [nvarchar](20) NULL,
	[dept_code] [nvarchar](10) NULL,
	[dept_name] [nvarchar](100) NULL,
	[email] [nvarchar](150) NULL,
	[tel] [nvarchar](50) NULL,
	[source] [nvarchar](20) NOT NULL,
	[create_date] [datetime] NOT NULL,
	[update_date] [datetime] NULL,
 CONSTRAINT [PK_Requester_Info] PRIMARY KEY CLUSTERED 
(
	[Doc_No] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Requester_Info] ADD  CONSTRAINT [DF_Requester_Info_source]  DEFAULT ('HRMS') FOR [source]
GO

ALTER TABLE [dbo].[Requester_Info] ADD  CONSTRAINT [DF_Requester_Info_create]  DEFAULT (getdate()) FOR [create_date]
GO


