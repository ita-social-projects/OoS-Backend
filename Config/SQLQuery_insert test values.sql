#test1@test.com
#Qwerty1!

#test2@test.com
#Qwerty2@

#test3@test.com
#Qwerty3%

#test4@test.com
#Qwerty4$

#USE OosBackend
#

#ALTER DATABASE DB_NAME COLLATE Ukrainian_100_CI_AS;
USE OutOfSchoolDb;
#==================== USERS ================================
INSERT INTO AspNetUsers
(Id
    ,CreatingTime
    ,LastLogin
    ,LastName
    ,MiddleName
    ,FirstName
    ,Role
    ,UserName
    ,NormalizedUserName
    ,Email
    ,NormalizedEmail
    ,EmailConfirmed
    ,PasswordHash
    ,SecurityStamp
    ,ConcurrencyStamp
    ,PhoneNumber
    ,PhoneNumberConfirmed
    ,TwoFactorEnabled
    ,LockoutEnd
    ,LockoutEnabled
    ,AccessFailedCount
    ,IsRegistered)
VALUES
	('fe78ed30-7df9-412b-8f1c-a21f3175334f', #Id
	NOW(), #CreatingTime
	'0001-01-01 00:00:00.0000000', #LastLogin
	'Адмін', #Last name
	'Адмін', #Middle name
	'Адмін', #First name
	'admin', #Role
	'admin@adm.com', #UserName
	'ADMIN@ADM.COM', #NormalizedUserName
	'admin@adm.com', #Email
	'ADMIN@ADM.COM', #NormalizedEmail
	0, #EmailConfirmed
	'AQAAAAEAACcQAAAAEB+AG8yyfG66fAkFIJ1mMdDx5ChM/wxoUHIwBuJt4cuO+jDCrPMw7D9BLYRtkvbrpg==', #PasswordHash  ->   Kf%ms3nAp7%aH
	'CALDXWOCPDFBODKYAKGNLA4LH36H7HAC', #SecurityStamp
	'9a8f388b-c168-4ea1-ad5a-ae3245d7bd4f', #ConcurrencyStamp
	'123456789', #PhoneNumber without +380
	0, #PhoneNumberConfirmed
	0, #TwoFactorEnabled
	NULL, #LockoutEnd
	1, #LockoutEnabled
	0, #AccessFailedCount
	1), #IsRegistered

    ('16575ce5-38e3-4ae7-b991-4508ed488369' #Id
        ,'2021-06-04 10:06:32.9282504 ' #CreatingTime
        ,'0001-01-01 00:00:00.0000000 ' #LastLogin
        ,'Батькоперший' #last name
        ,'Іванович' #middle name
        ,'Іван' #first name
        ,'parent' #role
        ,'test1@test.com'
        ,'TEST1@TEST.COM'
        ,'test1@test.com'
        ,'TEST1@TEST.COM'
        ,0 #emeil confirmed
        ,'AQAAAAEAACcQAAAAELVU2FZw3HwShwuAXJR/xKFl938KgGpwdRRegrC5UFgZ5gnXdV6mEfalZCAngmX5sQ==' #password hash
        ,'5Z5EXGBVHXUYVKZOBCCG7QLPWI6NJ22O'
        ,'81dfc15c-1f36-48f6-99fd-9d028096cdec'
        ,'1234567890' #phone
        ,0 #phone confirmed
        ,0
        ,NULL
        ,1
        ,0
        ,1) #is registered

        ,('7604a851-66db-4236-9271-1f037ffe3a81' #Id
        ,'2021-06-04 10:24:40.8990089 ' #CreatingTime
        ,'0001-01-01 00:00:00.0000000 ' #LastLogin
        ,'Батькодругий' #last name
        ,'Петрович' #middle name
        ,'Петро' #first name
        ,'parent' #role
        ,'test2@test.com'
        ,'TEST2@TEST.COM'
        ,'test2@test.com'
        ,'TEST2@TEST.COM'
        ,0 #emeil confirmed
        ,'AQAAAAEAACcQAAAAEE6AlX8whARS9uZwJ5AUZx8490dgEnfrv7Q1lBXFqJwZcSoN6Mnvadhg75HG3ooT/A==' #password hash
        ,'PB7OZCNE7PMY4YDB3VE34U5K2TXWGTLP'
        ,'83915185-5bbd-4047-9901-638de8bd3a27'
        ,'4561237890' #phone
        ,0 #phone confirmed
        ,0
        ,NULL
        ,1
        ,0
        ,1) #is registered

        ,('47802b21-2fb5-435e-9057-75c43d002cef' #Id
        ,'2021-06-04 10:29:56.7988521 ' #CreatingTime
        ,'0001-01-01 00:00:00.0000000 ' #LastLogin
        ,'Провайдерперший' #last name
        ,'Семенович' #middle name
        ,'Семен' #first name
        ,'provider' #role
        ,'test3@test.com'
        ,'TEST3@TEST.COM'
        ,'test3@test.com'
        ,'TEST3@TEST.COM'
        ,0 #emeil confirmed
        ,'AQAAAAEAACcQAAAAELY6XF2g82E4EWQJl6UFWlojctlsLegV4f6qoME2IwwdI5fGaOq/Y6L6t+oa1N9j4Q==' #password hash
        ,'EC36E7KFYXF2YMCSOSD27UXQRFKPMDKK'
        ,'789c5891-212c-4339-95a2-80f75a168231'
        ,'7890123456' #phone
        ,0 #phone confirmed
        ,0
        ,NULL
        ,1
        ,0
        ,1) #is registered

        ,('5bff5f95-1848-4c87-9846-a567aeb407ea' #Id
        ,'2021-06-04 10:33:26.6295481 ' #CreatingTime
        ,'0001-01-01 00:00:00.0000000 ' #LastLogin
        ,'Провайдердругий' #last name
        ,'Борисович' #middle name
        ,'Борис' #first name
        ,'provider' #role
        ,'test4@test.com'
        ,'TEST4@TEST.COM'
        ,'test4@test.com'
        ,'TEST4@TEST.COM'
        ,0 #emeil confirmed
        ,'AQAAAAEAACcQAAAAEOJFUEjnmnHWPonAOsg9K6tBuT8e1cUYbBejJbRJf3smSTzUzqphZyFGtB7i6vuT0g==' #password hash
        ,'NQTDJHP23OUXWSVOUSBDSUVBASPLCHV3'
        ,'b419d5b7-fe0f-40a8-869f-76b2826de58f'
        ,'0123456789' #phone
	    ,0 #phone confirmed
        ,0
        ,NULL
        ,1
        ,0
        ,1); #is registered
    


