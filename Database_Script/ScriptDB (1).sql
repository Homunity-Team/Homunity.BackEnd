-- إنشاء قاعدة البيانات
CREATE DATABASE Homunity;
GO

--  (Roles)جدول الأدوار
-- وصف: يحدد أنواع المستخدمين في النظام (مثلاً: مالك، طالب، مسؤول)
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(20) NOT NULL UNIQUE
);
GO

INSERT INTO Roles ([Name]) VALUES ('Admin');
INSERT INTO Roles ([Name]) VALUES ('Owner');
INSERT INTO Roles ([Name]) VALUES ('Student');



--  (Users)جدول المستخدمين
-- وصف: يخزن بيانات كل مستخدم (الاسم الكامل، رقم الهاتف، كلمة المرور المشفرة، الدور، والحالة النشطة)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(20) NOT NULL,
	LastName NVARCHAR(20) NOT NULL,
    Phone VARCHAR(20) NOT NULL UNIQUE,
    PasswordHash VARCHAR(300) NOT NULL,
    RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    IsActive BIT NOT NULL DEFAULT 1 
);
GO


--  (Location)جدول المواقع
--( وصف: يخزن تفاصيل المواقع للعقارات (المدينة، المنطقة، الشارع
CREATE TABLE Location (
    LocationId INT PRIMARY KEY IDENTITY(1,1),
    City NVARCHAR(20) NOT NULL,
    Area NVARCHAR(50) NOT NULL,
    Street NVARCHAR(50) NULL
);
GO


-- (PropertyStatus)جدول حالات العقارات 
-- وصف: يحدد حالات العقار بعد الإرسال (بإنتظار الموافقة، موافق، مرفوض)
CREATE TABLE PropertyStatus (
    PropertyStatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName VARCHAR(20) NOT NULL UNIQUE CHECK (StatusName IN ('Pending', 'Approved', 'Rejected'))
);
GO


--  (Properties)جدول العقارات
-- وصف: يخزن تفاصيل كل عقار (مالكه، العنوان، الوصف، السعر، عدد الغرف، النوع، الموقع، الحالة، سبب الرفض إن وجد)
CREATE TABLE Properties (
    PropertyId INT PRIMARY KEY IDENTITY(1,1),
    OwnerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(10,2) NOT NULL CHECK (Price > 0),
    Rooms INT NOT NULL CHECK (Rooms > 0),
    PropertyType VARCHAR(20) NOT NULL CHECK (PropertyType IN ('Room', 'Apartment')),
    LocationId INT NOT NULL FOREIGN KEY REFERENCES Location(LocationId),
    StatusId INT NOT NULL FOREIGN KEY REFERENCES PropertyStatus(PropertyStatusId),
    RejectReason NVARCHAR(300) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE() 
);
GO


--  (Services)جدول الخدمات
-- وصف: يحدد أنواع الخدمات المتوفرة في العقارات (مثلاً: إنترنت، مكيف، مطبخ)
CREATE TABLE Services (
    ServiceId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Icon NVARCHAR(100) NULL
);
GO


-- جدول وسيط (PropertyServices) -  جدول علاقة العقارات بالخدمات
-- وصف: جدول وسيط يربط بين العقارات والخدمات المتوفرة فيها
-- (لأن عقار يمكن أن يكون له عدة خدمات وخدمة واحدة في عقارات متعددة)
CREATE TABLE PropertyServices (
    PropertyServicesId INT PRIMARY KEY IDENTITY(1,1),
    PropertyId INT NOT NULL FOREIGN KEY REFERENCES Properties(PropertyId),
    ServiceId INT NOT NULL FOREIGN KEY REFERENCES Services(ServiceId),
    UNIQUE (PropertyId, ServiceId) 
);
GO


--  (PropertyImages)جدول صور العقارات
-- وصف: يخزن مسارات صور العقارات وتاريخ إضافتها
CREATE TABLE PropertyImages (
    ImageId INT PRIMARY KEY IDENTITY(1,1),
    PropertyId INT NOT NULL FOREIGN KEY REFERENCES Properties(PropertyId),
    ImagePath NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO


--  (PropertyVideo)جدول فيديوهات العقارات
-- وصف: يخزن مسارات فيديوهات العقارات وتاريخ إضافتها
CREATE TABLE PropertyVideo (
    VideoId INT PRIMARY KEY IDENTITY(1,1),
    PropertyId INT NOT NULL FOREIGN KEY REFERENCES Properties(PropertyId),
    VideoPath NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO


-- (BookingStatus)جدول حالات الحجوزات 
-- وصف: يحدد حالات الحجز (متاح، في المعالجة، محجوز)
CREATE TABLE BookingStatus (
    BookingStatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName VARCHAR(20) NOT NULL UNIQUE CHECK (StatusName IN ('Available', 'In-Process', 'Booked'))
);
GO


-- (Booking)جدول الحجوزات   
-- وصف: يخزن تفاصيل الحجوزات التي يقوم بها الطلاب على العقارات (العقار، الطالب، الحالة،
-- تاريخ الإنشاء، تاريخ التأكيد إن وجد)
CREATE TABLE Booking (
    BookingId INT PRIMARY KEY IDENTITY(1,1),
    PropertyId INT NOT NULL FOREIGN KEY REFERENCES Properties(PropertyId),
    StudentId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    StatusId INT NOT NULL FOREIGN KEY REFERENCES BookingStatus(BookingStatusId),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ConfirmedAt DATETIME NULL
);
GO


--  (AdminActions)جدول إجراءات المسؤول
-- وصف: يسجل إجراءات المسؤول على العقارات (مثلاً: الموافقة على العقار، الرفض) مع سببه وتاريخه
CREATE TABLE AdminActions (
    ActionId INT PRIMARY KEY IDENTITY(1,1),
    AdminId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    PropertyId INT NOT NULL FOREIGN KEY REFERENCES Properties(PropertyId),
    ActionType VARCHAR(20) NOT NULL,
    Reason NVARCHAR(300) NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO