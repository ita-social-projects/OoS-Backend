USE OutOfSchoolDb;

INSERT INTO AspNetUsers
(Id,
CreatingTime,
LastLogin,
LastName,
MiddleName,
FirstName,
Role,
UserName,
NormalizedUserName,
Email,
NormalizedEmail,
EmailConfirmed,
PasswordHash,
SecurityStamp,
ConcurrencyStamp,
PhoneNumber,
PhoneNumberConfirmed,
TwoFactorEnabled,
LockoutEnd,
LockoutEnabled,
AccessFailedCount,
IsRegistered,
MustChangePassword)
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
1, #IsRegistered
1) #MustChangePassword
ON DUPLICATE KEY UPDATE # to reset sensitive data   
Email='admin@adm.com', #Email
PasswordHash = 'AQAAAAEAACcQAAAAEB+AG8yyfG66fAkFIJ1mMdDx5ChM/wxoUHIwBuJt4cuO+jDCrPMw7D9BLYRtkvbrpg==', #PasswordHash  ->   Kf%ms3nAp7%aH
SecurityStamp = 'G4GRJFN7ET7EVVDQ4NEQQOY6SETRULJB', #SecurityStamp
ConcurrencyStamp = 'ed689760-8ab0-45fa-8b13-ec431c093627', #ConcurrencyStamp
MustChangePassword = 1; #MustChangePassword

INSERT INTO AspNetUserRoles
(UserId, RoleId)
VALUES
((SELECT Id FROM AspNetUsers WHERE Role = 'admin' LIMIT 1) #UserId (super admin)
,(SELECT Id FROM AspNetRoles WHERE Name = 'admin' LIMIT 1)) #RoleId (super admin)

