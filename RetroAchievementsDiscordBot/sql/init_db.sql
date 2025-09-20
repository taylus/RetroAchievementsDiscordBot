create table if not exists Users
(
    ULID text not null primary key,
    Name text not null,
    Avatar text null,
    LastUpdated int not null
);

insert into Users(ULID, Name, Avatar, LastUpdated) values
('01FWNYX95JRRRJTVDSQ4C9ZZ3K', 'Taylus', '/UserPic/Taylus.png', 1758120391);

select * from Users;

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

insert into UserAchievements(ULID, AchievementID, UnlockedAt, Title, Description, Points, GameTitle, GameID, ConsoleName, BadgeURL) values
('01FWNYX95JRRRJTVDSQ4C9ZZ3K', 186164, 1758120391, '[DW1] Here Comes the Sun', 'Obtain the Sun Stone', 5, 'Dragon Quest I & II', 3818, 'Game Boy Color', '/Badge/221391.png');

select u.Name, a.*
from Users u
inner join UserAchievements a
    on a.ULID = u.ULID;

drop table UserAchievements;
drop table Users;