-- INSERT INTO AspNetRoles
-- (Id,Name,NormalizedName,ConcurrencyStamp)
-- VALUES
-- ('adminIdkdjvn673kjneg', 'admin', 'ADMIN', 'a229d5b7-fe0f-40a8-869f-76b2826de59a'),
-- ('providerIdkdjvn673kjneg', 'provider', 'PROVIDER', 'a229d5b7-fe0f-40a8-869f-76b2826de60b'),
-- ('parentIdkdjvn673kjneg', 'parent', 'PARENT', 'a229d5b7-fe0f-40a8-869f-76b2826de73c');

#Roles' Ids according to your data in AspNetRoles.
INSERT INTO AspNetUserRoles
(UserId
    ,RoleId)
VALUES
	('fe78ed30-7df9-412b-8f1c-a21f3175334f' #UserId (super admin)
	,(SELECT Id FROM AspNetRoles WHERE Name = 'admin' LIMIT 1)), #RoleId (super admin)
    ('16575ce5-38e3-4ae7-b991-4508ed488369' #UserId (test1)
        ,(SELECT Id FROM AspNetRoles WHERE Name LIKE('parent') LIMIT 1)) #roleId (parent)

        ,('7604a851-66db-4236-9271-1f037ffe3a81' #UserId (test2)
        ,(SELECT Id FROM AspNetRoles WHERE Name LIKE('parent')LIMIT 1)) #roleId (parent)

        ,('47802b21-2fb5-435e-9057-75c43d002cef' #UserId (test3)
        ,(SELECT Id FROM AspNetRoles WHERE Name LIKE('provider')LIMIT 1)) #roleId (provider)

        ,('5bff5f95-1848-4c87-9846-a567aeb407ea' #UserId (test4)
        ,(SELECT Id FROM AspNetRoles WHERE Name LIKE('provider')LIMIT 1)); #roleId (provider)
    

