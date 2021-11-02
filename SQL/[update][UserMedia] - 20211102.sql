ALTER TABLE UserMedia
DROP COLUMN PhotoTags;


ALTER TABLE UserMedia
ADD ProjectTags nvarchar(max);