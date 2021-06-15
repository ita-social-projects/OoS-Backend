--test1@test.com
--Qwerty1!

--test2@test.com
--Qwerty2@

--test3@test.com
--Qwerty3%

--test4@test.com
--Qwerty4$

--USE [OosBackend]
--GO

--==================== USERS ================================
INSERT INTO [dbo].[AspNetUsers]
           ([Id]
           ,[CreatingTime]
           ,[LastLogin]
           ,[LastName]
           ,[MiddleName]
           ,[FirstName]
           ,[Role]
           ,[UserName]
           ,[NormalizedUserName]
           ,[Email]
           ,[NormalizedEmail]
           ,[EmailConfirmed]
           ,[PasswordHash]
           ,[SecurityStamp]
           ,[ConcurrencyStamp]
           ,[PhoneNumber]
           ,[PhoneNumberConfirmed]
           ,[TwoFactorEnabled]
           ,[LockoutEnd]
           ,[LockoutEnabled]
           ,[AccessFailedCount]
           ,[IsRegistered])
     VALUES
           ('16575ce5-38e3-4ae7-b991-4508ed488369' --Id
           ,'2021-06-04 10:06:32.9282504'
           ,'0001-01-01 00:00:00.0000000'
           ,'Батькоперший' --last name
           ,'Іванович' --middle name
           ,'Іван' --first name
           ,'parent' --role
           ,'test1@test.com'
           ,'TEST1@TEST.COM'
           ,'test1@test.com'
           ,'TEST1@TEST.COM'
           ,0 --emeil confirmed
           ,'AQAAAAEAACcQAAAAELVU2FZw3HwShwuAXJR/xKFl938KgGpwdRRegrC5UFgZ5gnXdV6mEfalZCAngmX5sQ==' --password hash
           ,'5Z5EXGBVHXUYVKZOBCCG7QLPWI6NJ22O'
           ,'81dfc15c-1f36-48f6-99fd-9d028096cdec'
           ,'1234567890' --phone
           ,0 --phone confirmed
           ,0
           ,NULL
           ,1
           ,0
           ,0) --is registered

		   ,('7604a851-66db-4236-9271-1f037ffe3a81' --Id
           ,'2021-06-04 10:24:40.8990089'
           ,'0001-01-01 00:00:00.0000000'
           ,'Батькодругий' --last name
           ,'Петрович' --middle name
           ,'Петро' --first name
           ,'parent' --role
           ,'test2@test.com'
           ,'TEST2@TEST.COM'
           ,'test2@test.com'
           ,'TEST2@TEST.COM'
           ,0 --emeil confirmed
           ,'AQAAAAEAACcQAAAAEE6AlX8whARS9uZwJ5AUZx8490dgEnfrv7Q1lBXFqJwZcSoN6Mnvadhg75HG3ooT/A==' --password hash
           ,'PB7OZCNE7PMY4YDB3VE34U5K2TXWGTLP'
           ,'83915185-5bbd-4047-9901-638de8bd3a27'
           ,'4561237890' --phone
           ,0 --phone confirmed
           ,0
           ,NULL
           ,1
           ,0
           ,0) --is registered

		   ,('47802b21-2fb5-435e-9057-75c43d002cef' --Id
           ,'2021-06-04 10:29:56.7988521'
           ,'0001-01-01 00:00:00.0000000'
           ,'Провайдерперший' --last name
           ,'Семенович' --middle name
           ,'Семен' --first name
           ,'provider' --role
           ,'test3@test.com'
           ,'TEST3@TEST.COM'
           ,'test3@test.com'
           ,'TEST3@TEST.COM'
           ,0 --emeil confirmed
           ,'AQAAAAEAACcQAAAAELY6XF2g82E4EWQJl6UFWlojctlsLegV4f6qoME2IwwdI5fGaOq/Y6L6t+oa1N9j4Q==' --password hash
           ,'EC36E7KFYXF2YMCSOSD27UXQRFKPMDKK'
           ,'789c5891-212c-4339-95a2-80f75a168231'
           ,'7890123456' --phone
           ,0 --phone confirmed
           ,0
           ,NULL
           ,1
           ,0
           ,0) --is registered
		   
		   ,('5bff5f95-1848-4c87-9846-a567aeb407ea' --Id
           ,'2021-06-04 10:33:26.6295481'
           ,'0001-01-01 00:00:00.0000000'
           ,'Провайдердругий' --last name
           ,'Борисович' --middle name
           ,'Борис' --first name
           ,'provider' --role
           ,'test4@test.com'
           ,'TEST4@TEST.COM'
           ,'test4@test.com'
           ,'TEST4@TEST.COM'
           ,0 --emeil confirmed
           ,'AQAAAAEAACcQAAAAEOJFUEjnmnHWPonAOsg9K6tBuT8e1cUYbBejJbRJf3smSTzUzqphZyFGtB7i6vuT0g==' --password hash
           ,'NQTDJHP23OUXWSVOUSBDSUVBASPLCHV3'
           ,'b419d5b7-fe0f-40a8-869f-76b2826de58f'
           ,'0123456789' --phone
           ,0 --phone confirmed
           ,0
           ,NULL
           ,1
           ,0
           ,0) --is registered
GO

--Change roles' Ids according to your data in [AspNetRoles].
INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
     VALUES
           ('16575ce5-38e3-4ae7-b991-4508ed488369' --UserId (test1)
           ,'c2e1a3bd-bafd-4c94-bc63-d33791b6e3c8') --roleId (parent)

		   ,('7604a851-66db-4236-9271-1f037ffe3a81' --UserId (test2)
           ,'c2e1a3bd-bafd-4c94-bc63-d33791b6e3c8') --roleId (parent)

		   ,('47802b21-2fb5-435e-9057-75c43d002cef' --UserId (test3)
           ,'739f5d36-64da-42d9-a967-bec1695e0e61') --roleId (provider)

		   ,('5bff5f95-1848-4c87-9846-a567aeb407ea' --UserId (test4)
           ,'739f5d36-64da-42d9-a967-bec1695e0e61') --roleId (provider)
GO

--====================PARENTS AND CHILDREN================================
--Parents
INSERT INTO [dbo].[Parents]
           ([UserId])
     VALUES
           ('16575ce5-38e3-4ae7-b991-4508ed488369') --UserId (test1)

		   ,('7604a851-66db-4236-9271-1f037ffe3a81') --UserId (test2)
GO

--Social Groups (скіпнути, коли буде заповнятися програмою)
--INSERT INTO [dbo].[SocialGroups]
--           ([Name])
--     VALUES
--		   ('Діти із багатодітних сімей') 
--		   ,('Діти із малозабезпечених сімей')
--		   ,('Діти з інвалідністю')
--		   ,('Діти-сироти')
--		   ,('Діти, позбавлені батьківського піклування')
--GO

--Children

INSERT INTO [dbo].[Children]
           ([FirstName]
           ,[LastName]
           ,[MiddleName]
           ,[DateOfBirth]
           ,[Gender]
           ,[ParentId]
           ,[SocialGroupId])
     VALUES
           ('Тетяна'
           ,'Батькоперший'
           ,'Іванівна'
           ,'2010-12-11'
           ,1 --gender
           ,1 --parent Id (parent 1, user 1)
           ,null) --social group

		   ,('Богдан'
           ,'Батькодругий'
           ,'Петрович'
           ,'2010-05-05'
           ,0 --gender
           ,2 --parent Id (parent 2, user 2)
           ,2) --social group

		   ,('Лідія'
           ,'Батькодругий'
           ,'Петрівна'
           ,'2015-10-01'
           ,1 --gender
           ,2 --parent Id (parent 2, user 2)
           ,2) --social group
GO

--Children birth certificates
INSERT INTO [dbo].[BirthCertificates]
           ([Id]
           ,[SvidSer]
           ,[SvidNum]
           ,[SvidNumMD5]
           ,[SvidWho]
           ,[SvidDate])
     VALUES
           (1
           ,'І-ФП'
           ,'315315'
           ,null
           ,'Виконавчий комітет Дарницького району м. Києва'
           ,'2010-12-12')

		   ,(2
           ,'І-ФВ'
           ,'415415'
           ,null
           ,'Виконавчий комітет Деснянського району м. Києва'
           ,'2010-05-12')

		   ,(3
           ,'А-ФВ'
           ,'455485'
           ,null
           ,'Виконавчий комітет м.Житомиру'
           ,'2015-10-12')
GO

--==================== PROVIDERS AND WORKSHOPS ================================
--Categories
INSERT INTO Categories (Title, Description) VALUES ('Музика', 'Музика'), ('Танці', 'Танці'), ('Спорт', 'Спорт')
GO

