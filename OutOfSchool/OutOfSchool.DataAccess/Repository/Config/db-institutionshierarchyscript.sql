USE out_of_school;

delete from directioninstitutionhierarchy;
delete from institutionfielddescriptions;
delete from institutionhierarchies where HierarchyLevel = 4;
delete from institutionhierarchies where HierarchyLevel = 3;
delete from institutionhierarchies where HierarchyLevel = 2;
delete from institutionhierarchies where HierarchyLevel = 1;
delete from institutions;

#Institutions

INSERT INTO institutions (Id, Title, NumberOfHierarchyLevels)
VALUES (UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), "МОН", 2)
, (UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), "МКІП", 4)
, (UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), "Мінспорт", 2)
, (UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), "Інше", 1);

#institutionfielddescriptions

insert into institutionfielddescriptions (Id, Title, HierarchyLevel, InstitutionId)
VALUES 
(UUID_TO_BIN("41ee97f4-6aff-47e7-aeac-2e1c20ece25a"), "Назва напрямку", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"))
,(UUID_TO_BIN("a7c39732-2cb3-4350-bf7b-1af4f9a0a280"), "Назва профілю/наукового відділення", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"))
,(UUID_TO_BIN("76b45cd2-1538-4591-af81-6d26d3f3d66a"), "Назва напрямку", 1, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"))
,(UUID_TO_BIN("16df1a49-3cfd-4fb3-9af8-e8d9b668ba55"), "Назва профілю", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"))
,(UUID_TO_BIN("44bdc490-0746-4704-a672-cb7adf5683ec"), "Назва відділу/відділення", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"))
,(UUID_TO_BIN("64b92715-2526-48bb-a0e5-222642b748a3"), "Назва класу", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"))
,(UUID_TO_BIN("5b0511be-9ccf-4fc4-9442-22aa432e4c43"), "Вид спорту", 1, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"))
,(UUID_TO_BIN("46806e1c-ad5a-4de4-9ca4-bfceb0b684f2"), "Назва виду спорта", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"))
,(UUID_TO_BIN("c63644c2-42b3-49e9-a909-6b01738132f0"), "Назва напрямку", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"));

#InstitutionHierarchies
INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"), "Науково-технічний напрям", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("7f09379f-b7e4-41df-b687-a2addd279077"), "Початково-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("d2402797-d91a-4a73-ad30-0602ad0dd77c"), "Спортивно-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("7e2c9407-868c-42a1-874e-2504a06b8837"), "Предметно-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("f5aa174f-4c30-4b29-859f-f6ba40417d73"), "Інформаційно-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("94e0d2b0-7173-4f5e-9f48-8e40cad3d037"), "Художньо-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("c6188362-ef6a-42ff-b0e0-118886e4207d"), "Виробничо-технічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("ac4dd8e5-0eeb-4aba-af1e-076785326e62"))
, (UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"), "Художньо-естетичний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("4c431342-9691-4203-85b3-a9dedbba207b"), "Музичний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("63c3ec8a-55fc-49ce-9e7a-9c5dec450ca6"), "Вокальний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("efca2ed9-5bdb-445c-a837-09e42978c1b4"), "Хореографічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("fad1e126-1e13-4e2a-b381-083ef1d1cbd1"), "Театральний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("2ac82b6b-0a58-4d1a-9121-3183b895b8cb"), "Цирковий", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("f0a2d774-b2d6-4afa-89f9-9640a44904f0"), "Образотворчий", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("2181c940-66ed-426a-9062-438dcd361358"), "Декоративно-ужитковий", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("3b25a026-0a48-44cb-b0f9-6168c24f098c"), "Фото- та кіномистецтва", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("75d3d7c7-b129-448c-b503-d7ace286ac70"))
, (UUID_TO_BIN("6577d9dc-0b51-4249-8ae7-b653a78b4e29"), "Еколого-натуралістичний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("35303c62-27ba-4761-9747-1dd6ed28fc10"), "Лісівничий", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("6577d9dc-0b51-4249-8ae7-b653a78b4e29"))
, (UUID_TO_BIN("ed2fe80a-8955-497e-98ac-ea88558bb74c"), "Еколого-біологічний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("6577d9dc-0b51-4249-8ae7-b653a78b4e29"))
, (UUID_TO_BIN("f686828f-322f-4796-a05a-085fd85e8f06"), "Туристсько-краєзнавчий", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("c79667b2-9c8b-4f43-8a4b-f961b2ca61b8"), "Туристсько-краєзнавчий", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f686828f-322f-4796-a05a-085fd85e8f06"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("4ffa1b28-2784-49a6-b8e8-b6387306f6f6"), "Туристсько-спортивний", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f686828f-322f-4796-a05a-085fd85e8f06"))
, (UUID_TO_BIN("d5b90591-887f-4c83-9d18-dba4894f212b"), "Військово-патріотичний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"), "Дослідницько-експериментальний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("7623de2f-dd39-4856-a09d-b026a4ae2c85"), "Наукове відділення економіки", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("aad6f415-5601-4f1b-980b-1098e1f3a935"), "Наукове відділення математики", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("b31ce691-600d-420e-959a-0da65316a908"), "Наукове відділення фізики і астрономії", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("5056cb1e-20b0-4990-a12e-6220c34c75e4"), "Наукове відділення «Філософії  та суспільствознавства»", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("c0a7b94d-806c-4115-b0c6-1675018c59d0"), "Наукове відділення наук про Землю", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("ecaca89b-594c-4c50-9bfb-2146ee9c45cd"), "Наукове відділення історії", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("3f25362b-2a9e-4852-9708-2012b0cc1385"), "Наукове відділення мовознавства", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("5c536b66-9922-4f87-9650-6bae88bf7c66"), "Наукове відділення хімії та біології", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("89d4ab3a-22f4-4093-9645-82381c8a138c"), "Наукове відділення технічних наук", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("939169da-fb61-49f5-9c60-0eac4c6fd68d"), "Наукове відділення літературознавства, фольклористики та мистецтвознавства", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("f5c3099e-7ca6-46a3-9704-7d8c20d281e4"))
, (UUID_TO_BIN("0bbeaeb1-ee5d-410c-b1e5-3d28f4181e6f"), "Пластово-Скаутський", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("3ca6616c-5ff1-4052-9454-5ce5d9f59ba7"), "Пласт", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("0bbeaeb1-ee5d-410c-b1e5-3d28f4181e6f"))
, (UUID_TO_BIN("f76424af-3c6f-4fc6-b675-9dbb86b4bc31"), "Скаутинг", 2, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), UUID_TO_BIN("0bbeaeb1-ee5d-410c-b1e5-3d28f4181e6f"))
, (UUID_TO_BIN("fe39520b-46af-4fd3-8e6d-c3441260d2ea"), "Фізкультурно-спортивний або спортивний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("82362cd3-675c-4065-b983-948fd7f00750"), "Бібліотечно-бібліографічний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("fb1e570a-dc90-4753-852d-747b0e68a765"), "Соціально-реабілітаційний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("c17879d7-c34b-47ba-96cd-79eeec61a7f8"), "Оздоровчий", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null);

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("c620921a-90b6-4dda-b27a-232a53ad81e9"), "Гуманітарний", 1, UUID_TO_BIN("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"), null)
, (UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"), "Мистецький", 1, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), null)
, (UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"), "Музичний", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"))
, (UUID_TO_BIN("44571be1-1451-43da-9046-b26d8699001c"), "Фортепіано", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("666d5fd8-ce1b-4ece-ae94-0670a3747274"), "Клас фортепіано", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("44571be1-1451-43da-9046-b26d8699001c"))
, (UUID_TO_BIN("6f4fbca9-9588-4aa4-8735-1e4759b8cf0c"), "Клас електронних клавішних (синтезатор)", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("44571be1-1451-43da-9046-b26d8699001c"))
, (UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"), "Народних інструментів", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("e7be108c-e344-453a-905d-84edacc36568"), "Клас бандури", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"))
, (UUID_TO_BIN("69a88ac6-ee07-4ea7-bd00-604e3907f073"), "Клас цимбал", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"))
, (UUID_TO_BIN("47c3b40a-edd0-4714-939a-821eb47619ce"), "Клас домри", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("4dd8ae2c-41ac-48f5-9bcb-e10c9cea94bd"), "Клас баяну", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"))
, (UUID_TO_BIN("8b5d69f0-646c-46a4-8ef3-d2769a4de7f0"), "Клас акордеону", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("05550884-237f-4b7c-847c-4690e368ff63"))
, (UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"), "Духових та ударних інструментів", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("0b033e3a-d6b3-4ac0-aad1-0cc45ce2eb50"), "Клас ударних", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("0f8748d3-2ccd-43b2-b622-6e20ebdadf06"), "Клас барабану", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("bdebd328-f63f-4f43-adc2-a7c4cad22ded"), "Клас флейти", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("5e531788-e7d7-4899-80cb-b3f696c42707"), "Клас кларнету", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("6e4bf5e4-e133-4858-b8f1-c33d51cb55d8"), "Клас саксофону", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("3d56ebd7-cf63-4a9b-9254-1468fa957b0d"), "Клас ксилофону", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"))
, (UUID_TO_BIN("f871708b-5e1e-490f-9efa-0161017bf5d8"), "Клас труби", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("23f9a065-5824-486f-bae3-1765ae751ec3"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"), "Струнних та смичкових інструменти", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("17257809-984f-47f3-a9ed-7779130ce2a5"), "Клас скрипки", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("ee63b10b-b2bb-486d-81fe-984b22868ea0"), "Клас віолончелі", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("19677ade-4755-4a09-b7ee-c8f1f94992a3"), "Клас арфи", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("b2b386a5-e9bf-48b7-91d1-16c38c4c45b5"), "Клас електрогітари", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("283f7e96-a465-4ec6-8903-87518574c261"), "Клас бас-гітари", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("f020ce2c-2b8a-4780-ba58-162690cfeefa"), "Клас гітари", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("da930700-8355-4cd3-a42d-cb2589150e2f"))
, (UUID_TO_BIN("ac3e8c2b-2eab-420c-8899-91dae8f6dd82"), "Хорового мистецтва", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("ab6d0a84-ddbc-4c51-bfff-9842732fbc41"), "Клас хорового співу", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("ac3e8c2b-2eab-420c-8899-91dae8f6dd82"))
, (UUID_TO_BIN("2b874412-c9c9-4086-aa2f-e10dcbee1fc2"), "Сольного співу", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("0f172033-98ad-4382-9543-e2a79b238431"), "Клас естрадного співу", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("2b874412-c9c9-4086-aa2f-e10dcbee1fc2"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("fdd4f225-da2a-42b9-8385-28833474613f"), "Клас народного співу", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("2b874412-c9c9-4086-aa2f-e10dcbee1fc2"))
, (UUID_TO_BIN("61e41b37-a867-4b59-afca-0326c94ccbfd"), "Клас академічного співу", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("2b874412-c9c9-4086-aa2f-e10dcbee1fc2"))
, (UUID_TO_BIN("def6a039-52f7-472b-b38b-4bbf003b8dc7"), "Фольклорного мистецтва", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("90d8f765-f9c6-46c3-8b24-2498a48a99ad"))
, (UUID_TO_BIN("cfb89ff8-bd2b-44fd-875f-821ff8bac40d"), "Клас музичного фольклору", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("def6a039-52f7-472b-b38b-4bbf003b8dc7"))
, (UUID_TO_BIN("186dcac7-a808-42f3-b26c-164c8a9dc5b7"), "Образотворчий", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"))
, (UUID_TO_BIN("e98afc56-28a3-40cd-9cc8-b6b47dd119bc"), "Художнього мистецтва", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("186dcac7-a808-42f3-b26c-164c8a9dc5b7"))
, (UUID_TO_BIN("dd574fcc-d843-48e8-8768-6a398984c53c"), "Клас малювання (рисунок, живопис, композиція)", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e98afc56-28a3-40cd-9cc8-b6b47dd119bc"))
, (UUID_TO_BIN("8614e19c-ea71-426d-9c7d-2bdee150d7a6"), "Клас ліплення", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e98afc56-28a3-40cd-9cc8-b6b47dd119bc"))
, (UUID_TO_BIN("0e817417-65af-4e2c-99d3-2a1a1229400f"), "Клас скульптури", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e98afc56-28a3-40cd-9cc8-b6b47dd119bc"))
, (UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"), "Декоративного мистецтва", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("186dcac7-a808-42f3-b26c-164c8a9dc5b7"))
, (UUID_TO_BIN("e7abfc2a-2cd2-4586-8c17-480ec0d38459"), "Клас вишивки", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("0f942db0-0d5c-4874-809a-670d4d288738"), "Клас витинанки", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("4d91ade2-82d5-4842-9c33-b07fecce76ec"), "Клас Петриківського розпису", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("d6524e40-0d62-4ac5-8ba4-148a9578f0e9"), "Клас декоративних розписів України", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("3269f99f-e34f-4093-804c-876a3617ff35"), "Клас ткацтва", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("37c4bd36-ebb8-450d-a9fc-f2f4c409aea2"), "Клас килимарства", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("2baede05-11de-4cb5-9d56-f076b5e7140d"), "Клас гончарства", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("a6d8f5ba-c4de-4458-bad0-44d8ca80babc"), "Клас кераміки", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("19d3ad03-ca92-4ab4-8b65-ea66a4b658e6"), "Клас різьблення", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("4afb4f54-3d18-47a7-adb3-9c70acf50798"), "Клас лозоплетіння", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("134f8851-28e4-4119-b33a-5345d0f1ca2b"), "Клас паперопластика", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("48aaba2f-da50-4b35-b165-0738a73d2e2a"), "Клас бісероплетіння", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("a111a3a7-79f9-47cc-913b-bb74ef0070b5"), "Клас виготовлення народних ляльок", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("56d1b016-5808-4a6f-949f-6102e3c14e0b"), "Клас батику", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("9dd397dc-f8f9-46aa-9292-a53a5881d83f"))
, (UUID_TO_BIN("a09518c6-64c9-42fe-b665-a0d200cae755"), "Хореографічний", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"))
, (UUID_TO_BIN("664753f6-7424-4ff8-b4b8-002f059b8105"), "Хореографічний", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("a09518c6-64c9-42fe-b665-a0d200cae755"))
, (UUID_TO_BIN("fe329584-1bdf-40ce-9aa5-5b06f870bb2b"), "Клас народно-сценічного танцю", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("664753f6-7424-4ff8-b4b8-002f059b8105"))
, (UUID_TO_BIN("fe9b3c8a-870c-4c73-b73c-08fee994f82a"), "Клас бального танцю", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("664753f6-7424-4ff8-b4b8-002f059b8105"))
, (UUID_TO_BIN("1ce025e2-8784-4884-a780-e68b21f0586d"), "Клас класичного танцю", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("664753f6-7424-4ff8-b4b8-002f059b8105"))
, (UUID_TO_BIN("c62aa157-3583-4da8-a4f9-85fc7aa6d361"), "Клас сучасного танцю", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("664753f6-7424-4ff8-b4b8-002f059b8105"))
, (UUID_TO_BIN("e7d1cb13-0856-4b7f-87ff-22db17e9e6f1"), "Кіно", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"))
, (UUID_TO_BIN("ab5aa65a-1506-4cb9-953c-a63241750fa0"), "Кіно та мультимедії", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e7d1cb13-0856-4b7f-87ff-22db17e9e6f1"))
, (UUID_TO_BIN("314f234e-ff39-45c9-b876-9f3921250183"), "Клас кіномистецтва", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("ab5aa65a-1506-4cb9-953c-a63241750fa0"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("2fe73c9a-7b71-4e10-8447-7ad16f4e1725"), "Клас мультимедіа", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("ab5aa65a-1506-4cb9-953c-a63241750fa0"))
, (UUID_TO_BIN("cb78521b-6fef-484b-ae09-9c612603a68a"), "Клас анімації", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("ab5aa65a-1506-4cb9-953c-a63241750fa0"))
, (UUID_TO_BIN("e1e311a7-1c12-4eed-bc3a-368afbfb7d78"), "Театральний", 2, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("991471cb-b7a3-4312-bb11-3714ee56cf8b"))
, (UUID_TO_BIN("8aab2306-4c70-46b2-9ec4-9ede27cb8567"), "Циркового мистецтво", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e1e311a7-1c12-4eed-bc3a-368afbfb7d78"))
, (UUID_TO_BIN("a87003a3-9ffc-487f-8abc-1c59bae69e73"), "Цирковий клас", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("8aab2306-4c70-46b2-9ec4-9ede27cb8567"))
, (UUID_TO_BIN("276d7530-f6e0-4b28-84ec-369f43b2074a"), "Театральне", 3, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("e1e311a7-1c12-4eed-bc3a-368afbfb7d78"))
, (UUID_TO_BIN("f39340bf-6469-4a51-9322-fa6eeb98c759"), "Клас драматичного театру", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("276d7530-f6e0-4b28-84ec-369f43b2074a"))
, (UUID_TO_BIN("978fa8f7-9c08-4ed8-ac0e-30232e342734"), "Клас музичного театру", 4, UUID_TO_BIN("55bef501-0838-493c-b5f2-7fead48fd8de"), UUID_TO_BIN("276d7530-f6e0-4b28-84ec-369f43b2074a"))
, (UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"), "Олімпійські види спорту", 1, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), null)
, (UUID_TO_BIN("c8ebcd0f-c20d-4c55-84c4-b2d23c474bb3"), "I.001. Бадмінтон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("65abc134-4618-4874-815a-72f878245213"), "I.002. Баскетбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("3be71038-5a18-41a6-b266-5f265cd0ab41"), "I.003. Бейсбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("41201d1c-7a05-468e-af6b-65f1cc07ae58"), "I.004. Біатлон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("8d1bd401-1197-4c08-b8b0-18e7a21d63a3"), "I.005. Бобслей", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("1b1014b1-3ce0-4a89-9b99-ceacbdbfed6b"), "I.006. Бокс", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("584b1689-88b1-4f02-a127-82c57abc3604"), "I.007. Боротьба вільна", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("67c47f55-8ced-4bbd-a3ab-583187c489cf"), "I.008. Боротьба греко-римська", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("4fc52d5c-2ede-4302-924c-fbcf802e7b78"), "I.009. Важка атлетика", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("928da7bd-3a8e-429c-9ca1-50c3e7741ca7"), "I.010. Велосипедний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("782d5ce3-468d-4745-a0d4-49c7f440c8b2"), "I.011. Веслувальний слалом", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("85f8778f-57d9-4111-a78b-9dd501474e3b"), "I.012. Веслування академічне", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("b6d887a0-7d72-4479-acec-13376e7e3d48"), "I.013. Веслування на байдарках і каное", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("594c3a85-cf4a-4d32-94b1-99e5d60c83f1"), "I.014. Вітрильний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("114eec08-32db-464d-a235-a01ec7ee80b6"), "I.015. Водне поло", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("9436442b-0bbc-4334-a4e4-94b16c9799a3"), "I.016. Волейбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("d7a30a2e-1b1a-46b0-96c5-e372b6667687"), "I.017. Волейбол пляжний", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("6c509e8a-e93a-4cae-acb8-f3391eee0346"), "I.018. Гандбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("25c0f3b2-1b9e-4b16-86e5-3867cc21742d"), "I.019. Гімнастика спортивна", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("a685f2bb-e6ff-4bd5-bfd2-bb5244e004ef"), "I.020. Гімнастика художня", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("81177045-33ca-43f9-8ba8-5c788e44ab12"), "I.021. Гірськолижний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("05b1d8af-c34e-46f7-bd24-26d6b2b9c871"), "I.022. Гольф", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("3301553d-c2ef-41c0-b833-8e23f7926ca4"), "I.023. Дзюдо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("80093681-86b5-4035-af67-a6887fe654d2"), "I.024. Кінний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("b77120b1-74fb-4a71-80c9-53e124cdac50"), "I.025. Ковзанярський спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("1159305c-77cc-406b-94c1-da6f5782317f"), "I.026. Кьорлінг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("f300633c-9248-4ce7-82f4-f4ea57c0e6b4"), "I.027. Легка атлетика", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("b00d7436-3308-40b6-8cbf-9f6ffe4cc18c"), "I.028. Лижне двоборство", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("a5518b57-2375-4438-97f9-dc7a748c3fa3"), "I.029. Лижні гонки", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("02da382a-14fd-4f0b-a09b-dcead6c9578f"), "I.030. Плавання", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("ce7fccfa-6a8b-42b3-8b13-d5d615c0b684"), "I.031. Плавання синхронне", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("d1676510-7f43-441e-b867-72bfc6f8f5e9"), "I.032. Регбі", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("50eb3503-aaaa-4459-b944-f819a280af47"), "I.033. Санний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("858d89bf-5101-42ab-8ee1-c6a5f9e37085"), "I.034. Сноуборд", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("7b820d6e-bd3e-406d-bd22-18c247b72b65"), "I.035. Софтбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("ca97d65a-bcc1-4aa3-af32-eaa5bfc41891"), "I.036. Стрибки на батуті", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("953c0104-94ec-422d-9bcd-3889bc9cb331"), "I.037. Стрибки на лижах з трампліна", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("fa978978-5a3a-412f-ac1b-f2fd9e4d697e"), "I.038. Стрибки у воду", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("3ed0d9ae-8e69-4029-8a2a-543b7fa85101"), "I.039. Стрільба з лука", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("c290c7f8-d69d-4183-acaf-f2723e8c3966"), "I.040. Стрільба кульова", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("2c8352d9-2920-4984-bf1a-ab6023f25883"), "I.041. Стрільба стендова", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("4a0286e8-2682-4e10-a10e-82564f46e40b"), "I.042. Сучасне п'ятиборство", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("1d4011fa-fee5-4015-8ced-0d4b13fa38c3"), "I.043. Теніс", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("2a796681-f624-4981-9084-05cb60e1ddc6"), "I.044. Теніс настільний", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("f0d7d048-6bbc-478e-b3ec-a4557e44e371"), "I.045. Триатлон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("a8a25d1b-5ae4-4743-aee9-72428c1ad47c"), "I.046. Тхеквондо (ВТФ)", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("26786218-ed5d-4c55-b3e5-5c87e9cb4d4b"), "I.047. Фехтування", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("c477c51c-6e4c-4d79-8cd8-9d403146f33f"), "I.048. Фігурне катання на ковзанах", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("ad5a4b6c-4776-4d09-9484-b8485c651c76"), "I.049. Фристайл", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("f97c6a3d-df4b-47e9-935e-0133be723a13"), "I.050. Футбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("092d4b9a-5d96-47ff-9ea8-04aae028a6a8"), "I.051. Хокей з шайбою", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("f7ecb95b-b443-451f-85a9-7fa5006b65eb"), "I.052. Хокей на траві", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("5c9b9d40-fb45-4a99-98d3-b66037aa8d0f"), "I.053. Шорт-трек", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("cc9acf94-b272-4094-af5a-f4a6f5ec0c10"), "I.055. Карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("2a5c518f-f1c2-47be-9ddd-e0532af6278e"), "I.056. Скелелазіння", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("22f22e39-831f-46f1-8317-ea4448b199f4"))
, (UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"), "Неолімпійські види спорту", 1, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), null)
, (UUID_TO_BIN("a5c0e0c0-5688-4459-9996-1540828cd4ca"), "II.001. Авіамодельний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("e3e24f68-47c1-41ec-875b-3102c7131485"), "II.002. Автомобільний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2da20dfd-c853-4329-a0c3-188249398c08"), "II.003. Автомодельний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("ff7fbdcc-d7cd-4ea3-88c0-622942043262"), "II.004. Айкідо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("46a503fa-aed4-4ae7-abc5-ebe2a63fb86d"), "II.005. Аквабайк", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("60f11f21-5d9a-401b-b233-b34f798ce9a0"), "II.006. Акробатичний рок-н-рол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("fcc5c739-6b37-4416-9eb4-b7e4240a862a"), "II.007. Альпінізм", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("105c0aaf-ad98-4a6d-8052-dab72165a653"), "II.008. Американський футбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("42def3b2-62c8-4fad-a2f0-8ac93a4a36a5"), "II.009. Армспорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("e922728e-d141-4826-b697-bf16ce1dce88"), "II.010. Багатоборство тілоохоронців", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("b6fa31c1-f314-468e-8c70-b190e8904e95"), "II.011. Більярдний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("d84aa35d-4a88-46e0-916a-9284fec54c1d"), "II.012. Богатирське багатоборство", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("759f6fc5-84b9-403d-8a85-219a6c3ed3a8"), "II.013. Бодібілдинг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("60dfcf3a-5b1a-40b4-b4f7-fcbf42ea1fda"), "II.014. Бойове самбо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("e281beff-97e7-4d94-b7a2-9da5470265a4"), "II.015. Боротьба Кураш", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("3f136236-b9e5-4a6c-a770-d9325a98b504"), "II.016. Боротьба на поясах", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("1e5f4fd5-cc86-45c6-a55e-dba79d968ddd"), "II.017. Боротьба на поясах Алиш", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("1192c69b-50aa-4ae0-ba48-c8ac7685c38f"), "II.018. Боротьба самбо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("d8d89ab0-78df-4ae5-b77f-52664d876273"), "II.019. Боулінг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("0cee42fe-cca0-4b20-a4cf-0587c013ab83"), "II.020. Вейкбординг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("fd7d9506-3829-49c3-abf2-74e3a6e8cb54"), "II.021. Вертолітний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("3ff63798-7d69-4c4a-981c-f49a4ce47b85"), "II.022. Веслування на човнах \"Дракон\"", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("961064de-8e43-4a48-9e75-98f9c687cf05"), "II.023. Військово-спортивні багатоборства", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("dc613473-4e03-41be-a1d5-e68272f24729"), "II.024. Воднолижний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("aca590a1-160a-4627-b2a4-dcccb709b047"), "II.025. Водно-моторний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("d6126815-27eb-4203-818a-432d03caa266"), "II.026. Гирьовий спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("dcc574cb-0314-41c2-bc03-b8001be86ef0"), "II.027. Го", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("18aa2391-f09e-42f2-a16c-2d004645b657"), "II.028. Годзю-рю карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2e027f6e-f786-4e71-b7f3-ee8976466285"), "II.029. Голубиний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("4d320ec7-7b22-46be-8401-6de5d32de115"), "II.030. Городковий спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("9a4563e5-39c7-475d-bcf8-b5a8b794e5c0"), "II.131. Дартс", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("3e9be0ad-509f-4253-a4f4-e4a7b9b4394d"), "II.032. Дельтапланерний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("782461d1-e6c2-43cb-8d04-b903d399f5f0"), "II.033. Джиу-джитсу", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("0eeae908-f972-4e50-b37f-3ae4dae4206c"), "II.034. Естетична групова гімнастика", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("7be8ad85-26f7-4cd3-83ab-c41f4df468e7"), "II.035. Змішані єдиноборства (ММА)", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("5614469d-8951-4353-a004-e434450f1862"), "II.036. Карате JKA WF", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("baa87fc6-16c2-4a87-9921-d70717e91e6c"), "II.037. Карате JKS", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("111a3802-e6e4-4804-a66e-c1c2a5ecd807"), "II.038. Карате WKC", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("6a3dea32-d972-449d-9f02-40c71bf07ff1"), "II.040. Кікбоксинг WKA", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("0326cd18-16d9-4fc5-9ba5-04ee7d096fb5"), "II.041. Кікбоксинг WPKA", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("5b2a27ba-b078-4355-81b0-4145f7dadc00"), "II.042. Кікбоксинг WAKO", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("e44978a7-1cc2-4ab4-ba3e-90b46ba370db"), "II.043. Кікбоксинг BTKA", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("f2a38a41-83bf-46a9-86db-3d821bb6217d"), "II.044. Кіокушин карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("dccc2a58-59b4-4e6b-a8ce-9c716cf539b1"), "II.045. Кіокушинкай карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("4d541618-ea14-4ea6-afef-744187064472"), "II.046. Кіокушинкайкан карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("0358b30d-9888-4eab-af8b-dcb34dae0b3e"), "II.047. Козацький двобій", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("3ab484f2-d73e-42e7-8569-ad6018925840"), "II.048. Комбат Дзю-Дзюцу", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("f4353e5d-0861-4696-ba11-9c43d0fb6a1b"), "II.049. Косіки карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("9596670f-0311-42fa-a8c2-e75c7818182b"), "II.050. Кунгфу", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("c8ecb3ee-8fb8-4151-b12d-40206840b33a"), "II.051. Літаковий спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("a561e2b6-d939-4e22-b7bb-7d34870f11f9"), "II.052. Міні-гольф", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("3773e2ba-0d84-4139-b1a7-9299c640660c"), "II.053. Морські багатоборства", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("f7fb164e-4aad-4127-8850-d8dfc2fd2129"), "II.054. Мотоциклетний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("e9fa01d9-e7f2-494d-a527-29d25dc75afb"), "II.055. Панкратіон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("256e3301-c0f9-4235-bce3-1ddcc636ea07"), "II.056. Парапланерний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("c0a992e2-8425-419c-903c-2fd14cb02e97"), "II.057. Парашутний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("69e694a2-47a7-4fc0-8c56-61514ae1de25"), "II.058. Пауерліфтинг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("06b5671f-155c-4cc1-90fb-011a01f585fd"), "II.059. Пейнтбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("9372fa15-561c-47f3-842c-afe04330fb8b"), "II.060. Перетягування канату", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("49e64abd-d9c8-430f-895c-b2a237422af4"), "II.061. Петанк", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2679c50d-751e-418e-975b-4e7eb7e44064"), "II.062. Підводний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2448debd-8ad9-451b-9abf-50bf2d9d15e2"), "II.063. Планерний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("465da170-3c79-4803-b272-544aa9ca1f0e"), "II.064. Пляжний гандбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2a5123d5-082d-499b-ad89-cd700e6e6e44"), "II.065. Пляжний футбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("4d4c6083-8f09-47b3-a5e0-49af6eeb102d"), "II.066. Повітроплавальний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("7e4b55a6-e0d9-4488-9ee1-a70a016a11c3"), "II.067. Пожежно-прикладний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("bf7594c6-02d9-4fd2-9ba3-2a76533f3440"), "II.068. Поліатлон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("dcd328da-f00c-4a13-825f-df50fe94ba93"), "II.069. Професійний бокс", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("65888d45-982b-49a8-b9aa-97fcf92abf90"), "II.070. Радіоспорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("685bc875-99da-488e-bc10-2fbcba976b42"), "II.071. Ракетомодельний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("063b130b-7a0f-4d26-91db-8bae16405f0f"), "II.072. Регбіліг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("b9f401b6-1975-42c9-94bf-a125fd76af30"), "II.073. Риболовний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("ea99b1ea-0f9d-4a99-b172-32bb6624b6f5"), "II.074. Роликовий спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("627c7489-c7df-49e5-9447-dfbfbc53706b"), "II.075. Рукопашний бій", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("64172b5a-afcf-4649-8a06-980c956e13ca"), "II.076. Сквош", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("5f37f658-681b-42c2-83da-ea4970166ea3"), "II.078. Спорт із собаками", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("47ccd2ab-8b0c-4ce6-bc82-3d32128da08d"), "II.079. Спорт надлегких літальних апаратів", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("c993e2ef-1bf2-4893-b074-6a4c1901da65"), "II.080. Спортивна аеробіка", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("a7eb2e09-a706-477a-a09f-41480b1e7b34"), "II.081. Спортивна акробатика", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("c5e3e82e-6cda-47e1-bb83-aa5d40d0dc43"), "II.082. Спортивне орієнтування", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("bf8db5cc-5a96-4898-bc03-825e00cb640e"), "II.083. Спортивний бридж", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("644f9b0b-883d-41ff-98f2-27568bc79c00"), "II.084. Спортивний туризм", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("972b7558-a935-4dfc-9fe5-d13060bbeb54"), "II.085. Спортивні танці", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("97b46647-acac-4a6e-ac3c-47aae0e966f0"), "II.086. Спортінг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("c6b9d359-059f-440c-b15a-4d874790a94b"), "II.088. Стронгмен", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("80ae816e-aa2e-4a18-8988-0289e1216312"), "II.089. Судномодельний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("4e1fff27-8f4c-431a-81f2-9e68f16aa079"), "II.090. Сумо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("75b60f2b-5285-46a8-9068-ba12a51951ad"), "II.091. Таеквондо (ІТФ)", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("d4ab6c7b-9153-498c-9f70-bb238581d811"), "II.092. Таїландський бокс Муей Тай", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("b97f65e8-03c0-4235-b13c-0a87a74aa1a3"), "II.093. Танцювальний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("73534e25-fcb2-4351-a896-ce5062934028"), "II.094. Традиційне карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("882be549-b106-42d9-a6eb-3ccdb27fffef"), "II.095. Українська боротьба на поясах", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("edcaef95-7861-45df-b34a-c9b94eb53451"), "II.096. Український рукопаш \"Спас\"", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("7ff2571c-c7ac-4ff0-83f9-c93e9b0f1345"), "II.097. Універсальний бій", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("9eb1cff3-1433-41c9-a9bb-cd36bef36103"), "II.098. Ушу", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("d3a3bd05-7e82-46ad-b093-33e519b07586"), "II.099. Фітнес", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2798ae05-23fa-4e73-b57c-9bd9ecb5ce13"), "II.100. Флорбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("cecb9b2d-8055-46bd-ae5c-7a6f0c282b0f"), "II.101. Французький бокс Сават", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("1508fd74-fb78-4281-b032-23c9f5afac54"), "II.102. Фрі-файт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("03c8c776-0f94-41a7-b177-800edcf87375"), "II.103. Фунакоші шотокан карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("bdeeb248-1670-4be5-8672-ed0684b603ba"), "II.104. Футзал", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("5caa6152-6490-4dec-9d4a-4e2848689782"), "II.105. Хортинг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("c2738458-417b-407a-ae2b-973c2f87bd7e"), "II.106. Черліденг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2b8fef1a-3f4e-40e4-bdfe-5ead1deb22ee"), "II.107. Шахи", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("3cf300c1-b57d-481c-ab6f-647d989799cf"), "II.108. Шашки", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("3f9f1d9a-a336-4fc0-a171-5ea1c37a0798"), "II.109. Шотокан карате-до С. К. І. Ф", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("81ad6300-4089-4e12-8677-cbb38e9f4f49"), "II. 110. Практична стрільба", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("744724b5-a5d2-4138-b455-d4fe2df6d6ff"), "II.111. Кйокушінкаі карате унія", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("57590463-e587-4f1e-b8e7-fc26cd8b0338"), "II.112. Спорт з літаючим диском", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("b95614c7-11e2-49dc-ab42-e31a93a7cb4e"), "II.113. Середньовічний бій", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("bc38035c-b825-4cb2-aa14-2a1946be5a7c"), "II.114. Кікбоксинг \"ІСКА", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("4aeae19a-f14b-4953-8d86-c9a3e47bd7de"), "II.115. Комбат самозахист ІСО", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("d39fc549-6b0a-4362-a72a-bf441c6ce9b7"), "II.119. Спортивний покер", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("2d74f28b-9d99-40d7-b606-7455aeb4922f"), "II.121. Кануполо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("51ac3662-510d-4a8e-a437-3ad0b8a191e1"), "II.122. Кіберспорт (електронний спорт)", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("241b0084-8ac6-449c-a337-a13fa4a7cad1"), "II.123. Кіокушин БуДо карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("1d86822b-278f-4c5a-9dda-cdeb35a18f8d"), "II. 124. Поло", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("8da01648-1dbf-424c-af7a-00486d5a747b"), "II. 125. Таеквон-До", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("aec9b975-7496-40df-9b1d-9302f23557c1"))
, (UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"), "Види спорту осіб з інвалідністю з ураженням опорно-рухового апарату, вадами зору, слуху та розумового і фізичного розвитку", 1, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), null)
, (UUID_TO_BIN("a07603cf-6709-4dcd-ab4d-851087c26e67"), "III.001. Армспорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("6c41c50c-ccb2-432b-ab91-f2d276d11a93"), "III.002. Бадмінтон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("5a9692cc-8196-40bc-aa7a-b2c589189ab4"), "III.003. Баскетбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("c4a1333e-a8f8-40bd-9316-68ca0de106f5"), "III.004. Баскетбол на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("5fdb0fca-b09d-4405-8cb8-17b79c3a2e64"), "III.005. Біатлон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("e5d7ac89-727d-455a-bebe-39b6b9a095a5"), "III.006. Більярдний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("015454d4-dda4-4902-9ed7-7dd9291e9596"), "III.007. Боротьба вільна", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("08110419-9e08-4057-b86f-d178ad33c88e"), "III.008. Боротьба греко-римська", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("9eb64951-8133-470a-a069-c8ee9252635a"), "III.009. Боулінг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("a59bd8d6-615d-42aa-9fa7-41ed1c45229b"), "III.010. Бочча", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("dd3c40ff-ef8c-4d97-ab51-056a3371a8ca"), "III.011. Велосипедний спорт-трек", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("fc4ce3f4-e08c-41a5-8a7a-ff79593204f2"), "III.012. Велосипедний спорт-шосе", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("6a8c2f08-2d4f-467d-b31a-f767e2d85bab"), "III.013. Веслування академічне", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("4c7c84c0-6504-424f-aa78-3f074c32e08e"), "III.014. Вітрильний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("617e348e-daa8-434d-8ba8-ebf1cabb3cf8"), "III.015.Водне поло", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("fe474ce7-43b8-4865-aca0-4a84170ac5a7"), "III.016. Волейбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("b6365aba-a293-43fa-b651-dc8727fd7fac"), "III.017. Волейбол сидячи", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("d66a44aa-dbdc-4a0d-9d5c-85a0767ff107"), "III.018. Гандбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("4ea8faff-ca50-4a67-96e8-f6944b4115f9"), "III.019. Гімнастика спортивна", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("9a9741d8-73b2-430d-904f-5f56c1ea7fef"), "III.020. Гімнастика художня", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("92d8783f-df9a-4e1e-b421-aa195aa9df85"), "III.021. Гірськолижний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("1ba99a66-c064-4dc2-9de2-9df628a0d96b"), "III.022. Голбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("72890156-6cd4-428f-b7b0-0adc701ab07f"), "III.023. Дзюдо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("a0912901-c22e-4e22-ac7c-90e9a707f430"), "III.024. Карате", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("69a236d9-93f4-4c64-9d46-e04e13f00d95"), "III.025. Керлінг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("8424f4cb-7841-4c9b-aa71-983b50f4fbbc"), "III.026. Керлінг на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("9db6fd27-6167-4f34-a7fd-a59208401cc4"), "III.027. Кінний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("f9eee3c8-a64b-4525-9503-3bc6dda1feeb"), "III.028. Ковзанярський спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("6b730643-8043-443b-91e4-e32c51eed76b"), "III.029. Легка атлетика", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("d98e2ac4-8065-4f26-bd54-2b82838855ed"), "III.030. Лижні перегони", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("1b3fbfca-3e22-4a92-91d5-fe49615e71fc"), "III.031. Параканое", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("26335b3c-906c-4307-bca3-d06f35289955"), "III.032. Паратриатлон", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("380d0905-5528-44c8-bcd8-03f407a0f30a"), "III.033. Пауерліфтинг", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("86b93ffe-fd87-4135-813e-f5dfc08af106"), "III.034. Плавання", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("dcc189fa-5f1d-4135-8d9d-7be154e1cbc8"), "III.035. Пляжний волейбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("43bdea3e-6136-41e9-8020-614731615927"), "III.036. Регбі на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("5069db38-5bc5-4ec7-8cfd-b3044c3fb7fb"), "III.037. Риболовний спорт", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("2cb31fc7-b8c2-44a9-a356-f7a5a4218c42"), "III.038. Спортивне орієнтування", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("3458b642-c548-4d9d-b9a4-03b3359d34cb"), "III.039. Спортивний туризм", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("c3ea7590-1372-431d-98f9-7b6c02020138"), "III.040. Спортивні танці на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("292b31f0-2dd0-48e0-b2a2-d90765aaae42"), "III.041. Стрільба з лука", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("fe5a7391-b978-40de-aa58-b8ee5f2ba544"), "III.042. Стрільба кульова", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("1359f96b-e3e3-414b-abf9-c32f5d4ab310"), "III.043. Теніс", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("99f3eb92-3450-430a-828a-1f263e09b99f"), "III.044. Теніс на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("26ba9f75-58c5-46c6-ad1d-4ec1b5739dfb"), "III.045. Теніс настільний", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("b33d4f5c-f754-4fc5-bf39-5a4870203d60"), "III.046. Тхеквондо", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"));

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("d22a31f3-20cc-482c-8262-77d439a9ef01"), "III.047. Фехтування на візках", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("c82f8bca-da9c-48b4-ad00-eb4f1b3d9529"), "III.048. Футбол", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("c277778d-a289-4179-a244-6a75e2a2ddee"), "III.049. Футзал", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("69851663-7439-4076-89b2-80612dbd593c"), "III.050. Шахи", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("5c0aed15-f34f-48d8-a0c5-e0994c19b776"), "III.051. Шашки", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("9e6eef40-926c-4dd5-b487-3035a32e5cff"), "III. 052. Гольф.", 2, UUID_TO_BIN("b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a"), UUID_TO_BIN("46b15ec2-0f71-4e2c-8779-80b195eb1c3c"))
, (UUID_TO_BIN("7b2b2002-ef3e-46b5-acdc-0f7374731d6c"), "Наука та техніка", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("20be3b0a-18db-453c-a61c-f897aa4ae701"), "IT, Програмування", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("b9e57678-9ece-44dd-9493-4d35aaf2bc85"), "Фотогуртки", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("1be53008-36e2-4cd1-8485-12b3e88b454c"), "Конструювання", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null);

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("0d7bf3be-1d4e-4954-9b94-d2f2f862fd26"), "Музика", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("fe27c0c6-63de-4f18-9b25-164359ea5989"), "Співи", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("c74dd0f1-0833-4026-89ce-4b26ce5af821"), "Танці", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("0fa5e477-a309-45d1-a559-b7863f32a983"), "Театр, цирк та кіно", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("6e7cc6f2-fd46-4ef0-833e-6e5724ccdb8f"), "Малювання", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("be6b696d-519e-40fe-981a-2b703d1dee49"), "Рукоділля", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("53b5e699-6f62-41fb-92da-a006d401d3f3"), "Садівництво", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("a20b3f3c-a407-4549-ba7b-5a3d05bcc756"), "Екологія", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("52ef1343-2165-4e16-b4dd-323b09139e72"), "Туризм", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("a3f94ee8-be43-49b7-a3f9-bdc649428f2c"), "Інші", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null);

INSERT INTO institutionhierarchies (Id, Title, HierarchyLevel, InstitutionId, ParentId)
VALUES (UUID_TO_BIN("255d6a11-59e2-48bc-aa4e-949fe84d5240"), "Наука та практичні досліди", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("f75fb6c6-ae30-4057-b321-28e9461549e2"), "Пласт/Скаутинг", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("44c17375-2864-46b1-8513-3909e6a1ae5b"), "Спорт", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("930d2206-b1d1-40ac-9984-306a0ed342d2"), "Книжки та про них", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("401cdda6-5e20-41ed-af6d-2862b0cef353"), "Реабілітація", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("a70accb3-041f-47e0-ab39-122c2f640127"), "Оздоровлення", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null)
, (UUID_TO_BIN("9335a91e-5d6c-4069-ba74-61a33a15be6b"), "Мови/Гуманітарій", 1, UUID_TO_BIN("c301afc9-585d-4a4b-b2d3-a1e05d18aeb5"), null);


#directions
INSERT INTO directions (Title, Description) 
SELECT "Наука та техніка", "Наука та техніка" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Наука та техніка" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "IT, Програмування", "IT, Програмування" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "IT, Програмування" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Фотогуртки", "Фотогуртки" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Фотогуртки" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Рукоділля", "Рукоділля" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Рукоділля" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Конструювання", "Конструювання" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Конструювання" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Музика", "Музика" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Музика" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Співи", "Співи" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Співи" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Танці", "Танці" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Танці" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Театр, цирк та кіно", "Театр, цирк та кіно" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Театр, цирк та кіно" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Малювання", "Малювання" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Малювання" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Театр, цирк та кіно", "Театр, цирк та кіно" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Театр, цирк та кіно" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Фотогуртки", "Фотогуртки" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Фотогуртки" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Садівництво", "Садівництво" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Садівництво" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Екологія", "Екологія" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Екологія" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Туризм", "Туризм" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Туризм" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Інші", "Інші" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Інші" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Наука та практичні досліди", "Наука та практичні досліди" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Наука та практичні досліди" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Пласт/Скаутинг", "Пласт/Скаутинг" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Пласт/Скаутинг" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Спорт", "Спорт" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Спорт" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Книжки та про них", "Книжки та про них" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Книжки та про них" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Реабілітація", "Реабілітація" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Реабілітація" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Оздоровлення", "Оздоровлення" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Оздоровлення" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Мови/Гуманітарій", "Мови/Гуманітарій" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Мови/Гуманітарій" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Музика", "Музика" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Музика" LIMIT 1);

INSERT INTO directions (Title, Description) 
SELECT "Фотогуртки", "Фотогуртки" 
WHERE NOT EXISTS (SELECT * FROM directions 
      WHERE Title = "Фотогуртки" LIMIT 1);


#directioninstitutionhierarchy

set @directionId = (select Id from directions where Title = "Наука та техніка" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("7f09379f-b7e4-41df-b687-a2addd279077"))
, (@directionId, UUID_TO_BIN("d2402797-d91a-4a73-ad30-0602ad0dd77c"))
, (@directionId, UUID_TO_BIN("7e2c9407-868c-42a1-874e-2504a06b8837"))
, (@directionId, UUID_TO_BIN("7b2b2002-ef3e-46b5-acdc-0f7374731d6c"));

set @directionId = (select Id from directions where Title = "IT, Програмування" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("f5aa174f-4c30-4b29-859f-f6ba40417d73"))
, (@directionId, UUID_TO_BIN("20be3b0a-18db-453c-a61c-f897aa4ae701"));

set @directionId = (select Id from directions where Title = "Фотогуртки" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("94e0d2b0-7173-4f5e-9f48-8e40cad3d037"));

set @directionId = (select Id from directions where Title = "Рукоділля" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("94e0d2b0-7173-4f5e-9f48-8e40cad3d037"))
, (@directionId, UUID_TO_BIN("2181c940-66ed-426a-9062-438dcd361358"))
, (@directionId, UUID_TO_BIN("cfb89ff8-bd2b-44fd-875f-821ff8bac40d"))
, (@directionId, UUID_TO_BIN("0e817417-65af-4e2c-99d3-2a1a1229400f"))
, (@directionId, UUID_TO_BIN("e7abfc2a-2cd2-4586-8c17-480ec0d38459"))
, (@directionId, UUID_TO_BIN("0f942db0-0d5c-4874-809a-670d4d288738"))
, (@directionId, UUID_TO_BIN("4d91ade2-82d5-4842-9c33-b07fecce76ec"))
, (@directionId, UUID_TO_BIN("d6524e40-0d62-4ac5-8ba4-148a9578f0e9"))
, (@directionId, UUID_TO_BIN("3269f99f-e34f-4093-804c-876a3617ff35"))
, (@directionId, UUID_TO_BIN("37c4bd36-ebb8-450d-a9fc-f2f4c409aea2"))
, (@directionId, UUID_TO_BIN("2baede05-11de-4cb5-9d56-f076b5e7140d"))
, (@directionId, UUID_TO_BIN("a6d8f5ba-c4de-4458-bad0-44d8ca80babc"))
, (@directionId, UUID_TO_BIN("19d3ad03-ca92-4ab4-8b65-ea66a4b658e6"))
, (@directionId, UUID_TO_BIN("4afb4f54-3d18-47a7-adb3-9c70acf50798"))
, (@directionId, UUID_TO_BIN("134f8851-28e4-4119-b33a-5345d0f1ca2b"))
, (@directionId, UUID_TO_BIN("48aaba2f-da50-4b35-b165-0738a73d2e2a"))
, (@directionId, UUID_TO_BIN("a111a3a7-79f9-47cc-913b-bb74ef0070b5"))
, (@directionId, UUID_TO_BIN("56d1b016-5808-4a6f-949f-6102e3c14e0b"))
, (@directionId, UUID_TO_BIN("be6b696d-519e-40fe-981a-2b703d1dee49"));

set @directionId = (select Id from directions where Title = "Конструювання" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("c6188362-ef6a-42ff-b0e0-118886e4207d"))
, (@directionId, UUID_TO_BIN("1be53008-36e2-4cd1-8485-12b3e88b454c"));

set @directionId = (select Id from directions where Title = "Музика" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("4c431342-9691-4203-85b3-a9dedbba207b"))
, (@directionId, UUID_TO_BIN("666d5fd8-ce1b-4ece-ae94-0670a3747274"))
, (@directionId, UUID_TO_BIN("6f4fbca9-9588-4aa4-8735-1e4759b8cf0c"))
, (@directionId, UUID_TO_BIN("e7be108c-e344-453a-905d-84edacc36568"))
, (@directionId, UUID_TO_BIN("69a88ac6-ee07-4ea7-bd00-604e3907f073"))
, (@directionId, UUID_TO_BIN("47c3b40a-edd0-4714-939a-821eb47619ce"))
, (@directionId, UUID_TO_BIN("4dd8ae2c-41ac-48f5-9bcb-e10c9cea94bd"))
, (@directionId, UUID_TO_BIN("8b5d69f0-646c-46a4-8ef3-d2769a4de7f0"))
, (@directionId, UUID_TO_BIN("0b033e3a-d6b3-4ac0-aad1-0cc45ce2eb50"))
, (@directionId, UUID_TO_BIN("0f8748d3-2ccd-43b2-b622-6e20ebdadf06"))
, (@directionId, UUID_TO_BIN("bdebd328-f63f-4f43-adc2-a7c4cad22ded"))
, (@directionId, UUID_TO_BIN("5e531788-e7d7-4899-80cb-b3f696c42707"))
, (@directionId, UUID_TO_BIN("6e4bf5e4-e133-4858-b8f1-c33d51cb55d8"))
, (@directionId, UUID_TO_BIN("3d56ebd7-cf63-4a9b-9254-1468fa957b0d"))
, (@directionId, UUID_TO_BIN("f871708b-5e1e-490f-9efa-0161017bf5d8"))
, (@directionId, UUID_TO_BIN("17257809-984f-47f3-a9ed-7779130ce2a5"))
, (@directionId, UUID_TO_BIN("ee63b10b-b2bb-486d-81fe-984b22868ea0"))
, (@directionId, UUID_TO_BIN("19677ade-4755-4a09-b7ee-c8f1f94992a3"))
, (@directionId, UUID_TO_BIN("b2b386a5-e9bf-48b7-91d1-16c38c4c45b5"))
, (@directionId, UUID_TO_BIN("283f7e96-a465-4ec6-8903-87518574c261"))
, (@directionId, UUID_TO_BIN("f020ce2c-2b8a-4780-ba58-162690cfeefa"))
, (@directionId, UUID_TO_BIN("0d7bf3be-1d4e-4954-9b94-d2f2f862fd26"));

set @directionId = (select Id from directions where Title = "Співи" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("63c3ec8a-55fc-49ce-9e7a-9c5dec450ca6"))
, (@directionId, UUID_TO_BIN("ab6d0a84-ddbc-4c51-bfff-9842732fbc41"))
, (@directionId, UUID_TO_BIN("0f172033-98ad-4382-9543-e2a79b238431"))
, (@directionId, UUID_TO_BIN("fdd4f225-da2a-42b9-8385-28833474613f"))
, (@directionId, UUID_TO_BIN("61e41b37-a867-4b59-afca-0326c94ccbfd"))
, (@directionId, UUID_TO_BIN("fe27c0c6-63de-4f18-9b25-164359ea5989"));

set @directionId = (select Id from directions where Title = "Танці" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("efca2ed9-5bdb-445c-a837-09e42978c1b4"))
, (@directionId, UUID_TO_BIN("fe329584-1bdf-40ce-9aa5-5b06f870bb2b"))
, (@directionId, UUID_TO_BIN("fe9b3c8a-870c-4c73-b73c-08fee994f82a"))
, (@directionId, UUID_TO_BIN("1ce025e2-8784-4884-a780-e68b21f0586d"))
, (@directionId, UUID_TO_BIN("c62aa157-3583-4da8-a4f9-85fc7aa6d361"))
, (@directionId, UUID_TO_BIN("c74dd0f1-0833-4026-89ce-4b26ce5af821"));

set @directionId = (select Id from directions where Title = "Театр, цирк та кіно" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("fad1e126-1e13-4e2a-b381-083ef1d1cbd1"))
, (@directionId, UUID_TO_BIN("2ac82b6b-0a58-4d1a-9121-3183b895b8cb"))
, (@directionId, UUID_TO_BIN("314f234e-ff39-45c9-b876-9f3921250183"))
, (@directionId, UUID_TO_BIN("2fe73c9a-7b71-4e10-8447-7ad16f4e1725"))
, (@directionId, UUID_TO_BIN("cb78521b-6fef-484b-ae09-9c612603a68a"))
, (@directionId, UUID_TO_BIN("a87003a3-9ffc-487f-8abc-1c59bae69e73"))
, (@directionId, UUID_TO_BIN("f39340bf-6469-4a51-9322-fa6eeb98c759"))
, (@directionId, UUID_TO_BIN("978fa8f7-9c08-4ed8-ac0e-30232e342734"))
, (@directionId, UUID_TO_BIN("0fa5e477-a309-45d1-a559-b7863f32a983"));

set @directionId = (select Id from directions where Title = "Малювання" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("f0a2d774-b2d6-4afa-89f9-9640a44904f0"))
, (@directionId, UUID_TO_BIN("dd574fcc-d843-48e8-8768-6a398984c53c"))
, (@directionId, UUID_TO_BIN("8614e19c-ea71-426d-9c7d-2bdee150d7a6"))
, (@directionId, UUID_TO_BIN("6e7cc6f2-fd46-4ef0-833e-6e5724ccdb8f"));

set @directionId = (select Id from directions where Title = "Театр, цирк та кіно" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("3b25a026-0a48-44cb-b0f9-6168c24f098c"));

set @directionId = (select Id from directions where Title = "Фотогуртки" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("3b25a026-0a48-44cb-b0f9-6168c24f098c"));

set @directionId = (select Id from directions where Title = "Садівництво" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("35303c62-27ba-4761-9747-1dd6ed28fc10"))
, (@directionId, UUID_TO_BIN("53b5e699-6f62-41fb-92da-a006d401d3f3"));

set @directionId = (select Id from directions where Title = "Екологія" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("ed2fe80a-8955-497e-98ac-ea88558bb74c"))
, (@directionId, UUID_TO_BIN("a20b3f3c-a407-4549-ba7b-5a3d05bcc756"));

set @directionId = (select Id from directions where Title = "Туризм" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("c79667b2-9c8b-4f43-8a4b-f961b2ca61b8"))
, (@directionId, UUID_TO_BIN("4ffa1b28-2784-49a6-b8e8-b6387306f6f6"))
, (@directionId, UUID_TO_BIN("52ef1343-2165-4e16-b4dd-323b09139e72"));

set @directionId = (select Id from directions where Title = "Інші" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("d5b90591-887f-4c83-9d18-dba4894f212b"))
, (@directionId, UUID_TO_BIN("a3f94ee8-be43-49b7-a3f9-bdc649428f2c"));

set @directionId = (select Id from directions where Title = "Наука та практичні досліди" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("7623de2f-dd39-4856-a09d-b026a4ae2c85"))
, (@directionId, UUID_TO_BIN("aad6f415-5601-4f1b-980b-1098e1f3a935"))
, (@directionId, UUID_TO_BIN("b31ce691-600d-420e-959a-0da65316a908"))
, (@directionId, UUID_TO_BIN("5056cb1e-20b0-4990-a12e-6220c34c75e4"))
, (@directionId, UUID_TO_BIN("c0a7b94d-806c-4115-b0c6-1675018c59d0"))
, (@directionId, UUID_TO_BIN("ecaca89b-594c-4c50-9bfb-2146ee9c45cd"))
, (@directionId, UUID_TO_BIN("3f25362b-2a9e-4852-9708-2012b0cc1385"))
, (@directionId, UUID_TO_BIN("5c536b66-9922-4f87-9650-6bae88bf7c66"))
, (@directionId, UUID_TO_BIN("89d4ab3a-22f4-4093-9645-82381c8a138c"))
, (@directionId, UUID_TO_BIN("939169da-fb61-49f5-9c60-0eac4c6fd68d"))
, (@directionId, UUID_TO_BIN("255d6a11-59e2-48bc-aa4e-949fe84d5240"));

set @directionId = (select Id from directions where Title = "Пласт/Скаутинг" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("3ca6616c-5ff1-4052-9454-5ce5d9f59ba7"))
, (@directionId, UUID_TO_BIN("f76424af-3c6f-4fc6-b675-9dbb86b4bc31"))
, (@directionId, UUID_TO_BIN("f75fb6c6-ae30-4057-b321-28e9461549e2"));

set @directionId = (select Id from directions where Title = "Спорт" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("fe39520b-46af-4fd3-8e6d-c3441260d2ea"))
, (@directionId, UUID_TO_BIN("c8ebcd0f-c20d-4c55-84c4-b2d23c474bb3"))
, (@directionId, UUID_TO_BIN("65abc134-4618-4874-815a-72f878245213"))
, (@directionId, UUID_TO_BIN("3be71038-5a18-41a6-b266-5f265cd0ab41"))
, (@directionId, UUID_TO_BIN("41201d1c-7a05-468e-af6b-65f1cc07ae58"))
, (@directionId, UUID_TO_BIN("8d1bd401-1197-4c08-b8b0-18e7a21d63a3"))
, (@directionId, UUID_TO_BIN("1b1014b1-3ce0-4a89-9b99-ceacbdbfed6b"))
, (@directionId, UUID_TO_BIN("584b1689-88b1-4f02-a127-82c57abc3604"))
, (@directionId, UUID_TO_BIN("67c47f55-8ced-4bbd-a3ab-583187c489cf"))
, (@directionId, UUID_TO_BIN("4fc52d5c-2ede-4302-924c-fbcf802e7b78"))
, (@directionId, UUID_TO_BIN("928da7bd-3a8e-429c-9ca1-50c3e7741ca7"))
, (@directionId, UUID_TO_BIN("782d5ce3-468d-4745-a0d4-49c7f440c8b2"))
, (@directionId, UUID_TO_BIN("85f8778f-57d9-4111-a78b-9dd501474e3b"))
, (@directionId, UUID_TO_BIN("b6d887a0-7d72-4479-acec-13376e7e3d48"))
, (@directionId, UUID_TO_BIN("594c3a85-cf4a-4d32-94b1-99e5d60c83f1"))
, (@directionId, UUID_TO_BIN("114eec08-32db-464d-a235-a01ec7ee80b6"))
, (@directionId, UUID_TO_BIN("9436442b-0bbc-4334-a4e4-94b16c9799a3"))
, (@directionId, UUID_TO_BIN("d7a30a2e-1b1a-46b0-96c5-e372b6667687"))
, (@directionId, UUID_TO_BIN("6c509e8a-e93a-4cae-acb8-f3391eee0346"))
, (@directionId, UUID_TO_BIN("25c0f3b2-1b9e-4b16-86e5-3867cc21742d"))
, (@directionId, UUID_TO_BIN("a685f2bb-e6ff-4bd5-bfd2-bb5244e004ef"))
, (@directionId, UUID_TO_BIN("81177045-33ca-43f9-8ba8-5c788e44ab12"))
, (@directionId, UUID_TO_BIN("05b1d8af-c34e-46f7-bd24-26d6b2b9c871"))
, (@directionId, UUID_TO_BIN("3301553d-c2ef-41c0-b833-8e23f7926ca4"))
, (@directionId, UUID_TO_BIN("80093681-86b5-4035-af67-a6887fe654d2"))
, (@directionId, UUID_TO_BIN("b77120b1-74fb-4a71-80c9-53e124cdac50"))
, (@directionId, UUID_TO_BIN("1159305c-77cc-406b-94c1-da6f5782317f"))
, (@directionId, UUID_TO_BIN("f300633c-9248-4ce7-82f4-f4ea57c0e6b4"))
, (@directionId, UUID_TO_BIN("b00d7436-3308-40b6-8cbf-9f6ffe4cc18c"))
, (@directionId, UUID_TO_BIN("a5518b57-2375-4438-97f9-dc7a748c3fa3"))
, (@directionId, UUID_TO_BIN("02da382a-14fd-4f0b-a09b-dcead6c9578f"))
, (@directionId, UUID_TO_BIN("ce7fccfa-6a8b-42b3-8b13-d5d615c0b684"))
, (@directionId, UUID_TO_BIN("d1676510-7f43-441e-b867-72bfc6f8f5e9"))
, (@directionId, UUID_TO_BIN("50eb3503-aaaa-4459-b944-f819a280af47"))
, (@directionId, UUID_TO_BIN("858d89bf-5101-42ab-8ee1-c6a5f9e37085"))
, (@directionId, UUID_TO_BIN("7b820d6e-bd3e-406d-bd22-18c247b72b65"))
, (@directionId, UUID_TO_BIN("ca97d65a-bcc1-4aa3-af32-eaa5bfc41891"))
, (@directionId, UUID_TO_BIN("953c0104-94ec-422d-9bcd-3889bc9cb331"))
, (@directionId, UUID_TO_BIN("fa978978-5a3a-412f-ac1b-f2fd9e4d697e"))
, (@directionId, UUID_TO_BIN("3ed0d9ae-8e69-4029-8a2a-543b7fa85101"))
, (@directionId, UUID_TO_BIN("c290c7f8-d69d-4183-acaf-f2723e8c3966"))
, (@directionId, UUID_TO_BIN("2c8352d9-2920-4984-bf1a-ab6023f25883"))
, (@directionId, UUID_TO_BIN("4a0286e8-2682-4e10-a10e-82564f46e40b"))
, (@directionId, UUID_TO_BIN("1d4011fa-fee5-4015-8ced-0d4b13fa38c3"))
, (@directionId, UUID_TO_BIN("2a796681-f624-4981-9084-05cb60e1ddc6"))
, (@directionId, UUID_TO_BIN("f0d7d048-6bbc-478e-b3ec-a4557e44e371"))
, (@directionId, UUID_TO_BIN("a8a25d1b-5ae4-4743-aee9-72428c1ad47c"))
, (@directionId, UUID_TO_BIN("26786218-ed5d-4c55-b3e5-5c87e9cb4d4b"))
, (@directionId, UUID_TO_BIN("c477c51c-6e4c-4d79-8cd8-9d403146f33f"))
, (@directionId, UUID_TO_BIN("ad5a4b6c-4776-4d09-9484-b8485c651c76"))
, (@directionId, UUID_TO_BIN("f97c6a3d-df4b-47e9-935e-0133be723a13"))
, (@directionId, UUID_TO_BIN("092d4b9a-5d96-47ff-9ea8-04aae028a6a8"))
, (@directionId, UUID_TO_BIN("f7ecb95b-b443-451f-85a9-7fa5006b65eb"))
, (@directionId, UUID_TO_BIN("5c9b9d40-fb45-4a99-98d3-b66037aa8d0f"))
, (@directionId, UUID_TO_BIN("cc9acf94-b272-4094-af5a-f4a6f5ec0c10"))
, (@directionId, UUID_TO_BIN("2a5c518f-f1c2-47be-9ddd-e0532af6278e"))
, (@directionId, UUID_TO_BIN("a5c0e0c0-5688-4459-9996-1540828cd4ca"))
, (@directionId, UUID_TO_BIN("e3e24f68-47c1-41ec-875b-3102c7131485"))
, (@directionId, UUID_TO_BIN("2da20dfd-c853-4329-a0c3-188249398c08"))
, (@directionId, UUID_TO_BIN("ff7fbdcc-d7cd-4ea3-88c0-622942043262"))
, (@directionId, UUID_TO_BIN("46a503fa-aed4-4ae7-abc5-ebe2a63fb86d"))
, (@directionId, UUID_TO_BIN("60f11f21-5d9a-401b-b233-b34f798ce9a0"))
, (@directionId, UUID_TO_BIN("fcc5c739-6b37-4416-9eb4-b7e4240a862a"))
, (@directionId, UUID_TO_BIN("105c0aaf-ad98-4a6d-8052-dab72165a653"))
, (@directionId, UUID_TO_BIN("42def3b2-62c8-4fad-a2f0-8ac93a4a36a5"))
, (@directionId, UUID_TO_BIN("e922728e-d141-4826-b697-bf16ce1dce88"))
, (@directionId, UUID_TO_BIN("b6fa31c1-f314-468e-8c70-b190e8904e95"))
, (@directionId, UUID_TO_BIN("d84aa35d-4a88-46e0-916a-9284fec54c1d"))
, (@directionId, UUID_TO_BIN("759f6fc5-84b9-403d-8a85-219a6c3ed3a8"))
, (@directionId, UUID_TO_BIN("60dfcf3a-5b1a-40b4-b4f7-fcbf42ea1fda"))
, (@directionId, UUID_TO_BIN("e281beff-97e7-4d94-b7a2-9da5470265a4"))
, (@directionId, UUID_TO_BIN("3f136236-b9e5-4a6c-a770-d9325a98b504"))
, (@directionId, UUID_TO_BIN("1e5f4fd5-cc86-45c6-a55e-dba79d968ddd"))
, (@directionId, UUID_TO_BIN("1192c69b-50aa-4ae0-ba48-c8ac7685c38f"))
, (@directionId, UUID_TO_BIN("d8d89ab0-78df-4ae5-b77f-52664d876273"))
, (@directionId, UUID_TO_BIN("0cee42fe-cca0-4b20-a4cf-0587c013ab83"))
, (@directionId, UUID_TO_BIN("fd7d9506-3829-49c3-abf2-74e3a6e8cb54"))
, (@directionId, UUID_TO_BIN("3ff63798-7d69-4c4a-981c-f49a4ce47b85"))
, (@directionId, UUID_TO_BIN("961064de-8e43-4a48-9e75-98f9c687cf05"))
, (@directionId, UUID_TO_BIN("dc613473-4e03-41be-a1d5-e68272f24729"))
, (@directionId, UUID_TO_BIN("aca590a1-160a-4627-b2a4-dcccb709b047"))
, (@directionId, UUID_TO_BIN("d6126815-27eb-4203-818a-432d03caa266"))
, (@directionId, UUID_TO_BIN("dcc574cb-0314-41c2-bc03-b8001be86ef0"))
, (@directionId, UUID_TO_BIN("18aa2391-f09e-42f2-a16c-2d004645b657"))
, (@directionId, UUID_TO_BIN("2e027f6e-f786-4e71-b7f3-ee8976466285"))
, (@directionId, UUID_TO_BIN("4d320ec7-7b22-46be-8401-6de5d32de115"))
, (@directionId, UUID_TO_BIN("9a4563e5-39c7-475d-bcf8-b5a8b794e5c0"))
, (@directionId, UUID_TO_BIN("3e9be0ad-509f-4253-a4f4-e4a7b9b4394d"))
, (@directionId, UUID_TO_BIN("782461d1-e6c2-43cb-8d04-b903d399f5f0"))
, (@directionId, UUID_TO_BIN("0eeae908-f972-4e50-b37f-3ae4dae4206c"))
, (@directionId, UUID_TO_BIN("7be8ad85-26f7-4cd3-83ab-c41f4df468e7"))
, (@directionId, UUID_TO_BIN("5614469d-8951-4353-a004-e434450f1862"))
, (@directionId, UUID_TO_BIN("baa87fc6-16c2-4a87-9921-d70717e91e6c"))
, (@directionId, UUID_TO_BIN("111a3802-e6e4-4804-a66e-c1c2a5ecd807"))
, (@directionId, UUID_TO_BIN("6a3dea32-d972-449d-9f02-40c71bf07ff1"))
, (@directionId, UUID_TO_BIN("0326cd18-16d9-4fc5-9ba5-04ee7d096fb5"))
, (@directionId, UUID_TO_BIN("5b2a27ba-b078-4355-81b0-4145f7dadc00"))
, (@directionId, UUID_TO_BIN("e44978a7-1cc2-4ab4-ba3e-90b46ba370db"))
, (@directionId, UUID_TO_BIN("f2a38a41-83bf-46a9-86db-3d821bb6217d"))
, (@directionId, UUID_TO_BIN("dccc2a58-59b4-4e6b-a8ce-9c716cf539b1"))
, (@directionId, UUID_TO_BIN("4d541618-ea14-4ea6-afef-744187064472"))
, (@directionId, UUID_TO_BIN("0358b30d-9888-4eab-af8b-dcb34dae0b3e"))
, (@directionId, UUID_TO_BIN("3ab484f2-d73e-42e7-8569-ad6018925840"))
, (@directionId, UUID_TO_BIN("f4353e5d-0861-4696-ba11-9c43d0fb6a1b"))
, (@directionId, UUID_TO_BIN("9596670f-0311-42fa-a8c2-e75c7818182b"))
, (@directionId, UUID_TO_BIN("c8ecb3ee-8fb8-4151-b12d-40206840b33a"))
, (@directionId, UUID_TO_BIN("a561e2b6-d939-4e22-b7bb-7d34870f11f9"))
, (@directionId, UUID_TO_BIN("3773e2ba-0d84-4139-b1a7-9299c640660c"))
, (@directionId, UUID_TO_BIN("f7fb164e-4aad-4127-8850-d8dfc2fd2129"))
, (@directionId, UUID_TO_BIN("e9fa01d9-e7f2-494d-a527-29d25dc75afb"))
, (@directionId, UUID_TO_BIN("256e3301-c0f9-4235-bce3-1ddcc636ea07"))
, (@directionId, UUID_TO_BIN("c0a992e2-8425-419c-903c-2fd14cb02e97"))
, (@directionId, UUID_TO_BIN("69e694a2-47a7-4fc0-8c56-61514ae1de25"))
, (@directionId, UUID_TO_BIN("06b5671f-155c-4cc1-90fb-011a01f585fd"))
, (@directionId, UUID_TO_BIN("9372fa15-561c-47f3-842c-afe04330fb8b"))
, (@directionId, UUID_TO_BIN("49e64abd-d9c8-430f-895c-b2a237422af4"))
, (@directionId, UUID_TO_BIN("2679c50d-751e-418e-975b-4e7eb7e44064"))
, (@directionId, UUID_TO_BIN("2448debd-8ad9-451b-9abf-50bf2d9d15e2"))
, (@directionId, UUID_TO_BIN("465da170-3c79-4803-b272-544aa9ca1f0e"))
, (@directionId, UUID_TO_BIN("2a5123d5-082d-499b-ad89-cd700e6e6e44"))
, (@directionId, UUID_TO_BIN("4d4c6083-8f09-47b3-a5e0-49af6eeb102d"))
, (@directionId, UUID_TO_BIN("7e4b55a6-e0d9-4488-9ee1-a70a016a11c3"))
, (@directionId, UUID_TO_BIN("bf7594c6-02d9-4fd2-9ba3-2a76533f3440"))
, (@directionId, UUID_TO_BIN("dcd328da-f00c-4a13-825f-df50fe94ba93"))
, (@directionId, UUID_TO_BIN("65888d45-982b-49a8-b9aa-97fcf92abf90"))
, (@directionId, UUID_TO_BIN("685bc875-99da-488e-bc10-2fbcba976b42"))
, (@directionId, UUID_TO_BIN("063b130b-7a0f-4d26-91db-8bae16405f0f"))
, (@directionId, UUID_TO_BIN("b9f401b6-1975-42c9-94bf-a125fd76af30"))
, (@directionId, UUID_TO_BIN("ea99b1ea-0f9d-4a99-b172-32bb6624b6f5"))
, (@directionId, UUID_TO_BIN("627c7489-c7df-49e5-9447-dfbfbc53706b"))
, (@directionId, UUID_TO_BIN("64172b5a-afcf-4649-8a06-980c956e13ca"))
, (@directionId, UUID_TO_BIN("5f37f658-681b-42c2-83da-ea4970166ea3"))
, (@directionId, UUID_TO_BIN("47ccd2ab-8b0c-4ce6-bc82-3d32128da08d"))
, (@directionId, UUID_TO_BIN("c993e2ef-1bf2-4893-b074-6a4c1901da65"))
, (@directionId, UUID_TO_BIN("a7eb2e09-a706-477a-a09f-41480b1e7b34"))
, (@directionId, UUID_TO_BIN("c5e3e82e-6cda-47e1-bb83-aa5d40d0dc43"))
, (@directionId, UUID_TO_BIN("bf8db5cc-5a96-4898-bc03-825e00cb640e"))
, (@directionId, UUID_TO_BIN("644f9b0b-883d-41ff-98f2-27568bc79c00"))
, (@directionId, UUID_TO_BIN("972b7558-a935-4dfc-9fe5-d13060bbeb54"))
, (@directionId, UUID_TO_BIN("97b46647-acac-4a6e-ac3c-47aae0e966f0"))
, (@directionId, UUID_TO_BIN("c6b9d359-059f-440c-b15a-4d874790a94b"))
, (@directionId, UUID_TO_BIN("80ae816e-aa2e-4a18-8988-0289e1216312"))
, (@directionId, UUID_TO_BIN("4e1fff27-8f4c-431a-81f2-9e68f16aa079"))
, (@directionId, UUID_TO_BIN("75b60f2b-5285-46a8-9068-ba12a51951ad"))
, (@directionId, UUID_TO_BIN("d4ab6c7b-9153-498c-9f70-bb238581d811"))
, (@directionId, UUID_TO_BIN("b97f65e8-03c0-4235-b13c-0a87a74aa1a3"))
, (@directionId, UUID_TO_BIN("73534e25-fcb2-4351-a896-ce5062934028"))
, (@directionId, UUID_TO_BIN("882be549-b106-42d9-a6eb-3ccdb27fffef"))
, (@directionId, UUID_TO_BIN("edcaef95-7861-45df-b34a-c9b94eb53451"))
, (@directionId, UUID_TO_BIN("7ff2571c-c7ac-4ff0-83f9-c93e9b0f1345"))
, (@directionId, UUID_TO_BIN("9eb1cff3-1433-41c9-a9bb-cd36bef36103"))
, (@directionId, UUID_TO_BIN("d3a3bd05-7e82-46ad-b093-33e519b07586"))
, (@directionId, UUID_TO_BIN("2798ae05-23fa-4e73-b57c-9bd9ecb5ce13"))
, (@directionId, UUID_TO_BIN("cecb9b2d-8055-46bd-ae5c-7a6f0c282b0f"))
, (@directionId, UUID_TO_BIN("1508fd74-fb78-4281-b032-23c9f5afac54"))
, (@directionId, UUID_TO_BIN("03c8c776-0f94-41a7-b177-800edcf87375"))
, (@directionId, UUID_TO_BIN("bdeeb248-1670-4be5-8672-ed0684b603ba"))
, (@directionId, UUID_TO_BIN("5caa6152-6490-4dec-9d4a-4e2848689782"))
, (@directionId, UUID_TO_BIN("c2738458-417b-407a-ae2b-973c2f87bd7e"))
, (@directionId, UUID_TO_BIN("2b8fef1a-3f4e-40e4-bdfe-5ead1deb22ee"))
, (@directionId, UUID_TO_BIN("3cf300c1-b57d-481c-ab6f-647d989799cf"))
, (@directionId, UUID_TO_BIN("3f9f1d9a-a336-4fc0-a171-5ea1c37a0798"))
, (@directionId, UUID_TO_BIN("81ad6300-4089-4e12-8677-cbb38e9f4f49"))
, (@directionId, UUID_TO_BIN("744724b5-a5d2-4138-b455-d4fe2df6d6ff"))
, (@directionId, UUID_TO_BIN("57590463-e587-4f1e-b8e7-fc26cd8b0338"))
, (@directionId, UUID_TO_BIN("b95614c7-11e2-49dc-ab42-e31a93a7cb4e"))
, (@directionId, UUID_TO_BIN("bc38035c-b825-4cb2-aa14-2a1946be5a7c"))
, (@directionId, UUID_TO_BIN("4aeae19a-f14b-4953-8d86-c9a3e47bd7de"))
, (@directionId, UUID_TO_BIN("d39fc549-6b0a-4362-a72a-bf441c6ce9b7"))
, (@directionId, UUID_TO_BIN("2d74f28b-9d99-40d7-b606-7455aeb4922f"))
, (@directionId, UUID_TO_BIN("51ac3662-510d-4a8e-a437-3ad0b8a191e1"))
, (@directionId, UUID_TO_BIN("241b0084-8ac6-449c-a337-a13fa4a7cad1"))
, (@directionId, UUID_TO_BIN("1d86822b-278f-4c5a-9dda-cdeb35a18f8d"))
, (@directionId, UUID_TO_BIN("8da01648-1dbf-424c-af7a-00486d5a747b"))
, (@directionId, UUID_TO_BIN("a07603cf-6709-4dcd-ab4d-851087c26e67"))
, (@directionId, UUID_TO_BIN("6c41c50c-ccb2-432b-ab91-f2d276d11a93"))
, (@directionId, UUID_TO_BIN("5a9692cc-8196-40bc-aa7a-b2c589189ab4"))
, (@directionId, UUID_TO_BIN("c4a1333e-a8f8-40bd-9316-68ca0de106f5"))
, (@directionId, UUID_TO_BIN("5fdb0fca-b09d-4405-8cb8-17b79c3a2e64"))
, (@directionId, UUID_TO_BIN("e5d7ac89-727d-455a-bebe-39b6b9a095a5"))
, (@directionId, UUID_TO_BIN("015454d4-dda4-4902-9ed7-7dd9291e9596"))
, (@directionId, UUID_TO_BIN("08110419-9e08-4057-b86f-d178ad33c88e"))
, (@directionId, UUID_TO_BIN("9eb64951-8133-470a-a069-c8ee9252635a"))
, (@directionId, UUID_TO_BIN("a59bd8d6-615d-42aa-9fa7-41ed1c45229b"))
, (@directionId, UUID_TO_BIN("dd3c40ff-ef8c-4d97-ab51-056a3371a8ca"))
, (@directionId, UUID_TO_BIN("fc4ce3f4-e08c-41a5-8a7a-ff79593204f2"))
, (@directionId, UUID_TO_BIN("6a8c2f08-2d4f-467d-b31a-f767e2d85bab"))
, (@directionId, UUID_TO_BIN("4c7c84c0-6504-424f-aa78-3f074c32e08e"))
, (@directionId, UUID_TO_BIN("617e348e-daa8-434d-8ba8-ebf1cabb3cf8"))
, (@directionId, UUID_TO_BIN("fe474ce7-43b8-4865-aca0-4a84170ac5a7"))
, (@directionId, UUID_TO_BIN("b6365aba-a293-43fa-b651-dc8727fd7fac"))
, (@directionId, UUID_TO_BIN("d66a44aa-dbdc-4a0d-9d5c-85a0767ff107"))
, (@directionId, UUID_TO_BIN("4ea8faff-ca50-4a67-96e8-f6944b4115f9"))
, (@directionId, UUID_TO_BIN("9a9741d8-73b2-430d-904f-5f56c1ea7fef"))
, (@directionId, UUID_TO_BIN("92d8783f-df9a-4e1e-b421-aa195aa9df85"))
, (@directionId, UUID_TO_BIN("1ba99a66-c064-4dc2-9de2-9df628a0d96b"))
, (@directionId, UUID_TO_BIN("72890156-6cd4-428f-b7b0-0adc701ab07f"))
, (@directionId, UUID_TO_BIN("a0912901-c22e-4e22-ac7c-90e9a707f430"))
, (@directionId, UUID_TO_BIN("69a236d9-93f4-4c64-9d46-e04e13f00d95"))
, (@directionId, UUID_TO_BIN("8424f4cb-7841-4c9b-aa71-983b50f4fbbc"))
, (@directionId, UUID_TO_BIN("9db6fd27-6167-4f34-a7fd-a59208401cc4"))
, (@directionId, UUID_TO_BIN("f9eee3c8-a64b-4525-9503-3bc6dda1feeb"))
, (@directionId, UUID_TO_BIN("6b730643-8043-443b-91e4-e32c51eed76b"))
, (@directionId, UUID_TO_BIN("d98e2ac4-8065-4f26-bd54-2b82838855ed"))
, (@directionId, UUID_TO_BIN("1b3fbfca-3e22-4a92-91d5-fe49615e71fc"))
, (@directionId, UUID_TO_BIN("26335b3c-906c-4307-bca3-d06f35289955"))
, (@directionId, UUID_TO_BIN("380d0905-5528-44c8-bcd8-03f407a0f30a"))
, (@directionId, UUID_TO_BIN("86b93ffe-fd87-4135-813e-f5dfc08af106"))
, (@directionId, UUID_TO_BIN("dcc189fa-5f1d-4135-8d9d-7be154e1cbc8"))
, (@directionId, UUID_TO_BIN("43bdea3e-6136-41e9-8020-614731615927"))
, (@directionId, UUID_TO_BIN("5069db38-5bc5-4ec7-8cfd-b3044c3fb7fb"))
, (@directionId, UUID_TO_BIN("2cb31fc7-b8c2-44a9-a356-f7a5a4218c42"))
, (@directionId, UUID_TO_BIN("3458b642-c548-4d9d-b9a4-03b3359d34cb"))
, (@directionId, UUID_TO_BIN("c3ea7590-1372-431d-98f9-7b6c02020138"))
, (@directionId, UUID_TO_BIN("292b31f0-2dd0-48e0-b2a2-d90765aaae42"))
, (@directionId, UUID_TO_BIN("fe5a7391-b978-40de-aa58-b8ee5f2ba544"))
, (@directionId, UUID_TO_BIN("1359f96b-e3e3-414b-abf9-c32f5d4ab310"))
, (@directionId, UUID_TO_BIN("99f3eb92-3450-430a-828a-1f263e09b99f"))
, (@directionId, UUID_TO_BIN("26ba9f75-58c5-46c6-ad1d-4ec1b5739dfb"))
, (@directionId, UUID_TO_BIN("b33d4f5c-f754-4fc5-bf39-5a4870203d60"))
, (@directionId, UUID_TO_BIN("d22a31f3-20cc-482c-8262-77d439a9ef01"))
, (@directionId, UUID_TO_BIN("c82f8bca-da9c-48b4-ad00-eb4f1b3d9529"))
, (@directionId, UUID_TO_BIN("c277778d-a289-4179-a244-6a75e2a2ddee"))
, (@directionId, UUID_TO_BIN("69851663-7439-4076-89b2-80612dbd593c"))
, (@directionId, UUID_TO_BIN("5c0aed15-f34f-48d8-a0c5-e0994c19b776"))
, (@directionId, UUID_TO_BIN("9e6eef40-926c-4dd5-b487-3035a32e5cff"))
, (@directionId, UUID_TO_BIN("44c17375-2864-46b1-8513-3909e6a1ae5b"));

set @directionId = (select Id from directions where Title = "Книжки та про них" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("82362cd3-675c-4065-b983-948fd7f00750"))
, (@directionId, UUID_TO_BIN("930d2206-b1d1-40ac-9984-306a0ed342d2"));

set @directionId = (select Id from directions where Title = "Реабілітація" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("fb1e570a-dc90-4753-852d-747b0e68a765"))
, (@directionId, UUID_TO_BIN("401cdda6-5e20-41ed-af6d-2862b0cef353"));

set @directionId = (select Id from directions where Title = "Оздоровлення" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("c17879d7-c34b-47ba-96cd-79eeec61a7f8"))
, (@directionId, UUID_TO_BIN("a70accb3-041f-47e0-ab39-122c2f640127"));

set @directionId = (select Id from directions where Title = "Мови/Гуманітарій" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("c620921a-90b6-4dda-b27a-232a53ad81e9"))
, (@directionId, UUID_TO_BIN("9335a91e-5d6c-4069-ba74-61a33a15be6b"));

set @directionId = (select Id from directions where Title = "Музика" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("cfb89ff8-bd2b-44fd-875f-821ff8bac40d"));

set @directionId = (select Id from directions where Title = "Фотогуртки" limit 1);

INSERT INTO directioninstitutionhierarchy (DirectionsId, InstitutionHierarchiesId)
VALUES (@directionId, UUID_TO_BIN("b9e57678-9ece-44dd-9493-4d35aaf2bc85"));

