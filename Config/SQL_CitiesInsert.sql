--------------------------------------------------------------------------
-- Author               Vitalii Ivanchuk
-- Created              30.06.2021
-- Task                 Create endpoints for Cities #163
-- Addition             Please follow the attached in the task instruction
--------------------------------------------------------------------------

  SET IDENTITY_INSERT Cities ON
  INSERT INTO Cities (Id, Region, District, Name, Latitude, Longitude)
  SELECT Id, Region, District, Name, Cast(Latitude as float), Cast(Longitude as float) FROM [Test]
  SET IDENTITY_INSERT Cities OFF

  DROP TABLE Test