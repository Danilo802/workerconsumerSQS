
CREATE TABLE IF NOT EXISTS files (
	filename varchar(1000) PRIMARY KEY,
	filesize long,
	last_modified datetime
);

SELECT filename FROM WHERE  filename = @filename

select * from files;

SELECT filename,  CONVERT(DATE_FORMAT(NOW(), '%d-%m-%Y %H-%i-00'),DATETIME) FROM files WHERE  filename = 'appsettings.json'

select  CONVERT(DATE_FORMAT(NOW(),  '%d-%m-%Y %H-%i-00'),DATETIME) 

CONVERT(DATE_FORMAT(NOW(), '%d-%m-%Y %H-%i-00'), DATETIME);
   # 2017-04-07 10:05:00


INSERT INTO files (filename, filesize, last_modified) VALUES ("teste.txt", 100, "2021-12-19 10:20:00");
INSERT INTO files(filename, filesize, last_modified) VALUES("appsettings.json",760,"20-12-2021 00:53:17");
delete from files where filename = "appsettings.json"


"SELECT filename, last_modified FROM files WHERE  filename = @filename"