#====================PARENTS AND CHILDREN================================
#Parents
INSERT INTO Parents
(Id,UserId)
VALUES
    (UUID_TO_BIN('20000ce5-38e3-4ae7-b991-4508ed488200'),'16575ce5-38e3-4ae7-b991-4508ed488369') #UserId (test1)

        ,(UUID_TO_BIN('10000851-66db-4236-9271-1f037ffe3100'),'7604a851-66db-4236-9271-1f037ffe3a81'); #UserId (test2)
    

#Social Groups (скіпнути, коли буде заповнятися програмою)
#INSERT INTO SocialGroups
#           (Name)
#     VALUES
#		   ('Діти із багатодітних сімей') 
#		   ,('Діти із малозабезпечених сімей')
#		   ,('Діти з інвалідністю')
#		   ,('Діти-сироти')
#		   ,('Діти, позбавлені батьківського піклування')
#

#Children

INSERT INTO Children
(Id,FirstName
    ,LastName
    ,MiddleName
    ,DateOfBirth
    ,Gender
    ,ParentId
	,PlaceOfStudy)
VALUES
    (UUID_TO_BIN('10000851-66db-4236-9271-1f037ffe3101'),'Тетяна'
        ,'Батькоперший'
        ,'Іванівна'
        ,'2010-12-11'
        ,1 #gender
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200') #parent Id (parent 1, user 1)
		,'Загальноосвітня школа №125') #PlaceOfStudy

        ,(UUID_TO_BIN('10000851-66db-4236-9271-1f037ffe3102'),'Богдан'
        ,'Батькодругий'
        ,'Петрович'
        ,'2010-05-05'
        ,0 #gender
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100') #parent Id (parent 2, user 2)
		,'ЗО №14') #PlaceOfStudy

        ,(UUID_TO_BIN('10000851-66db-4236-9271-1f037ffe3103'),'Лідія'
        ,'Батькодругий'
        ,'Петрівна'
        ,'2015-10-01'
        ,1 #gender
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100') #parent Id (parent 2, user 2)
		,'СШ №1'); #PlaceOfStudy
    

#==================== PROVIDERS AND WORKSHOPS ================================
#Directions
INSERT INTO Directions (Title, Description) VALUES ('Музика', 'Музика'), ('Танці', 'Танці'), ('Спорт', 'Спорт');
    

INSERT INTO Departments (Title, Description, DirectionId)
VALUES
    ('Народних інструментів', 'Народних інструментів', 1),
    ('Духових та ударних інструментів', 'Духових та ударних інструментів', 1),
    ('Хореографічний', 'Хореографічний', 2),
    ('Олімпійські види спорту', 'Олімпійські види спорту', 3),
    ('Неолімпійські види спорту', 'Неолімпійські види спорту', 3);
    

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
    ('Альпінізм', 'II.007. Альпінізм', 5);
    

#providers addresses, workshops
INSERT INTO Addresses
(Region
    ,District
    ,City
    ,Street
    ,BuildingNumber
    ,Latitude
    ,Longitude)
