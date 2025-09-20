create table if not exists Users
(
    ULID text not null primary key,
    Name text not null,
    Avatar text null,
    LastUpdated int not null
);

create table if not exists UserAchievements
(
    ULID text not null,
    AchievementID int not null,
    UnlockedAt int not null,
    Title text not null,
    Description text null,
    Points int not null default 0,
    GameTitle text not null,
    GameID int not null,
    ConsoleName text not null,
    BadgeURL text null,
    primary key (ULID, AchievementID)
    foreign key (ULID) references Users(ULID)
);

create table if not exists UserGameStatus
(
    ULID text not null,
    GameID int not null,
    Beaten boolean not null default 0,
    Mastered boolean not null default 0,
    primary key (ULID, GameID)
);

/* rollback
drop table UserGameStatus;
drop table UserAchievements;
drop table Users;
*/
