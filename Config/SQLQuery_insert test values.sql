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

ALTER DATABASE DB_NAME COLLATE Ukrainian_100_CI_AS;

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
        ,'2021-06-04 10:06:32.9282504 +00:00' --[CreatingTime]
        ,'0001-01-01 00:00:00.0000000 +00:00' --LastLogin
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
        ,1) --is registered

        ,('7604a851-66db-4236-9271-1f037ffe3a81' --Id
        ,'2021-06-04 10:24:40.8990089 +00:00' --[CreatingTime]
        ,'0001-01-01 00:00:00.0000000 +00:00' --LastLogin
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
        ,1) --is registered

        ,('47802b21-2fb5-435e-9057-75c43d002cef' --Id
        ,'2021-06-04 10:29:56.7988521 +00:00' --[CreatingTime]
        ,'0001-01-01 00:00:00.0000000 +00:00' --LastLogin
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
        ,1) --is registered

        ,('5bff5f95-1848-4c87-9846-a567aeb407ea' --Id
        ,'2021-06-04 10:33:26.6295481 +00:00' --[CreatingTime]
        ,'0001-01-01 00:00:00.0000000 +00:00' --LastLogin
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
        ,1) --is registered
    GO

--Roles' Ids according to your data in [AspNetRoles].
INSERT INTO [dbo].[AspNetUserRoles]
([UserId]
    ,[RoleId])
VALUES
    ('16575ce5-38e3-4ae7-b991-4508ed488369' --UserId (test1)
        ,(SELECT TOP (1) [Id] FROM [AspNetRoles] WHERE [Name] LIKE('parent'))) --roleId (parent)

        ,('7604a851-66db-4236-9271-1f037ffe3a81' --UserId (test2)
        ,(SELECT TOP (1) [Id] FROM [AspNetRoles] WHERE [Name] LIKE('parent'))) --roleId (parent)

        ,('47802b21-2fb5-435e-9057-75c43d002cef' --UserId (test3)
        ,(SELECT TOP (1) [Id] FROM [AspNetRoles] WHERE [Name] LIKE('provider'))) --roleId (provider)

        ,('5bff5f95-1848-4c87-9846-a567aeb407ea' --UserId (test4)
        ,(SELECT TOP (1) [Id] FROM [AspNetRoles] WHERE [Name] LIKE('provider'))) --roleId (provider)
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
    ,[SocialGroupId]
	,[PlaceOfStudy])
VALUES
    ('Тетяна'
        ,'Батькоперший'
        ,'Іванівна'
        ,'2010-12-11'
        ,1 --gender
        ,1 --parent Id (parent 1, user 1)
        ,null --social group
		,'Загальноосвітня школа №125') --[PlaceOfStudy]

        ,('Богдан'
        ,'Батькодругий'
        ,'Петрович'
        ,'2010-05-05'
        ,0 --gender
        ,2 --parent Id (parent 2, user 2)
        ,2 --social group
		,'ЗО №14') --[PlaceOfStudy]

        ,('Лідія'
        ,'Батькодругий'
        ,'Петрівна'
        ,'2015-10-01'
        ,1 --gender
        ,2 --parent Id (parent 2, user 2)
        ,2 --social group
		,'СШ №1') --[PlaceOfStudy]
    GO

--==================== PROVIDERS AND WORKSHOPS ================================
--Directions
INSERT INTO Directions (Title, Description) VALUES ('Музика', 'Музика'), ('Танці', 'Танці'), ('Спорт', 'Спорт')
    GO

INSERT INTO Departments (Title, Description, DirectionId)
VALUES
    ('Народних інструментів', 'Народних інструментів', 1),
    ('Духових та ударних інструментів', 'Духових та ударних інструментів', 1),
    ('Хореографічний', 'Хореографічний', 2),
    ('Олімпійські види спорту', 'Олімпійські види спорту', 3),
    ('Неолімпійські види спорту', 'Неолімпійські види спорту', 3)
    GO

INSERT INTO Classes (Title, Description, DepartmentId)
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

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) --id 11

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) --id 12

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) --id 13

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) --id 14

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) --id 15


        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) --id 16

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) --id 17

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) --id 18

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) --id 19

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) --id 20

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 21

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 22

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 23

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 24

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 25

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) --id 26

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Бойченка'
        ,'42'
        ,50.265
        ,28.677) --id 27

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Бойченка'
        ,'42'
        ,50.265
        ,28.677) --id 28

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Вокарчука'
        ,'42'
        ,50.5
        ,28.9) --id 29

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Вокарчука'
        ,'42'
        ,50.5
        ,28.9) --id 30
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
    ,[Keywords]
    ,[Phone]
    ,[Email]
    ,[Website]
    ,[Facebook]
    ,[Instagram]
    ,[MinAge]
    ,[MaxAge]
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
    ,[DirectionId]
    ,[DepartmentId]
    ,[ClassId]
    ,[ProviderTitle])
