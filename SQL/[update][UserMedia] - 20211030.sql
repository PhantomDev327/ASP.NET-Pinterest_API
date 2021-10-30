
ALTER TABLE usermedia
DROP COLUMN isPrimary;


alter table usermedia
add media_thumbnail_url nvarchar(1000)