VALUES
#providers
    ('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'29'
        ,50.4547
        ,30.5238) #provider1 legal

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'35'
        ,50.4547
        ,30.5238) #provider1 actual

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Вокзальна'
        ,'10'
        ,50.2648700
        ,28.6766900) #provider2 legal

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Привозна'
        ,'12А'
        ,50.2648700
        ,28.6766900) #provider2 actual

#workshops
        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'35'
        ,50.4547
        ,30.5238) #workshop1 provider1 actual

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'35'
        ,50.4547
        ,30.5238) #workshop2 provider1 actual

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'35'
        ,50.4547
        ,30.5238) #workshop3 provider1 actual

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Старонаводницька'
        ,'35'
        ,50.4547
        ,30.5238) #workshop4 provider1 actual

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Привозна'
        ,'12А'
        ,50.2648700
        ,28.6766900) #workshop5 provider2 actual

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Привозна'
        ,'12А'
        ,50.2648700
        ,28.6766900) #workshop6 provider2 actual

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) #id 11

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) #id 12

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) #id 13

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) #id 14

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ляхова'
        ,'12'
        ,51.4547
        ,31.5238) #id 15


        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) #id 16

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) #id 17

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) #id 18

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) #id 19

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Ворфоломіївська'
        ,'12'
        ,50.5
        ,30.5) #id 20

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 21

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 22

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 23

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 24

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 25

        ,('Київська обл.'
        ,'м. Київ'
        ,'Київ'
        ,'Бажана'
        ,'12'
        ,50.5
        ,31.5) #id 26

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Бойченка'
        ,'42'
        ,50.265
        ,28.677) #id 27

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Бойченка'
        ,'42'
        ,50.265
        ,28.677) #id 28

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Вокарчука'
        ,'42'
        ,50.5
        ,28.9) #id 29

        ,('Житомирська обл.'
        ,'м. Житомир'
        ,'Житомир'
        ,'Вокарчука'
        ,'42'
        ,50.5
        ,28.9); #id 30
    

#Providers
INSERT INTO Providers
(Id,FullTitle
    ,ShortTitle
    ,Website
    ,Email
    ,Facebook
    ,Instagram
    ,EdrpouIpn
    ,Director
    ,DirectorDateOfBirth
    ,PhoneNumber
    ,Founder
    ,Ownership
    ,Type
    ,Status
    ,LegalAddressId
    ,ActualAddressId
    ,UserId)
VALUES
    (UUID_TO_BIN('12300851-66db-4236-9271-1f037ffe3101'),'Музична школа №1'
        ,'Музична школа'
        ,'http://provider1'
        ,'provider1@test.com'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,'12345678'
        ,'Провайдерперший Семен Семенович' #Director
        ,'2000-10-12'
        ,'0981234567'
        ,'Ващенко Володимир Богданович' #Founder
        ,0 #Ownership(state)
        ,4 #Type(EducationalInstitution)
        ,1 #Status
        ,1 #LegalAddressId
        ,2 #ActualAddressId
        ,'47802b21-2fb5-435e-9057-75c43d002cef') #User Id (test4)

        ,(UUID_TO_BIN('12300851-66db-4236-9271-1f037ffe3102'),'Школа бойових мистецтв №2'
        ,'ШБК №2'
        ,'http://provider2'
        ,'provider1@test.com'
        ,'http://facebook/provider2'
        ,'http://instagram/provider2'
        ,'98764523'
        ,'Дорогий Захар Несторович'
        ,'1990-11-02'
        ,'0981234567'
        ,'Дорогий Захар Несторович'
        ,2 #Ownership(private)
        ,3 #Type(Private)
        ,1 #Status
        ,3 #LegalAddressId
        ,4 #ActualAddressId
        ,'5bff5f95-1848-4c87-9846-a567aeb407ea');
    