INSERT INTO Subcategories (Title, Description, CategoryId) 
VALUES 
('Народних інструментів', 'Народних інструментів', 1),
('Духових та ударних інструментів', 'Духових та ударних інструментів', 1),
('Хореографічний', 'Хореографічний', 2),
('Олімпійські види спорту', 'Олімпійські види спорту', 3),
('Неолімпійські види спорту', 'Неолімпійські види спорту', 3)
GO

INSERT INTO SubSubcategories (Title, Description, SubcategoryId) 
VALUES 
('Бандура', 'Клас Бандури', 1),
('Акордеон', 'Клас Акордеону', 1),
('Ударні', 'Клас Ударних', 2),
('Флейта', 'Клас Флейти', 2),
('Бальні танці', 'Клас Бального танцю', 3),
('Сучасні танці', 'Клас Сучасного танцю', 3),
('Плавання', 'I.030. Плавання', 4),
('Футбол', 'I.050. Футбол', 4),
('Айкідо', 'II.004. Айкідо', 5),
('Альпінізм', 'II.007. Альпінізм', 5)
GO

--providers addresses, workshops
INSERT INTO [dbo].[Addresses]
           ([Region]
           ,[District]
           ,[City]
           ,[Street]
           ,[BuildingNumber]
           ,[Latitude]
           ,[Longitude])
     VALUES         
--providers
		   ('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'29'
           ,50.4547
           ,30.5238) --provider1 legal

		   ,('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'35'
           ,50.4547
           ,30.5238) --provider1 actual

		   ,('Житомирська обл.'
           ,'м. Житомир'
           ,'Житомир'
           ,'Вокзальна'
           ,'10'
           ,50.2648700
           ,28.6766900) --provider2 legal

		    ,('Житомирська обл.'
           ,'м. Житомир'
           ,'Житомир'
           ,'Привозна'
           ,'12А'
           ,50.2648700
           ,28.6766900) --provider2 actual

--workshops
		   ,('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'35'
           ,50.4547
           ,30.5238) --workshop1 provider1 actual

		   ,('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'35'
           ,50.4547
           ,30.5238) --workshop2 provider1 actual

		   ,('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'35'
           ,50.4547
           ,30.5238) --workshop3 provider1 actual

		   ,('Київська обл.'
           ,'м. Київ'
           ,'Київ'
           ,'Старонаводницька'
           ,'35'
           ,50.4547
           ,30.5238) --workshop4 provider1 actual

		   ,('Житомирська обл.'
           ,'м. Житомир'
           ,'Житомир'
           ,'Привозна'
           ,'12А'
           ,50.2648700
           ,28.6766900) --workshop5 provider2 actual

		   ,('Житомирська обл.'
           ,'м. Житомир'
           ,'Житомир'
           ,'Привозна'
           ,'12А'
           ,50.2648700
           ,28.6766900) --workshop6 provider2 actual
GO

--Providers
INSERT INTO [dbo].[Providers]
           ([FullTitle]
           ,[ShortTitle]
           ,[Website]
           ,[Email]
           ,[Facebook]
           ,[Instagram]
           ,[Description]
           ,[EdrpouIpn]
           ,[Director]
           ,[DirectorDateOfBirth]
           ,[PhoneNumber]
           ,[Founder]
           ,[Ownership]
           ,[Type]
           ,[Status]
           ,[LegalAddressId]
           ,[ActualAddressId]
           ,[UserId])
     VALUES
           ('Музична школа №1'
           ,'Музична школа'
           ,'http://provider1'
           ,'provider1@test.com'
           ,'http://facebook/provider1'
           ,'http://instagram/provider1'
           ,'Музикальні гуртки' --Description
           ,'12345678'
           ,'Провайдерперший Семен Семенович' --Director
           ,'2000-10-12'
           ,'0981234567'
           ,'Ващенко Володимир Богданович' --Founder
           ,0 --Ownership(state)
           ,4 --Type(EducationalInstitution)
           ,1 --Status
           ,1 --LegalAddressId
           ,2 --ActualAddressId
           ,'47802b21-2fb5-435e-9057-75c43d002cef') --User Id (test4)

		   ,('Школа бойових мистецтв №2'
           ,'ШБК №2'
           ,'http://provider2'
           ,'provider1@test.com'
           ,'http://facebook/provider2'
           ,'http://instagram/provider2'
           ,'Спортивні гуртки'
           ,'98764523'
           ,'Дорогий Захар Несторович'
           ,'1990-11-02'
           ,'0981234567'
           ,'Дорогий Захар Несторович'
           ,2 --Ownership(private)
           ,3 --Type(Private)
           ,1 --Status
           ,3 --LegalAddressId
           ,4 --ActualAddressId
           ,'5bff5f95-1848-4c87-9846-a567aeb407ea')
