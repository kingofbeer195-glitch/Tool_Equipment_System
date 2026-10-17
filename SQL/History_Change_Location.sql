USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[History_Change_Location]    Script Date: 10/17/2026 3:53:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[History_Change_Location](
	[History_id] [int] IDENTITY(1,1) NOT NULL,
	[Tool_id] [nvarchar](50) NOT NULL,
	[Tool_sub] [nvarchar](50) NOT NULL,
	[System_id] [nvarchar](10) NOT NULL,
	[Fix_Asset_No] [nvarchar](100) NULL,
	[Control_No] [nvarchar](100) NULL,
	[Tool_No] [nvarchar](200) NULL,
	[Old_DeptCode] [nvarchar](50) NULL,
	[New_DeptCode] [nvarchar](50) NULL,
	[Old_Responsible] [nvarchar](100) NULL,
	[New_Responsible] [nvarchar](100) NULL,
	[Old_Model_Id] [nvarchar](50) NULL,
	[Old_Model_Name] [nvarchar](200) NULL,
	[New_Model_Id] [nvarchar](50) NULL,
	[New_Model_Text] [nvarchar](200) NULL,
	[HD_Model] [nvarchar](10) NULL,
	[Old_Cell_Id] [int] NULL,
	[Old_Cell_Name] [nvarchar](200) NULL,
	[New_Cell_Id] [int] NULL,
	[New_Cell_Text] [nvarchar](200) NULL,
	[HD_Cell] [nvarchar](10) NULL,
	[Old_Station_Id] [int] NULL,
	[Old_Station_Name] [nvarchar](200) NULL,
	[New_Station_Id] [int] NULL,
	[New_Station_Text] [nvarchar](200) NULL,
	[HD_Station] [nvarchar](10) NULL,
	[Old_Location1_id] [int] NULL,
	[Edit_By] [nvarchar](50) NOT NULL,
	[Edit_Date] [datetime] NOT NULL,
	[Remark] [nvarchar](500) NULL,
	[Old_Status_Id] [int] NULL,
	[New_Status_Id] [int] NULL,
	[Old_DueDate] [datetime] NULL,
	[New_DueDate] [datetime] NULL,
	[Old_Remark] [nvarchar](250) NULL,
	[New_Remark] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[History_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[History_Change_Location] ADD  DEFAULT (getdate()) FOR [Edit_Date]
GO