#workshops
INSERT INTO Workshops
(Id,Title
    ,Keywords
    ,Phone
    ,Email
    ,Website
    ,Facebook
    ,Instagram
    ,MinAge
    ,MaxAge
    ,Price
    ,WithDisabilityOptions
    ,DisabilityOptionsDesc
    ,CoverImageId
    ,ProviderId
    ,AddressId
    ,DirectionId
    ,DepartmentId
    ,ClassId
    ,ProviderTitle)
VALUES
    (UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3101'),'Уроки аккордиону'
        ,null
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,100 #maxAge
        ,50 #price
        ,1 #WithDisabilityOptions
        ,'Немає конкретних обмежень' # disability description
        ,'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3101') #ProviderId
        ,5 #AddressId
        ,1 #directionId
        ,2 #departmentId
        ,2 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3102'),'Уроки бандури'
        ,null
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,100 #maxAge
        ,500 #price
        ,1 #WithDisabilityOptions
        ,'Немає конкретних обмежень' # disability description
        ,'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3101') #ProviderId
        ,6 #AddressId
        ,1 #directionId
        ,1 #departmentId
        ,1 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3103'),'Гра на барабані'
        ,null
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,100 #maxAge
        ,500 #price
        ,1 #WithDisabilityOptions
        ,'Немає конкретних обмежень' # disability description
        ,'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3101') #ProviderId
        ,7 #AddressId
        ,1 #directionId
        ,2 #departmentId
        ,3 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3104'),'Уроки гри на флейті'
        ,null
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,100 #maxAge
        ,100 #price
        ,1 #WithDisabilityOptions
        ,'Немає конкретних обмежень' # disability description
        ,'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3101') #ProviderId
        ,8 #AddressId
        ,1 #directionId
        ,2 #departmentId
        ,4 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3105'),'Айкідо'
        ,null
        ,'1234567890' #Phone
        ,'provider2@test.com'
        ,'http://provider2'
        ,'http://facebook/provider2'
        ,'http://instagram/provider2'
        ,7 #minAge
        ,50 #maxAge
        ,300 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,'Логотип'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,9 #AddressId
        ,3 #directionId
        ,5 #departmentId
        ,9 #classId
        ,'Школа бойових мистецтв №2') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3106'),'Плавання'
        ,null
        ,'1234567890' #Phone
        ,'provider2@test.com'
        ,'http://provider2'
        ,'http://facebook/provider2'
        ,'http://instagram/provider2'
        ,3 #minAge
        ,100 #maxAge
        ,300 #price
        ,1 #WithDisabilityOptions
        ,'будь-які' # disability description
        ,'Логотип'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,10 #AddressId
        ,3 #directionId
        ,4 #departmentId
        ,7 #classId
        ,'Школа бойових мистецтв №2') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3107'),'Співочий аккордион дошкільнят'
        ,'аккордион¤співи'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,0 #minAge
        ,5 #maxAge
        ,0 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,11 #AddressId
        ,1 #directionId
        ,1 #departmentId
        ,1 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3108'),'Співочий аккордион юніорів'
        ,'аккордион¤співи'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 #minAge
        ,10 #maxAge
        ,0 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,12 #AddressId
        ,1 #directionId
        ,1 #departmentId
        ,1 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3109'),'Співочий аккордион тінейджерів'
        ,'аккордион¤співи'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,11 #minAge
        ,16 #maxAge
        ,0 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,13 #AddressId
        ,1 #directionId
        ,1 #departmentId
        ,1 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3110'),'Струни душі'
        ,'бандура'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 #minAge
        ,10 #maxAge
        ,360 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,14 #AddressId
        ,1 #directionId
        ,1 #departmentId
        ,1 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3112'),'Тендітні носочки'
        ,'танці¤бальні¤класичні'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,0 #minAge
        ,5 #maxAge
        ,240 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,16 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,5 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3113'),'Прима-балерина'
        ,'танці¤бальні¤класичні'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 #minAge
        ,18 #maxAge
        ,780 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,17 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,5 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3114'),'Танці вулиць'
        ,'танці¤вуличні¤сучасні'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 #minAge
        ,8 #maxAge
        ,640 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,18 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3115'),'Хіп-хоп'
        ,'танці¤сучасні'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 #minAge
        ,12 #maxAge
        ,740 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,19 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3116'),'Електрік-бугі'
        ,'танці¤сучасні¤вугі'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 #minAge
        ,12 #maxAge
        ,740 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,20 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3117'),'Диско денс'
        ,'диско'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 #minAge
        ,12 #maxAge
        ,490 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,21 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3118'),'Степ'
        ,'степ¤сучасно¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,10 #minAge
        ,12 #maxAge
        ,355 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,22 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3119'),'Диско як стан душі'
        ,'диско¤дорослі¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,18 #minAge
        ,100 #maxAge
        ,278 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,23 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3120'),'Шаффл-денс'
        ,'шаффл¤дорослі¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,18 #minAge
        ,40 #maxAge
        ,600 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,24 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3121'),'Шаффл'
        ,'шаффл¤дорослі¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,6 #minAge
        ,10 #maxAge
        ,300 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,25 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3122'),'Уроки гри на флейті'
        ,null
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,10 #maxAge
        ,90 #price
        ,1 #WithDisabilityOptions
        ,'Немає конкретних обмежень' # disability description
        ,'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,26 #AddressId
        ,1 #cateryId
        ,2 #subcateryId
        ,4 #SubsubcateryId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3123'),'Шаффл діти'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,5 #minAge
        ,8 #maxAge
        ,200 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,27 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3124'),'Шаффл школярі'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,9 #minAge
        ,12 #maxAge
        ,250 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,28 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3125'),'Шаффл підлітки'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,13 #minAge
        ,18 #maxAge
        ,350 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,29 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1') #provider title

        ,(UUID_TO_BIN('12316600-66db-4236-9271-1f037ffe3126'),'Шаффл дорослі'
        ,'шаффл¤сучасні¤танці'
        ,'1234567890' #Phone
        ,'provider1@test.com'
        ,'http://provider1'
        ,'http://facebook/provider1'
        ,'http://instagram/provider1'
        ,19 #minAge
        ,100 #maxAge
        ,550 #price
        ,0 #WithDisabilityOptions
        ,null # disability description
        ,null #'Lo'
        ,UUID_TO_BIN('12300851-66DB-4236-9271-1F037FFE3102') #ProviderId
        ,30 #AddressId
        ,2 #directionId
        ,2 #departmentId
        ,6 #classId
        ,'Музична школа №1'); #provider title
    

