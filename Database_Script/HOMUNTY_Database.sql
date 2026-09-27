/*
============================================================
HOMUNITY DATABASE - COMPLETE INITIALIZATION SCRIPT
============================================================

Purpose:
Create a fresh Homunity SQL Server database from scratch.

Source:
Current local Homunity database schema supplied by the project.

Includes:
- Database creation
- Tables and columns
- Primary keys
- Unique constraints
- Default constraints
- Foreign keys / relationships
- Check constraints
- Current local nonclustered indexes

Database name:
    Homunity

NOTE:
This script is intended for a NEW database.
It does not delete an existing database.
If Homunity already exists, the CREATE DATABASE step is skipped.

============================================================
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF DB_ID(N'Homunity') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [Homunity]');
END
GO

USE [Homunity]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[AdminActions]    Script Date: 9/23/2026 4:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminActions](
	[ActionId] [int] IDENTITY(1,1) NOT NULL,
	[AdminId] [int] NOT NULL,
	[PropertyId] [int] NOT NULL,
	[ActionType] [varchar](20) NOT NULL,
	[Reason] [nvarchar](300) NULL,
	[ActionDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ActionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Booking]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Booking](
	[BookingId] [int] IDENTITY(1,1) NOT NULL,
	[PropertyId] [int] NOT NULL,
	[StudentId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ConfirmedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[BookingStatus]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookingStatus](
	[BookingStatusId] [int] IDENTITY(1,1) NOT NULL,
	[StatusName] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[StatusName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ChatMessages]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatMessages](
	[MessageId] [int] IDENTITY(1,1) NOT NULL,
	[StudentId] [int] NOT NULL,
	[Role] [varchar](20) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Location]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Location](
	[LocationId] [int] IDENTITY(1,1) NOT NULL,
	[City] [nvarchar](20) NOT NULL,
	[Area] [nvarchar](50) NOT NULL,
	[Street] [nvarchar](50) NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
PRIMARY KEY CLUSTERED 
(
	[LocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Payments]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[BookingId] [int] NOT NULL,
	[StudentId] [int] NOT NULL,
	[OwnerId] [int] NOT NULL,
	[PropertyId] [int] NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[MockOrderId] [varchar](120) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[PaidAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Properties]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Properties](
	[PropertyId] [int] IDENTITY(1,1) NOT NULL,
	[OwnerId] [int] NOT NULL,
	[Title] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[Rooms] [int] NOT NULL,
	[PropertyType] [varchar](20) NOT NULL,
	[LocationId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[RejectReason] [nvarchar](300) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UniversityId] [int] NULL,
	[FullAddress] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[PropertyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PropertyImages]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyImages](
	[ImageId] [int] IDENTITY(1,1) NOT NULL,
	[PropertyId] [int] NOT NULL,
	[ImagePath] [nvarchar](200) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PropertyServices]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyServices](
	[PropertyServicesId] [int] IDENTITY(1,1) NOT NULL,
	[PropertyId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PropertyServicesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[PropertyId] ASC,
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PropertyStatus]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyStatus](
	[PropertyStatusId] [int] IDENTITY(1,1) NOT NULL,
	[StatusName] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PropertyStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[StatusName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PropertyVideo]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyVideo](
	[VideoId] [int] IDENTITY(1,1) NOT NULL,
	[PropertyId] [int] NOT NULL,
	[VideoPath] [nvarchar](200) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[VideoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Roles]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Services]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Services](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Icon] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Universities]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Universities](
	[UniversityId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UniversityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 9/23/2026 4:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](20) NOT NULL,
	[LastName] [nvarchar](20) NOT NULL,
	[Phone] [varchar](20) NOT NULL,
	[PasswordHash] [varchar](300) NOT NULL,
	[RoleId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Phone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AdminActions] ADD  DEFAULT (getdate()) FOR [ActionDate]
GO

ALTER TABLE [dbo].[Booking] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[ChatMessages] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Payments] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Properties] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[PropertyImages] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[PropertyVideo] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[AdminActions]  WITH NOCHECK ADD FOREIGN KEY([AdminId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[AdminActions]  WITH NOCHECK ADD FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO

ALTER TABLE [dbo].[Booking]  WITH NOCHECK ADD FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO

ALTER TABLE [dbo].[Booking]  WITH NOCHECK ADD FOREIGN KEY([StatusId])
REFERENCES [dbo].[BookingStatus] ([BookingStatusId])
GO

ALTER TABLE [dbo].[Booking]  WITH NOCHECK ADD FOREIGN KEY([StudentId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[ChatMessages]  WITH NOCHECK ADD FOREIGN KEY([StudentId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD  CONSTRAINT [FK_Payment_Booking] FOREIGN KEY([BookingId])
REFERENCES [dbo].[Booking] ([BookingId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payment_Booking]
GO

ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD  CONSTRAINT [FK_Payment_Owner] FOREIGN KEY([OwnerId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payment_Owner]
GO

ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD  CONSTRAINT [FK_Payment_Property] FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payment_Property]
GO

ALTER TABLE [dbo].[Payments]  WITH NOCHECK ADD  CONSTRAINT [FK_Payment_Student] FOREIGN KEY([StudentId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payment_Student]
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([LocationId])
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD FOREIGN KEY([OwnerId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD FOREIGN KEY([StatusId])
REFERENCES [dbo].[PropertyStatus] ([PropertyStatusId])
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD FOREIGN KEY([UniversityId])
REFERENCES [dbo].[Universities] ([UniversityId])
GO

ALTER TABLE [dbo].[PropertyImages]  WITH NOCHECK ADD FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO

ALTER TABLE [dbo].[PropertyServices]  WITH NOCHECK ADD FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO

ALTER TABLE [dbo].[PropertyServices]  WITH NOCHECK ADD FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Services] ([ServiceId])
GO

ALTER TABLE [dbo].[PropertyVideo]  WITH NOCHECK ADD FOREIGN KEY([PropertyId])
REFERENCES [dbo].[Properties] ([PropertyId])
GO

ALTER TABLE [dbo].[Users]  WITH NOCHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO

ALTER TABLE [dbo].[BookingStatus]  WITH NOCHECK ADD  CONSTRAINT [CK_BookingStatus_StatusName] CHECK  (([StatusName]='Confirmed' OR [StatusName]='In-Process' OR [StatusName]='Cancelled' OR [StatusName]='Booked' OR [StatusName]='Available'))
GO
ALTER TABLE [dbo].[BookingStatus] CHECK CONSTRAINT [CK_BookingStatus_StatusName]
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD CHECK  (([Price]>(0)))
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD CHECK  (([PropertyType]='Apartment' OR [PropertyType]='Room'))
GO

ALTER TABLE [dbo].[Properties]  WITH NOCHECK ADD CHECK  (([Rooms]>(0)))
GO

ALTER TABLE [dbo].[PropertyStatus]  WITH NOCHECK ADD CHECK  (([StatusName]='Rejected' OR [StatusName]='Approved' OR [StatusName]='Pending'))
GO


/*
============================================================
CURRENT LOCAL NONCLUSTERED INDEXES
============================================================
*/