GO

--workshops
INSERT INTO [dbo].[Workshops]
           ([Title]
           ,[Phone]
           ,[Email]
           ,[Website]
           ,[Facebook]
           ,[Instagram]
           ,[MinAge]
           ,[MaxAge]
           ,[DaysPerWeek]
           ,[Price]
           ,[Description]
           ,[WithDisabilityOptions]
           ,[DisabilityOptionsDesc]
           ,[Logo]
           ,[Head]
           ,[HeadDateOfBirth]
           ,[IsPerMonth]
           ,[ProviderId]
           ,[AddressId]
		   ,[CategoryId]
		   ,[SubcategoryId]
           ,[SubsubcategoryId])
     VALUES
            ('Уроки аккордиону'
           ,'1234567890' --Phone
           ,'provider1@test.com'
           ,'http://provider1'
           ,'http://facebook/provider1'
           ,'http://instagram/provider1'
           ,5 --minAge
           ,100 --maxAge
           ,1 --days per week
           ,50 --price
           ,'Уроки аккордиону' --Description
           ,1 --WithDisabilityOptions
           ,'Немає конкретних обмежень' -- disability description
           ,'Logo'
           ,'Василенко Світлана Львівна' --Head
           ,'1987-09-22'
           ,1 --IsPerMonth
           ,1 --ProviderId
           ,5 --AddressId
		   ,1 --categoryId
		   ,2 --subcategoryId
           ,2) --SubsubcategoryId

           ,('Уроки бандури'
           ,'1234567890' --Phone
           ,'provider1@test.com'
           ,'http://provider1'
           ,'http://facebook/provider1'
           ,'http://instagram/provider1'
           ,5 --minAge
           ,100 --maxAge
           ,2 --days per week
           ,500 --price
           ,'Уроки бандури'
           ,1 --WithDisabilityOptions
           ,'Немає конкретних обмежень' -- disability description
           ,'Logo'
           ,'Денисенко Денис Денисович'
           ,'1987-09-22'
           ,1 --IsPerMonth
           ,1 --ProviderId
           ,6 --AddressId
		   ,1 --categoryId
		   ,1 --subcategoryId
           ,1) --SubsubcategoryId

		   ,('Гра на барабані'
           ,'1234567890' --Phone
           ,'provider1@test.com'
           ,'http://provider1'
           ,'http://facebook/provider1'
           ,'http://instagram/provider1'
           ,5 --minAge
           ,100 --maxAge
           ,2 --days per week
           ,500 --price
           ,'Уроки гри на ударних інструментах'
           ,1 --WithDisabilityOptions
           ,'Немає конкретних обмежень' -- disability description
           ,'Logo'
           ,'Гуляйборода Катерина Василівна'
           ,'1977-09-22'
           ,0 --IsPerMonth
           ,1 --ProviderId
           ,7 --AddressId
		   ,1 --categoryId
		   ,2 --subcategoryId
           ,3) --SubsubcategoryId

		   ,('Уроки гри на флейті'
           ,'1234567890' --Phone
           ,'provider1@test.com'
           ,'http://provider1'
           ,'http://facebook/provider1'
           ,'http://instagram/provider1'
           ,5 --minAge
           ,100 --maxAge
           ,2 --days per week
           ,0 --price
           ,'Уроки гри на флейті'
           ,1 --WithDisabilityOptions
           ,'Немає конкретних обмежень' -- disability description
           ,'Logo'
           ,'Гуляйборода Катерина Василівна'
           ,'1977-09-22'
           ,0 --IsPerMonth
           ,1 --ProviderId
           ,8 --AddressId
		   ,1 --categoryId
		   ,2 --subcategoryId
           ,4) --SubsubcategoryId

		   ,('Айкідо'
           ,'1234567890' --Phone
           ,'provider2@test.com'
           ,'http://provider2'
           ,'http://facebook/provider2'
           ,'http://instagram/provider2'
           ,7 --minAge
           ,50 --maxAge
           ,3 --days per week
           ,300 --price
           ,'Уроки айкідо'
           ,0 --WithDisabilityOptions
           ,null -- disability description
           ,'Логотип'
           ,'Дорогий Захар Несторович'
           ,'1984-09-02'
           ,1 --IsPerMonth
           ,2 --ProviderId
           ,9 --AddressId
		   ,3 --categoryId
		   ,5 --subcategoryId
           ,9) --SubsubcategoryId

		   ,('Плавання'
           ,'1234567890' --Phone
           ,'provider2@test.com'
           ,'http://provider2'
           ,'http://facebook/provider2'
           ,'http://instagram/provider2'
           ,3 --minAge
           ,100 --maxAge
           ,3 --days per week
           ,300 --price
           ,'Уроки плавання'
           ,1 --WithDisabilityOptions
           ,'будь-які' -- disability description
           ,'Логотип'
           ,'Рибочкін Леонід Федорович'
           ,'1995-09-06'
           ,1 --IsPerMonth
           ,2 --ProviderId
           ,10 --AddressId
		   ,3 --categoryId
		   ,4 --subcategoryId
           ,7) --SubsubcategoryId