#teachers
INSERT INTO Teachers
(Id,FirstName
    ,LastName
    ,MiddleName
    ,DateOfBirth
    ,Description
    ,CoverImageId
    ,WorkshopId)
VALUES
    (UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488301'),'Леонід' #firs name
        ,'Вчительперший' #last name
        ,'Леонідович' #middle name
        ,'1995-09-06'
        ,'Найкращий вчитель'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101')) #workshop Id

        ,(UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488302'),'Наталія' #firs name
        ,'Вчительдругий' #last name
        ,'Богданівна' #middle name
        ,'1985-04-06'
        ,'Найкращий вчитель року'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102')) #workshop Id

        ,(UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488303'),'Катерина' #firs name
        ,'Гуляйборода' #last name
        ,'Василівна' #middle name
        ,'1995-09-06'
        ,'Найкращий вчитель'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3103')) #workshop Id

        ,(UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488304'),'Георгій' #firs name
        ,'Вчительчетвертий' #last name
        ,'Ігорович' #middle name
        ,'1985-04-06'
        ,'Найкращий вчитель року'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3104')) #workshop Id

        ,(UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488305'),'Святослав' #firs name
        ,'Вчительтретій' #last name
        ,'Ігорович' #middle name
        ,'2000-04-06'
        ,'КМС з айкідо'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3105')) #workshop Id

        ,(UUID_TO_BIN('55555ce5-38e3-4ae7-b991-4508ed488306'),'Денис' #firs name
        ,'Вчительчетвертий' #last name
        ,'Владиславович' #middle name
        ,'1998-04-06'
        ,'Чемпіон національних олімпіад з плавання'
        ,''
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3106')); #workshop Id
    

#Schedule
INSERT INTO DateTimeRanges
           (StartTime
           ,EndTime
           ,WorkshopId
           ,Workdays)
     VALUES
           ('8:00' #<StartTime>
           ,'17:30' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') #<WorkshopId>
           ,21) #<Workdays>

		   ,('8:00' #<StartTime>
           ,'20:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') #<WorkshopId>
           ,255) #<Workdays>

		   ,('8:00' #<StartTime>
           ,'20:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3103') #<WorkshopId>
           ,21) #<Workdays>

		   ,('9:00' #<StartTime>
           ,'21:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3103') #<WorkshopId>
           ,10) #<Workdays>

		   ,('12:00' #<StartTime>
           ,'19:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3104') #<WorkshopId>
           ,21) #<Workdays>

		   ,('9:00' #<StartTime>
           ,'21:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3104') #<WorkshopId>
           ,10) #<Workdays>

		   ,('9:00' #<StartTime>
           ,'21:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3106') #<WorkshopId>
           ,96) #<Workdays>

		   ,('9:00' #<StartTime>
           ,'21:00' #<EndTime>
           ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3106') #<WorkshopId>
           ,96); #<Workdays>


