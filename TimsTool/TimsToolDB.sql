USE [master]
GO
if exists(select * from master.sys.databases where trim(name)='TimsTool')
begin
	declare @dbName varchar(200)
	declare @sqlString varchar(1000)
	set @dbName = (select name from master.sys.databases where trim(name)='TimsTool')
	set @sqlString = 'alter database [' + @dbName + '] ' + 'set single_user with rollback immediate'
	execute(@sqlString)

	set @sqlString = 'drop database [' + @dbName + '] '
    execute(@sqlString)
end
/****** Object:  Database [TimsTool]    Script Date: 15/02/2023 13:07:35 ******/
CREATE DATABASE [TimsTool]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TimsTool', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TimsTool.mdf' , SIZE = 204800KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TimsTool_log', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TimsTool_log.ldf' , SIZE = 335872KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [TimsTool] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TimsTool].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TimsTool] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TimsTool] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TimsTool] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TimsTool] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TimsTool] SET ARITHABORT OFF 
GO
ALTER DATABASE [TimsTool] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TimsTool] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TimsTool] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TimsTool] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TimsTool] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TimsTool] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TimsTool] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TimsTool] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TimsTool] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TimsTool] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TimsTool] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TimsTool] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TimsTool] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TimsTool] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TimsTool] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TimsTool] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TimsTool] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TimsTool] SET RECOVERY FULL 
GO
ALTER DATABASE [TimsTool] SET  MULTI_USER 
GO
ALTER DATABASE [TimsTool] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TimsTool] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TimsTool] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TimsTool] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TimsTool] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TimsTool] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'TimsTool', N'ON'
GO
ALTER DATABASE [TimsTool] SET QUERY_STORE = OFF
GO
USE [TimsTool]
GO
/****** Object:  Table [dbo].[Comment]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comment](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[ParentUUID] [uniqueidentifier] NOT NULL,
	[ParentMasterUUID] [uniqueidentifier] NOT NULL,
	[Note] [varchar](max) NOT NULL,
	[SystemCommentType] [varchar](20) NULL,
	[PublicationTags] [varchar](500) NULL,
	[RowVersion] [timestamp] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FixedList]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FixedList](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_FixedList] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Now]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Now](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Now] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NowRequirement]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NowRequirement](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[ResultDefinitionUUID] [uniqueidentifier] NOT NULL,
	[NOWUUID] [uniqueidentifier] NOT NULL,
	[ParentNowRequirementUUID] [uniqueidentifier] NULL,
	[RootParentNowRequirementUUID] [uniqueidentifier] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_NowRequirement] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NowSubscription]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NowSubscription](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_NowSubscription] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultDefinition]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultDefinition](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[ResultDefinitionWordGroupIds] [varchar](500) NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultDefinition] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultDefinitionRule]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultDefinitionRule](
	[UUID] [uniqueidentifier] NOT NULL,
	[ParentUuid] [uniqueidentifier] NOT NULL,
	[ChildResultDefinitionUuid] [uniqueidentifier] NOT NULL,
	[Rule] [varchar](20) NULL,
	[RuleMags] [varchar](20) NULL,
	[RuleCrown] [varchar](20) NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultDefinitionRule] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultDefinitionWordGroup]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultDefinitionWordGroup](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultDefinitionWordGroup] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultPrompt]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultPrompt](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[ResultPromptWordGroupIds] [varchar](500) NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultPrompt] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultPromptRule]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultPromptRule](
	[UUID] [uniqueidentifier] NOT NULL,
	[ResultDefinitionUUID] [uniqueidentifier] NOT NULL,
	[ResultPromptUUID] [uniqueidentifier] NOT NULL,
	[Rule] [varchar](20) NULL,
	[RuleMags] [varchar](20) NULL,
	[RuleCrown] [varchar](20) NULL,
	[PromptSequence] [int] NULL,
	[UserGroups] [varchar](1000) NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultPromptRule] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResultPromptWordGroup]    Script Date: 15/02/2023 13:07:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultPromptWordGroup](
	[UUID] [uniqueidentifier] NOT NULL,
	[MasterUUID] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[CreatedUser] [varchar](500) NOT NULL,
	[LastModifiedUser] [varchar](500) NOT NULL,
	[DeletedUser] [varchar](500) NULL,
	[PublishedStatus] [varchar](20) NULL,
	[PublishedStatusDate] [datetime] NULL,
	[PublicationTags] [varchar](500) NULL,
	[Payload] [varchar](max) NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_ResultPromptWordGroup] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [TimsTool] SET  READ_WRITE 
GO
