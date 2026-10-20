USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[History_Transfer]    Script Date: 10/20/2026 10:28:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[History_Transfer](
	[History_id] [int] IDENTITY(1,1) NOT NULL,
	[Tool_id] [varchar](50) NOT NULL,
	[Tool_sub] [varchar](50) NOT NULL,
	[System_id] [int] NOT NULL,
	[Fix_Asset_No] [varchar](50) NULL,
	[Control_No] [varchar](50) NULL,
	[Tool_No] [varchar](100) NULL,
	[Old_Location1_id] [int] NULL,
	[Old_Location2_id] [int] NULL,
	[Old_Cell] [varchar](100) NULL,
	[Old_Station] [varchar](100) NULL,
	[New_Location2_id] [int] NULL,
	[New_Section_Name] [varchar](100) NULL,
	[Transfer_By] [varchar](50) NOT NULL,
	[Transfer_Date] [datetime] NOT NULL,
	[Remark] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[History_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[History_Transfer] ADD  DEFAULT (getdate()) FOR [Transfer_Date]
GO