#Applications
INSERT INTO Applications
(Id,Status
    ,WorkshopId
    ,ChildId
    ,CreationTime
    ,ParentId)
VALUES

#workshop1
    (UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40701'), #<Status, int,>
        1,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3101') #<ChildId, bigint,>
        ,'2021-06-06 12:20:20' #<CreationTime>
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200')) #parentId

        ,(UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40702'),2 #<Status, int,>
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3102') #<ChildId, bigint,>
        ,'2021-06-04 15:34:20' #<CreationTime>
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200')) #parentId

        ,(UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40703'),1 #<Status, int,>
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3103') #<ChildId, bigint,>
        ,'2021-06-05 08:23:20' #<CreationTime>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100')) #parentId

#workshop2
        ,(UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40704'),1 #<Status, int,>
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3101') #<ChildId, bigint,>
        ,'2021-06-15 17:20:00' #<CreationTime>
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200')) #parentId

        ,(UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40705'),1 #<Status, int,>
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3102') #<ChildId, bigint,>
        ,'2021-06-15 17:26:10' #<CreationTime>
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200')) #parentId

        ,(UUID_TO_BIN('88ff5f95-1848-4c87-9846-a567aeb40706'),1 #<Status, int,>
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') #<WorkshopId, bigint,>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3103') #<ChildId, bigint,>
        ,'2021-06-15 18:00:45' #<CreationTime>
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100')); #parentId
    

#Rating
INSERT INTO Ratings
(Rate
    ,Type
    ,EntityId
    ,ParentId
    ,CreationTime)
VALUES
#parent 1
    (5 #rating
        ,2 #workshop
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') # workshopId
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200') #parent
        ,'2021-01-15 18:00:45')

        ,(5 #rating
        ,2 #workshop
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') # workshopId
        ,UUID_TO_BIN('20000CE5-38E3-4AE7-B991-4508ED488200') #parent
        ,'2021-02-15 18:00:45')

#parent 2
        ,(5 #rating
        ,2 #workshop
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3101') # workshopId
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100') #parent
        ,'2021-01-02 18:00:45')

        ,(5 #rating
        ,2 #workshop
        ,UUID_TO_BIN('12316600-66DB-4236-9271-1F037FFE3102') # workshopId
        ,UUID_TO_BIN('10000851-66DB-4236-9271-1F037FFE3100') #parent
        ,'2021-02-02 18:00:45');
    