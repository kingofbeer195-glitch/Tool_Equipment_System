USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[History_Scrap]    Script Date: 10/20/2026 10:28:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[History_Scrap](
	[History_id] [int] IDENTITY(1,1) NOT NULL,
	[Tool_id] [nvarchar](50) NOT NULL,
	[Tool_sub] [nvarchar](50) NOT NULL,
	[System_id] [nvarchar](10) NOT NULL,
	[Fix_Asset_No] [nvarchar](100) NULL,
	[Control_No] [nvarchar](100) NULL,
	[Tool_No] [nvarchar](200) NULL,
	[Old_Location1_id] [int] NULL,
	[Old_Location2_id] [int] NULL,
	[Old_Cell] [nvarchar](200) NULL,
	[Old_Station] [nvarchar](200) NULL,
	[Old_Status_id] [int] NULL,
	[New_Status_id] [int] NOT NULL,
	[Scrap_By] [nvarchar](50) NOT NULL,
	[Scrap_Date] [datetime] NOT NULL,
	[Remark] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[History_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[History_Scrap] ADD  DEFAULT ((12)) FOR [New_Status_id]
GO

ALTER TABLE [dbo].[History_Scrap] ADD  DEFAULT (getdate()) FOR [Scrap_Date]
GO


