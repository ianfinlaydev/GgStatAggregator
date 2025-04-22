select *
from [Players]

select *
from [Tables]

select *
from [StatSets]

declare @CurrentDate datetime = GetUtcDate(); 

insert into Players
values
	('Ian', @CurrentDate),
	('Emma', @CurrentDate),
	('Laurie', @CurrentDate),
	('Shane', @CurrentDate),
	('Tara', @CurrentDate),
	('Helder', @CurrentDate),
	('Rylee', @CurrentDate),
	('Harper', @CurrentDate),
	('Deb', @CurrentDate),
	('David', @CurrentDate),
	('Erica', @CurrentDate),
	('Erik', @CurrentDate),
	('Nasvhille', @CurrentDate),
	('Ninja', @CurrentDate),
	('Boris', @CurrentDate),
	('Phoebe', @CurrentDate)

--insert into [Tables]
--values ('default', 0)

--delete from Players;
--delete from StatSets where Id = 7;

--insert into StatSets
--values
--	(17, 1, 100, 80, 0, 0, 0, @CurrentDate),
--	(17, 1, 200, 60, 0, 0, 0, @CurrentDate)
	