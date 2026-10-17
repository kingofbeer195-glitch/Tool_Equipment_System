USE [tool_eduipment_system]
GO

/****** Object:  Table [dbo].[Withdraw_List]    Script Date: 10/17/2026 4:06:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Withdraw_List](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Doc_No] [nvarchar](15) NULL,
	[No] [int] NULL,
	[FixAsset] [nvarchar](15) NULL,
	[ControlNo] [nvarchar](20) NULL,
	[tool_id] [int] NULL,
	[sub_id] [int] NULL,
	[system_id] [int] NULL,
	[Use_Dept] [nvarchar](10) NULL,
	[Use_Model_Id] [int] NULL,
	[New_Model] [nvarchar](50) NULL,
	[HD_Model] [nvarchar](10) NULL,
	[Use_Cell_Id] [int] NULL,
	[New_Cell] [nvarchar](50) NULL,
	[HD_Cell] [nvarchar](10) NULL,
	[Use_Station_Id] [int] NULL,
	[New_Station] [nvarchar](50) NULL,
	[HD_Station] [nvarchar](10) NULL,
	[status_id] [int] NULL,
	[update_date] [datetime] NULL
) ON [PRIMARY]
GO


