-- initialize users:
-- get ULID from GET https://retroachievements.org/API/API_GetUserProfile.php?u={username}
-- TODO: build user management into the bot later
insert into Users(ULID, Name, Avatar, LastUpdated) values
('01FWNYX95JRRRJTVDSQ4C9ZZ3K', 'Taylus', '/UserPic/Taylus.png', 1758220491)
returning *;

/* test skipping an already posted achievement
insert into UserAchievements(ULID, AchievementID, UnlockedAt, Title, Description, Points, GameTitle, GameID, ConsoleName, BadgeURL) values
('01FWNYX95JRRRJTVDSQ4C9ZZ3K', 192262, 1758239318, '[DW2] The Shiny Key', 'Obtain the Silver Key', 5, 'Dragon Quest I & II', 3818, 'Game Boy Color', '/Badge/221451.png')
returning *;
*/

/* rollback
delete from UserAchievements;
delete from Users;
vacuum;
*/