CREATE NONCLUSTERED INDEX [IX_Booking_PropertyId_StatusId]
ON [dbo].[Booking]
(
    [PropertyId],
    [StatusId]
);
GO

CREATE NONCLUSTERED INDEX [IX_ChatMessages_StudentId]
ON [dbo].[ChatMessages]
(
    [StudentId]
);
GO

CREATE NONCLUSTERED INDEX [IX_Location_City_Area]
ON [dbo].[Location]
(
    [City],
    [Area]
);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_BookingId]
ON [dbo].[Payments]
(
    [BookingId]
);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_StudentId]
ON [dbo].[Payments]
(
    [StudentId]
);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_Order]
ON [dbo].[Payments]
(
    [MockOrderId]
);
GO

CREATE NONCLUSTERED INDEX [IX_Properties_StatusId]
ON [dbo].[Properties]
(
    [StatusId]
)
INCLUDE
(
    [Title],
    [Price],
    [Rooms],
    [PropertyType],
    [LocationId],
    [CreatedAt]
);
GO

CREATE NONCLUSTERED INDEX [IX_Properties_CreatedAt]
ON [dbo].[Properties]
(
    [CreatedAt]
);
GO

CREATE NONCLUSTERED INDEX [IX_Properties_OwnerId]
ON [dbo].[Properties]
(
    [OwnerId]
);
GO

/*
============================================================
END OF HOMUNITY DATABASE INITIALIZATION SCRIPT
============================================================
*/
