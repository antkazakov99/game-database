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
-- = 2. Таблицы
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



-- ==============================================
-- = *. Возврат в master
-- ==============================================

USE master;