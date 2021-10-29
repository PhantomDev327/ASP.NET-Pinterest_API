ALTER TABLE UserMedia
ADD IsPrimary bit;

go 

ALTER TABLE UserMedia
ADD media_title nvarchar(max);
go
ALTER TABLE UserMedia
ADD media_description nvarchar(max);

go
ALTER TABLE UserMedia
ADD PhotoTags nvarchar(max);;