  SET IDENTITY_INSERT Cities ON
  INSERT INTO Cities (Id, Region, District, Name, Latitude, Longitude)
  SELECT Id, Region, District, Name, Cast(Latitude as float), Cast(Longitude as float) FROM [Test]
  SET IDENTITY_INSERT Cities OFF

  DROP TABLE Test