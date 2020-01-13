-- ==============================================
-- = 1. База данных
-- ==============================================

USE master;

-- Удаление существующей базы данных
IF DB_ID('GameDatabase') IS NOT NULL
	DROP DATABASE GameDatabase;

-- Создание новой базы данных
CREATE DATABASE GameDatabase;
GO


-- ==============================================
-- = 2. Добавление пользователей
-- ==============================================

USE GameDatabase;

-- Добавление пользователя с правами на чтение
IF EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'reader')
DROP LOGIN reader
GO
-- Добавление логина
CREATE LOGIN reader WITH PASSWORD = 'reader'
GO
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'reader')
DROP USER reader
GO
CREATE USER reader FOR LOGIN reader
EXEC sp_addrolemember 'db_datareader', 'reader'

-- Добавление администратора
IF EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'admin')
DROP LOGIN admin
GO
-- Добавление логина
CREATE LOGIN admin WITH PASSWORD = 'admin'
GO
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'admin')
DROP USER admin
GO
CREATE USER admin FOR LOGIN admin
EXEC sp_addrolemember 'db_datareader', 'admin'
EXEC sp_addrolemember 'db_datawriter', 'admin'

-- ==============================================
-- = 3. Таблицы
-- ==============================================

USE GameDatabase;

-- Таблица "Игры"
CREATE TABLE GameDatabase.dbo.Games
(
	-- Столбцы
	id				INTEGER			NOT NULL	IDENTITY,
	title			NVARCHAR(255)	NOT NULL,
	summary			NVARCHAR(MAX)	NOT NULL,
	website			NVARCHAR(255)	NOT NULL,
	release_date	DATE			NULL

	-- Ограничения
	CONSTRAINT PK_Games
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Разработчики"
CREATE TABLE GameDatabase.dbo.Developers
(
	-- Столбцы
	id		INTEGER			NOT NULL	IDENTITY,
	title	NVARCHAR(255)	NOT NULL

	-- Ограничения
	CONSTRAINT PK_Developers
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Игры - Разработчкики"
CREATE TABLE GameDatabase.dbo.Games_Developers
(
	-- Столбцы
	game_id			INTEGER	NOT NULL,
	developer_id	INTEGER NOT NULL

	-- Ограничения
	CONSTRAINT PK_GamesDevelopers
		PRIMARY KEY CLUSTERED (game_id ASC, developer_id ASC),
	CONSTRAINT FK_GamesDevelopers_Games
		FOREIGN KEY (game_id)
			REFERENCES dbo.Games (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
	CONSTRAINT FK_GamesDevelopers_Developers
		FOREIGN KEY (developer_id)
			REFERENCES dbo.Developers (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE
);

-- Таблица "Издатели"
CREATE TABLE GameDatabase.dbo.Publishers
(
	-- Столбцы
	id		INTEGER			NOT NULL	IDENTITY,
	title	NVARCHAR(255)	NOT NULL

	-- Ограничения
	CONSTRAINT PK_Publishers
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Игры - Издатели"
CREATE TABLE GameDatabase.dbo.Games_Publishers
(
	-- Столбцы
	game_id			INTEGER	NOT NULL,
	publisher_id	INTEGER NOT NULL

	-- Ограничения
	CONSTRAINT PK_GamesPublishers
		PRIMARY KEY CLUSTERED (game_id ASC, publisher_id ASC),
	CONSTRAINT FK_GamesPublishers_Games
		FOREIGN KEY (game_id)
			REFERENCES dbo.Games (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
	CONSTRAINT FK_GamesPublishers_Publishers
		FOREIGN KEY (publisher_id)
			REFERENCES dbo.Publishers (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE
);

-- Таблица "Жанры"
CREATE TABLE GameDatabase.dbo.Genres
(
	-- Столбцы
	id		INTEGER			NOT NULL	IDENTITY,
	title	NVARCHAR(255)	NOT NULL

	-- Ограничения
	CONSTRAINT PK_Genres
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Игры - Жанры"
CREATE TABLE GameDatabase.dbo.Games_Genres
(
	-- Столбцы
	game_id		INTEGER	NOT NULL,
	genre_id	INTEGER	NOT NULL

	-- Ограничения
	CONSTRAINT PK_GamesGenres
		PRIMARY KEY CLUSTERED (game_id ASC, genre_id ASC)
	CONSTRAINT FK_GamesGenres_Games
		FOREIGN KEY (game_id)
			REFERENCES dbo.Games (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
	CONSTRAINT FK_GamesGenres_Genres
		FOREIGN KEY (genre_id)
			REFERENCES dbo.Genres (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE
);

-- Таблица "Платформы"
CREATE TABLE GameDatabase.dbo.Platforms
(
	-- Столбцы
	id		INTEGER			NOT NULL	IDENTITY,
	title	NVARCHAR(255)	NOT NULL

	-- Ограничения
	CONSTRAINT PK_Platforms
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Игры - Платформы"
CREATE TABLE GameDatabase.dbo.Games_Platforms
(
	-- Столбцы
	game_id		INTEGER	NOT NULL,
	platform_id	INTEGER	NOT NULL

	-- Ограничения
	CONSTRAINT PK_GamesPlatforms
		PRIMARY KEY CLUSTERED (game_id ASC, platform_id ASC),
	CONSTRAINT FK_GamesPlatforms_Games
		FOREIGN KEY (game_id)
			REFERENCES dbo.Games (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
	CONSTRAINT FK_GamesPlatforms_Platforms
		FOREIGN KEY (platform_id)
			REFERENCES dbo.Platforms (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE
);

-- Таблица "Рецензенты"
CREATE TABLE GameDatabase.dbo.Reviewers
(
	-- Столбцы
	id		INTEGER			NOT NULL	IDENTITY,
	title	NVARCHAR(255)	NOT NULL

	-- Ограничения
	CONSTRAINT PK_Reviewers
		PRIMARY KEY CLUSTERED (id ASC)
);

-- Таблица "Рецензии"
CREATE TABLE GameDatabase.dbo.Reviews
(
	-- Столбцы
	game_id		INTEGER			NOT NULL,
	reviewer_id	INTEGER			NOT NULL,
	summary		NVARCHAR(MAX)	NOT NULL,
	web_address	NVARCHAR(255)	NOT NULL,
	score		TINYINT			NULL

	-- Ограничения
	CONSTRAINT PK_Reviews
		PRIMARY KEY CLUSTERED (game_id ASC, reviewer_id ASC)
	CONSTRAINT FK_Reviews_Games
		FOREIGN KEY (game_id)
			REFERENCES dbo.Games (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
	CONSTRAINT FK_Reviews_Reviewers
		FOREIGN KEY (reviewer_id)
			REFERENCES dbo.Reviewers (id)
			ON DELETE CASCADE
			ON UPDATE CASCADE
);
GO

-- ==============================================
-- = 4. Хранимые процедуры
-- ==============================================

-- Добавление игр
CREATE OR ALTER PROCEDURE StrProc_AddGame
	@title		NVARCHAR(255),
	@release	DATE,
	@website	NVARCHAR(255),
	@summary	NVARCHAR(MAX)
AS
BEGIN
	INSERT INTO dbo.Games (title, release_date, website, summary)
	VALUES (@title, @release, @website, @summary)
END;
GO

-- Добавление разработчиков
CREATE OR ALTER PROCEDURE StrProc_AddDeveloper
	@title	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Developers(title)
	VALUES (@title)
END;
GO

-- Добавление разработчика игры
CREATE OR ALTER PROCEDURE StrProc_AddGameDeveloper
	@game_id		INT,
	@developer_id	INT
AS
BEGIN
	INSERT INTO dbo.Games_Developers(game_id, developer_id)
	VALUES (@game_id, @developer_id)
END;
GO

-- Добавление издателей
CREATE OR ALTER PROCEDURE StrProc_AddPublisher
	@title	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Publishers(title)
	VALUES (@title)
END;
GO

-- Добавление издателей игры
CREATE OR ALTER PROCEDURE StrProc_AddGamePublisher
	@game_id		INT,
	@publisher_id	INT
AS
BEGIN
	INSERT INTO dbo.Games_Publishers(game_id, publisher_id)
	VALUES (@game_id, @publisher_id)
END;
GO

-- Добавление жанров
CREATE OR ALTER PROCEDURE StrProc_AddGenre
	@title	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Genres(title)
	VALUES (@title)
END;
GO

-- Добавление жанров игры
CREATE OR ALTER PROCEDURE StrProc_AddGameGenre
	@game_id	INT,
	@genre_id	INT
AS
BEGIN
	INSERT INTO dbo.Games_Genres(game_id, genre_id)
	VALUES (@game_id, @genre_id)
END;
GO

-- Добавление платформ
CREATE OR ALTER PROCEDURE StrProc_AddPlatform
	@title	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Platforms(title)
	VALUES (@title)
END;
GO

-- Добавление платформ игры
CREATE OR ALTER PROCEDURE StrProc_AddGamePlatform
	@game_id		INT,
	@platform_id	INT
AS
BEGIN
	INSERT INTO dbo.Games_Platforms(game_id, platform_id)
	VALUES (@game_id, @platform_id)
END;
GO

-- Добавление рецензентов
CREATE OR ALTER PROCEDURE StrProc_AddReviewer
	@title	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Reviewers(title)
	VALUES (@title)
END;
GO

-- Добавление платформ игры
CREATE OR ALTER PROCEDURE StrProc_AddReview
	@game_id		INT,
	@reviewer_id	INT,
	@score			TINYINT,
	@summary		NVARCHAR(MAX),
	@web_address	NVARCHAR(255)
AS
BEGIN
	INSERT INTO dbo.Reviews(game_id, reviewer_id, score, summary, web_address)
	VALUES (@game_id, @reviewer_id, @score, @summary, @web_address)
END;
GO

GRANT EXECUTE ON dbo.StrProc_AddGame TO admin
GRANT EXECUTE ON dbo.StrProc_AddDeveloper TO admin
GRANT EXECUTE ON dbo.StrProc_AddGameDeveloper TO admin
GRANT EXECUTE ON dbo.StrProc_AddPublisher TO admin
GRANT EXECUTE ON dbo.StrProc_AddGamePublisher TO admin
GRANT EXECUTE ON dbo.StrProc_AddGenre TO admin
GRANT EXECUTE ON dbo.StrProc_AddGameGenre TO admin
GRANT EXECUTE ON dbo.StrProc_AddPlatform TO admin
GRANT EXECUTE ON dbo.StrProc_AddGamePlatform TO admin
GRANT EXECUTE ON dbo.StrProc_AddReviewer TO admin
GRANT EXECUTE ON dbo.StrProc_AddReview TO admin
GO

-- ==============================================
-- = 5. Заполнение
-- ==============================================

-- ----------------------------------------------
-- - 5.1 Игры
-- ----------------------------------------------

--EXEC StrProc_AddGame
--	@title = '',
--	@release = '',
--	@website = '',
--	@summary = ''

EXEC StrProc_AddGame
	@title = 'S.T.A.L.K.E.R. 2',
	@release = NULL,
	@website = 'stalker2.com',
	@summary = 'S.T.A.L.K.E.R. 2 является шутером от первого лица в открытом мире с элементами survival horror и ролевой игры. Действие игры происходит в постапокалиптической зоне отчуждения Чернобыльской АЭС, где — помимо аварии в 1986 году — в 2006 году произошла вторая катастрофа, в результате которой физические, химические и биологические процессы на данной территории изменились, появилось множество аномалий, артефактов и существ-мутантов.'

EXEC StrProc_AddGame
	@title = 'Dishonored',
	@release = '20120910',
	@website = 'dishonored.bethesda.net',
	@summary = 'Действие игры происходит в охваченном эпидемией чумы вымышленном городе Дануолл, прообразом которого послужил Лондон времён Викторианской эпохи. Главный герой игры, лорд-защитник Корво Аттано, после обвинения в убийстве императрицы и побега из тюрьмы пытается отомстить своим врагам. Управляемый игроком Корво действует в качестве наёмного убийцы, задачей которого в каждой отдельной миссии является поиск и убийство или бескровное устранение определенного персонажа. Кроме внушительного арсенала разнообразного оружия, персонаж также обладает сверхъестественными способностями. Ключевой особенностью игры является нелинейность: каждую миссию можно выполнить множеством путей, как вступая в бой с различными противниками, так и действуя скрытно и избегая обнаружения.'

EXEC StrProc_AddGame
	@title = 'Valiant Hearts: The Great War',
	@release = '20140625',
	@website = 'valianthearts.ubi.com',
	@summary = 'Сюжет игры рассказывает о четырёх разных людях, оказавшихся на фронте Первой Мировой войны. Им предстоит пройти все испытания войны плечом к плечу и достигнуть своей цели. Используя мультипликационную рисовку, разработчики хотят рассказать о сложных вещах более простым языком.'

EXEC StrProc_AddGame
	@title = 'Ori and the Blind Forest',
	@release = '20150311',
	@website = 'oriblindforest.com',
	@summary = 'Игра представляет собой двухмерный платформер. Игрок управляет персонажем по имени Ори (сказочное существо белого цвета, что-то среднее между лисицей, белкой и дикой кошкой) и защищающим его духом по имени Сейн (англ. Sein) — сгусток энергии, который следует за Ори. С помощью Сейна можно атаковать врагов, в которых он (по нажатии левой кнопки мыши, клавиши «X» на клавиатуре или кнопки геймпада) выпускает заряды «духовного пламени». Кроме того, Сейн может сделать кратковременный мощный выброс энергии (на что тратится ресурс), поражающий всех врагов поблизости от Ори и разрушающий некоторые объекты. Сам Ори изначально умеет только прыгать, но в процессе игры он сможет научиться карабкаться по стенам, нырять под воду, парить в воздухе, совершать двойные-тройные прыжки и использовать энергию, чтобы выстреливать собой или отталкивать врагов и предметы.'

EXEC StrProc_AddGame
	@title = 'Ori and the Will of the Wisps',
	@release = '20200311',
	@website = 'orithegame.com',
	@summary = 'Ori and the Will of the Wisps — предстоящая видеоигра в жанре платформер, разрабатываемая студией Moon Studios для Windows 10 и Xbox One. Является продолжением игры 2015 года Ori and the Blind Forest. О разработке игры было объявлено на выставке Electronic Entertainment Expo 2017. Дата выхода игры — 11 марта 2020 года.'

EXEC StrProc_AddGame
	@title = 'Assassin’s Creed III',
	@release = '20121030',
	@website = 'ubisoft.com/en-us/game/assassins-creed-3-remastered',
	@summary = 'Assassin’s Creed III относится к жанру action-adventure с элементами стелс-экшена. События игры происходят в открытом мире, который представлен несколькими локациями. В Assassin’s Creed III представлены города Бостон и Нью-Йорк, а также территория Фронтира. Игра имеет 2 сюжетные линии: присутствуют сюжетная линия в прошлом времени, действие которой разворачивается во второй половине XVIII века и сюжетная линия в настоящем, события которой происходят в 2012 году. Игрок может свободно переключаться между ними. В Assasin’s Creed III, как и в прошлых частях серии, для перемещения по игровому миру игрок может использовать искусство паркур: можно «зацепиться» за любой выступ, забраться на любую постройку и т. д. В игре есть «точки обзора» — здания или природные возвышенности, забравшись на которые, игрок может выполнить «синхронизацию» — открытие некоторой части карты. После выполнения «синхронизации» игрок может сделать «Прыжок веры» — выдуманный игровой прыжок, который позволяет осуществить быстрый спуск со здания или возвышенности в смягчающую падение среду (стог сена, водное пространство и т. д.). Также в игре присутствуют предметы коллекционирования и случайные события в открытом мире. Помимо передвижения пешком, игровой персонаж может использовать транспортные средства: лошадей, а также боевой корабль для перемещения и выполнения заданий.'

-- ----------------------------------------------
-- - 5.2 Разработчики
-- ----------------------------------------------

--EXEC StrProc_AddDeveloper
--	@title = ''

EXEC StrProc_AddDeveloper
	@title = 'GSC Game World'

EXEC StrProc_AddDeveloper
	@title = 'Arkane Studios'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Montpellier'

EXEC StrProc_AddDeveloper
	@title = 'Moon Studios'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Montreal'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Annecy'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Quebec'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Singapore'

EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Ukraine'
	
EXEC StrProc_AddDeveloper
	@title = 'Ubisoft Barcelona'

-- ----------------------------------------------
-- - 5.3 Игры - Разработчики
-- ----------------------------------------------

--EXEC StrProc_AddGameDeveloper
--	@game_id = ,
--	@developer_id = 

EXEC StrProc_AddGameDeveloper
	@game_id = 1,
	@developer_id = 1

EXEC StrProc_AddGameDeveloper
	@game_id = 2,
	@developer_id = 2

EXEC StrProc_AddGameDeveloper
	@game_id = 3,
	@developer_id = 3

EXEC StrProc_AddGameDeveloper
	@game_id = 4,
	@developer_id = 4

EXEC StrProc_AddGameDeveloper
	@game_id = 5,
	@developer_id = 4

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 5

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 6

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 7

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 8

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 9

EXEC StrProc_AddGameDeveloper
	@game_id = 6,
	@developer_id = 10

-- ----------------------------------------------
-- - 5.4 Издатели
-- ----------------------------------------------

--EXEC StrProc_AddPublisher
--	@title = ''

EXEC StrProc_AddPublisher
	@title = 'Bethesda Softworks'

EXEC StrProc_AddPublisher
	@title = 'Ubisoft'

EXEC StrProc_AddPublisher
	@title = 'Microsoft Studios'

EXEC StrProc_AddPublisher
	@title = 'Xbox Game Studios'

-- ----------------------------------------------
-- - 5.5 Игры - Издатели
-- ----------------------------------------------

--EXEC StrProc_AddGamePublisher
--	@game_id = ,
--	@publisher_id = 

EXEC StrProc_AddGamePublisher
	@game_id = 2,
	@publisher_id = 1

EXEC StrProc_AddGamePublisher
	@game_id = 3,
	@publisher_id = 2

EXEC StrProc_AddGamePublisher
	@game_id = 4,
	@publisher_id = 3

EXEC StrProc_AddGamePublisher
	@game_id = 5,
	@publisher_id = 4

EXEC StrProc_AddGamePublisher
	@game_id = 6,
	@publisher_id = 2

-- ----------------------------------------------
-- - 5.6 Жанры
-- ----------------------------------------------

EXEC StrProc_AddGenre
	@title = 'Shooter'

EXEC StrProc_AddGenre
	@title = 'Action'

EXEC StrProc_AddGenre
	@title = 'Adventure'

EXEC StrProc_AddGenre
	@title = 'Platformer'

EXEC StrProc_AddGenre
	@title = 'Point and click'

-- ----------------------------------------------
-- - 5.7 Игры - Жанры
-- ----------------------------------------------

--EXEC StrProc_AddGameGenre
--	@game_id = ,
--	@genre_id = 

EXEC StrProc_AddGameGenre
	@game_id = 1,
	@genre_id = 1

EXEC StrProc_AddGameGenre
	@game_id = 1,
	@genre_id = 2

EXEC StrProc_AddGameGenre
	@game_id = 2,
	@genre_id = 2

EXEC StrProc_AddGameGenre
	@game_id = 2,
	@genre_id = 3

EXEC StrProc_AddGameGenre
	@game_id = 3,
	@genre_id = 4
	
EXEC StrProc_AddGameGenre
	@game_id = 3,
	@genre_id = 5

EXEC StrProc_AddGameGenre
	@game_id = 4,
	@genre_id = 3

EXEC StrProc_AddGameGenre
	@game_id = 4,
	@genre_id = 4

EXEC StrProc_AddGameGenre
	@game_id = 5,
	@genre_id = 3

EXEC StrProc_AddGameGenre
	@game_id = 5,
	@genre_id = 4

EXEC StrProc_AddGameGenre
	@game_id = 6,
	@genre_id = 2

EXEC StrProc_AddGameGenre
	@game_id = 6,
	@genre_id = 3
	
-- ----------------------------------------------
-- - 5.8 Платформы
-- ----------------------------------------------

EXEC StrProc_AddPlatform
	@title = 'Windows'
	
EXEC StrProc_AddPlatform
	@title = 'PlayStation 3'
	
EXEC StrProc_AddPlatform
	@title = 'PlayStation 4'
	
EXEC StrProc_AddPlatform
	@title = 'Xbox 360'
	
EXEC StrProc_AddPlatform
	@title = 'Xbox One'

EXEC StrProc_AddPlatform
	@title = 'Nintendo Switch'

EXEC StrProc_AddPlatform
	@title = 'iOS'

EXEC StrProc_AddPlatform
	@title = 'Android'

-- ----------------------------------------------
-- - 5.9 Игры - Платформы
-- ----------------------------------------------

--EXEC StrProc_AddGamePlatform
--	@game_id = ,
--	@platform_id = 

EXEC StrProc_AddGamePlatform
	@game_id = 2,
	@platform_id = 1

EXEC StrProc_AddGamePlatform
	@game_id = 2,
	@platform_id = 2

EXEC StrProc_AddGamePlatform
	@game_id = 2,
	@platform_id = 3

EXEC StrProc_AddGamePlatform
	@game_id = 2,
	@platform_id = 4

EXEC StrProc_AddGamePlatform
	@game_id = 2,
	@platform_id = 5

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 1

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 2

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 3

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 4

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 5

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 6

EXEC StrProc_AddGamePlatform
	@game_id = 3,
	@platform_id = 7

EXEC StrProc_AddGamePlatform
	@game_id = 4,
	@platform_id = 1

EXEC StrProc_AddGamePlatform
	@game_id = 4,
	@platform_id = 5

EXEC StrProc_AddGamePlatform
	@game_id = 4,
	@platform_id = 6

EXEC StrProc_AddGamePlatform
	@game_id = 5,
	@platform_id = 1

EXEC StrProc_AddGamePlatform
	@game_id = 5,
	@platform_id = 5

	EXEC StrProc_AddGamePlatform
	@game_id = 6,
	@platform_id = 1

EXEC StrProc_AddGamePlatform
	@game_id = 6,
	@platform_id = 2

EXEC StrProc_AddGamePlatform
	@game_id = 6,
	@platform_id = 3

EXEC StrProc_AddGamePlatform
	@game_id = 6,
	@platform_id = 4

EXEC StrProc_AddGamePlatform
	@game_id = 6,
	@platform_id = 5

-- ----------------------------------------------
-- - 5.10 Рецензенты
-- ----------------------------------------------

EXEC StrProc_AddReviewer
	@title = 'Игромания'

-- ----------------------------------------------
-- - 5.11 Рецензии
-- ----------------------------------------------

--EXEC StrProc_AddReview
--	@game_id = ,
--	@reviewer_id = ,
--	@score = ,
--	@summary = '',
--	@web_address = ''

EXEC StrProc_AddReview
	@game_id = 2,
	@reviewer_id = 1,
	@score = 80,
	@summary = 'Перед нами одна из главных игр года, которая, к сожалению, не взяла заявленную планку. Смелая, потому что не побоялась проигнорировать концепцию боевиков этого поколения, она в то же время глупа, поскольку сделала это как некий немой протест. Dishonored примеряет на себя механику игр прошлого не чтобы развить старые идеи или поднять их на новый уровень, а... кажется, просто из ностальгии.',
	@web_address = 'igromania.ru/article/22098/Dishonored'

EXEC StrProc_AddReview
	@game_id = 3,
	@reviewer_id = 1,
	@score = 90,
	@summary = 'Рассказать о трагичных событиях в подчеркнуто не серьезном стиле решаются не многие. Компания Ubisoft Montpellier рискнула - и сделала шедевр. Ведь помимо изящного повествования о давно минувших днях мы получили еще и прекрасную, захватывающую, тщательно продуманную игру.',
	@web_address = 'igromania.ru/article/25099/Valiant_Hearts_The_Great_War'

EXEC StrProc_AddReview
	@game_id = 4,
	@reviewer_id = 1,
	@score = 90,
	@summary = 'Одурманивающе красивый, интересный и по-своему глубокий платформер. Ori and the Blind Forest не держит вас за дурака и постоянно подкидывает новые испытания, но не теряет хватку: игра всегда ровно настолько сложная, насколько нужно.',
	@web_address = 'igromania.ru/article/26324/Cvet_volshebstva_Obzor_Ori_and_the_Blind_Forest'

EXEC StrProc_AddReview
	@game_id = 6,
	@reviewer_id = 1,
	@score = 90,
	@summary = 'Assassin''s Creed 3 стала апофеозом развития сериала и одной из главных высот современного поколения консолей. Гигантская многосоставная игра в потрясающем открытом мире, с интересным сюжетом и очень дорогой картинкой. Даже такая эфемерная материя, как «душа», кажется, на месте. Один из главных претендентов на звание игры года.',
	@web_address = 'igromania.ru/article/22351/Assassins_Creed_3'

-- ==============================================
-- = *. Возврат в master
-- ==============================================

USE master;