VALUES
    ('Уроки аккордиону'
        ,null
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,100 --maxAge
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
        ,1 --directionId
        ,2 --departmentId
        ,2 --classId
        ,'Музична школа №1') --provider title

        ,('Уроки бандури'
        ,null
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,100 --maxAge
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
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Гра на барабані'
        ,null
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,100 --maxAge
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
        ,1 --directionId
        ,2 --departmentId
        ,3 --classId
        ,'Музична школа №1') --provider title

        ,('Уроки гри на флейті'
        ,null
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,100 --maxAge
        ,100 --price
        ,'Уроки гри на флейті'
        ,1 --WithDisabilityOptions
        ,'Немає конкретних обмежень' -- disability description
        ,'Logo'
        ,'Гуляйборода Катерина Василівна'
        ,'1977-09-22'
        ,0 --IsPerMonth
        ,1 --ProviderId
        ,8 --AddressId
        ,1 --directionId
        ,2 --departmentId
        ,4 --classId
        ,'Музична школа №1') --provider title

        ,('Айкідо'
        ,null
        ,'1234567890' --Phone
        ,'provider2@test.com'
        ,'http://provider2'
        ,'http://facebook/provider2'
        ,'http://instagram/provider2'
        ,7 --minAge
        ,50 --maxAge
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
        ,3 --directionId
        ,5 --departmentId
        ,9 --classId
        ,'Школа бойових мистецтв №2') --provider title

        ,('Плавання'
        ,null
        ,'1234567890' --Phone
        ,'provider2@test.com'
        ,'http://provider2'
        ,'http://facebook/provider2'
        ,'http://instagram/provider2'
        ,3 --minAge
        ,100 --maxAge
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
        ,3 --directionId
        ,4 --departmentId
        ,7 --classId
        ,'Школа бойових мистецтв №2') --provider title

        ,('Співочий аккордион дошкільнят'
        ,'аккордион¤співи'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,0 --minAge
        ,5 --maxAge
        ,0 --price
        ,'Аккордион і співи' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,11 --AddressId
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Співочий аккордион юніорів'
        ,'аккордион¤співи'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 --minAge
        ,10 --maxAge
        ,0 --price
        ,'Аккордион і співи' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,12 --AddressId
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Співочий аккордион тінейджерів'
        ,'аккордион¤співи'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,11 --minAge
        ,16 --maxAge
        ,0 --price
        ,'Аккордион і співи' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,13 --AddressId
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Струни душі'
        ,'бандура'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 --minAge
        ,10 --maxAge
        ,360 --price
        ,'Гра на бандурі' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,14 --AddressId
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Балалайка для малят'
        ,'бандура'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,0 --minAge
        ,5 --maxAge
        ,0 --price
        ,'Гра на бандурі' --Description
        ,240 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,15 --AddressId
        ,1 --directionId
        ,1 --departmentId
        ,1 --classId
        ,'Музична школа №1') --provider title

        ,('Тендітні носочки'
        ,'танці¤бальні¤класичні'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,0 --minAge
        ,5 --maxAge
        ,240 --price
        ,'Бальні танці' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,16 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,5 --classId
        ,'Музична школа №1') --provider title

        ,('Прима-балерина'
        ,'танці¤бальні¤класичні'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 --minAge
        ,18 --maxAge
        ,780 --price
        ,'Бальні танці' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,17 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,5 --classId
        ,'Музична школа №1') --provider title

        ,('Танці вулиць'
        ,'танці¤вуличні¤сучасні'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 --minAge
        ,8 --maxAge
        ,640 --price
        ,'Ми навчимо вашу дитину танцювати енергійно. STREET DANCE.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,18 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Хіп-хоп'
        ,'танці¤сучасні'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 --minAge
        ,12 --maxAge
        ,740 --price
        ,'HIP-HOP і тільки.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,19 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Електрік-бугі'
        ,'танці¤сучасні¤вугі'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 --minAge
        ,12 --maxAge
        ,740 --price
        ,'ELECTRIC BOOGIE це весело і корисно.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,20 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Диско денс'
        ,'диско'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 --minAge
        ,12 --maxAge
        ,490 --price
        ,'Танці диско - це ярко і сміливо.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,21 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Степ'
        ,'степ¤сучасно¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 --minAge
        ,12 --maxAge
        ,355 --price
        ,'Просто приходь, просто танцюй.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,22 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Диско як стан душі'
        ,'диско¤дорослі¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,18 --minAge
        ,100 --maxAge
        ,278 --price
        ,'Просто приходь, просто танцюй.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,23 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Шаффл-денс'
        ,'шаффл¤дорослі¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,18 --minAge
        ,40 --maxAge
        ,600 --price
        ,'Відкриваємо світ енергійноо, незвичного танцю шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,24 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Шаффл'
        ,'шаффл¤дорослі¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 --minAge
        ,10 --maxAge
        ,300 --price
        ,'Відкриваємо світ енергійноо, незвичного танцю шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,25 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Уроки гри на флейті'
        ,null
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,10 --maxAge
        ,90 --price
        ,'Уроки гри на флейті'
        ,1 --WithDisabilityOptions
        ,'Немає конкретних обмежень' -- disability description
        ,'Logo'
        ,'Гуляйборода Катерина Василівна'
        ,'1977-09-22'
        ,0 --IsPerMonth
        ,1 --ProviderId
        ,26 --AddressId
        ,1 --categoryId
        ,2 --subcategoryId
        ,4 --SubsubcategoryId
        ,'Музична школа №1') --provider title

        ,('Шаффл діти'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 --minAge
        ,8 --maxAge
        ,200 --price
        ,'Крутий танок шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,27 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Шаффл школярі'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 --minAge
        ,12 --maxAge
        ,250 --price
        ,'Крутий танок шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,28 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Шаффл підлітки'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,13 --minAge
        ,18 --maxAge
        ,350 --price
        ,'Крутий танок шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,29 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title

        ,('Шаффл дорослі'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' --Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,19 --minAge
        ,100 --maxAge
        ,550 --price
        ,'Крутий танок шаффл.' --Description
        ,0 --WithDisabilityOptions
        ,null -- disability description
        ,null --'Logo'
        ,'Василенко Світлана Львівна' --Head
        ,'1987-09-22'
        ,1 --IsPerMonth
        ,1 --ProviderId
        ,30 --AddressId
        ,2 --directionId
        ,2 --departmentId
        ,6 --classId
        ,'Музична школа №1') --provider title
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

--Schedule
INSERT INTO [dbo].[DateTimeRanges]
           ([StartTime]
           ,[EndTime]
           ,[WorkshopId]
           ,[Workdays])
     VALUES
           ('8:00' --<StartTime>
           ,'17:30' --<EndTime>
           ,1 --<WorkshopId>
           ,21) --<Workdays>

		   ,('8:00' --<StartTime>
           ,'20:00' --<EndTime>
           ,2 --<WorkshopId>
           ,255) --<Workdays>

		   ,('8:00' --<StartTime>
           ,'20:00' --<EndTime>
           ,3 --<WorkshopId>
           ,21) --<Workdays>

		   ,('9:00' --<StartTime>
           ,'21:00' --<EndTime>
           ,3 --<WorkshopId>
           ,10) --<Workdays>

		   ,('12:00' --<StartTime>
           ,'19:00' --<EndTime>
           ,4 --<WorkshopId>
           ,21) --<Workdays>

		   ,('9:00' --<StartTime>
           ,'21:00' --<EndTime>
           ,4 --<WorkshopId>
           ,10) --<Workdays>

		   ,('9:00' --<StartTime>
           ,'21:00' --<EndTime>
           ,5 --<WorkshopId>
           ,96) --<Workdays>

		   ,('9:00' --<StartTime>
           ,'21:00' --<EndTime>
           ,6 --<WorkshopId>
           ,96) --<Workdays>
GO

--Applications
INSERT INTO [dbo].[Applications]
([Status]
    ,[WorkshopId]
    ,[ChildId]
    ,[CreationTime]
    ,[ParentId])
VALUES

--workshop1
    (2 --<Status, int,>
        ,1 --<WorkshopId, bigint,>
        ,1 --<ChildId, bigint,>
        ,'2021-06-06 12:20:20' --<CreationTime>
        ,1) --parentId

        ,(2 --<Status, int,>
        ,1 --<WorkshopId, bigint,>
        ,2 --<ChildId, bigint,>
        ,'2021-06-04 15:34:20' --<CreationTime>
        ,2) --parentId

        ,(1 --<Status, int,>
        ,1 --<WorkshopId, bigint,>
        ,3 --<ChildId, bigint,>
        ,'2021-06-05 08:23:20' --<CreationTime>
        ,2) --parentId

--workshop2
        ,(1 --<Status, int,>
        ,2 --<WorkshopId, bigint,>
        ,1 --<ChildId, bigint,>
        ,'2021-06-15 17:20:00' --<CreationTime>
        ,1) --parentId

        ,(1 --<Status, int,>
        ,2 --<WorkshopId, bigint,>
        ,2 --<ChildId, bigint,>
        ,'2021-06-15 17:26:10' --<CreationTime>
        ,2) --parentId

        ,(1 --<Status, int,>
        ,2 --<WorkshopId, bigint,>
        ,3 --<ChildId, bigint,>
        ,'2021-06-15 18:00:45' --<CreationTime>
        ,2) --parentId
    GO

--Rating
INSERT INTO [dbo].[Ratings]
([Rate]
    ,[Type]
    ,[EntityId]
    ,[ParentId]
    ,[CreationTime])
VALUES
--parent 1
    (5 --rating
        ,2 --workshop
        ,1 -- workshopId
        ,1 --parent
        ,'2021-01-15 18:00:45')

        ,(5 --rating
        ,2 --workshop
        ,2 -- workshopId
        ,1 --parent
        ,'2021-02-15 18:00:45')

--parent 2
        ,(5 --rating
        ,2 --workshop
        ,1 -- workshopId
        ,2 --parent
        ,'2021-01-02 18:00:45')

        ,(5 --rating
        ,2 --workshop
        ,2 -- workshopId
        ,2 --parent
        ,'2021-02-02 18:00:45')
    GO