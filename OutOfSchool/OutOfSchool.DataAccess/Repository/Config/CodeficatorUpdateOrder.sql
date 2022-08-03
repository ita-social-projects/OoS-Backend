USE out_of_school;

SET SQL_SAFE_UPDATES = 0;

UPDATE `codeficators` SET `Order` = 120 WHERE `Category` = 'O';
UPDATE `codeficators` SET `Order` = 110 WHERE `Category` = 'P';
UPDATE `codeficators` SET `Order` = 100 WHERE `Category` = 'H';
UPDATE `codeficators` SET `Order` = 80 WHERE `Category` = 'X';
UPDATE `codeficators` SET `Order` = 70 WHERE `Category` = 'C';
UPDATE `codeficators` SET `Order` = 60 WHERE `Category` = 'T';
UPDATE `codeficators` SET `Order` = 50 WHERE `Category` = 'M';
UPDATE `codeficators` SET `Order` = 40 WHERE `Category` = 'B';

UPDATE `codeficators` SET `Order` = 30 WHERE Id in (4506,4791,6006,6240,6752,19191,31748) AND `Category` IN ('M','K');
UPDATE `codeficators` SET `Order` = 20 WHERE Id in (1378,3917,9335,10720,21800,24274,28289,30145) AND `Category` = 'M';
UPDATE `codeficators` SET `Order` = 10 WHERE Id in (1099,4090,6193,7385,9793,13117,13960,15447,17423,18415,20675,23241,26238,27209,29660,31734,31737) AND `Category` IN ('M','K');

SELECT * FROM `out_of_school`.`codeficators` WHERE `Order` = 0;

SET SQL_SAFE_UPDATES = 1;