GO

--teachers
INSERT INTO [dbo].[Teachers]
           ([FirstName]
           ,[LastName]
           ,[MiddleName]
           ,[DateOfBirth]
           ,[Description]
           ,[Image]
           ,[WorkshopId])
     VALUES
           ('Леонід' --firs name
           ,'Вчительперший' --last name
           ,'Леонідович' --middle name
           ,'1995-09-06'
           ,'Найкращий вчитель'
           ,null
           ,1) --workshop Id

		   ,('Наталія' --firs name
           ,'Вчительдругий' --last name
           ,'Богданівна' --middle name
           ,'1985-04-06'
           ,'Найкращий вчитель року'
           ,null
           ,2) --workshop Id

		   ,('Катерина' --firs name
           ,'Гуляйборода' --last name
           ,'Василівна' --middle name
           ,'1995-09-06'
           ,'Найкращий вчитель'
           ,null
           ,3) --workshop Id

		   ,('Георгій' --firs name
           ,'Вчительчетвертий' --last name
           ,'Ігорович' --middle name
           ,'1985-04-06'
           ,'Найкращий вчитель року'
           ,null
           ,4) --workshop Id

		   ,('Святослав' --firs name
           ,'Вчительтретій' --last name
           ,'Ігорович' --middle name
           ,'2000-04-06'
           ,'КМС з айкідо'
           ,null
           ,5) --workshop Id

		   ,('Денис' --firs name
           ,'Вчительчетвертий' --last name
           ,'Владиславович' --middle name
           ,'1998-04-06'
           ,'Чемпіон національних олімпіад з плавання'
           ,null
           ,6) --workshop Id
GO

--Applications
INSERT INTO [dbo].[Applications]
           ([Status]
           ,[WorkshopId]
           ,[ChildId]
           ,[UserId]
           ,[CreationTime])
     VALUES

--workshop1
           (1 --<Status, int,>
           ,1 --<WorkshopId, bigint,>
           ,1 --<ChildId, bigint,>
           ,'16575ce5-38e3-4ae7-b991-4508ed488369' --<UserId, nvarchar(450),> 1
           ,'2021-06-06') --<CreationTime>

		   ,(1 --<Status, int,>
           ,1 --<WorkshopId, bigint,>
           ,2 --<ChildId, bigint,>
           ,'7604a851-66db-4236-9271-1f037ffe3a81' --<UserId, nvarchar(450),> 2
           ,'2021-06-04') --<CreationTime>

		   ,(0 --<Status, int,>
           ,1 --<WorkshopId, bigint,>
           ,3 --<ChildId, bigint,>
           ,'7604a851-66db-4236-9271-1f037ffe3a81' --<UserId, nvarchar(450),> 2
           ,'2021-06-04') --<CreationTime>

--workshop2
		   ,(0 --<Status, int,>
           ,2 --<WorkshopId, bigint,>
           ,1 --<ChildId, bigint,>
           ,'16575ce5-38e3-4ae7-b991-4508ed488369' --<UserId, nvarchar(450),> 1
           ,'2021-06-15') --<CreationTime>

		   ,(0 --<Status, int,>
           ,2 --<WorkshopId, bigint,>
           ,2 --<ChildId, bigint,>
           ,'7604a851-66db-4236-9271-1f037ffe3a81' --<UserId, nvarchar(450),> 2
           ,'2021-06-15') --<CreationTime>

		   ,(0 --<Status, int,>
           ,2 --<WorkshopId, bigint,>
           ,3 --<ChildId, bigint,>
           ,'7604a851-66db-4236-9271-1f037ffe3a81' --<UserId, nvarchar(450),> 2
           ,'2021-06-15') --<CreationTime>
GO