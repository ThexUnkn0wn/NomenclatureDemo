CREATE TABLE articles
(
	[id]		INT NOT NULL PRIMARY KEY IDENTITY, 
    [name]		VARCHAR(255) NOT NULL UNIQUE, 
    [code]		VARCHAR(50) NOT NULL UNIQUE, 
    [state]		VARCHAR(50) NOT NULL DEFAULT 'INACTIVE'
);

CREATE TABLE articlepropertys
(
	[id]					INT NOT NULL PRIMARY KEY IDENTITY, 
    [code]					VARCHAR(50) FOREIGN KEY REFERENCES articles(code) ON DELETE CASCADE ON UPDATE CASCADE,
	[tva]					FLOAT NOT NULL DEFAULT 0,
    [acquisition_price]		FLOAT NULL, 
    [full_price]			FLOAT NULL,
	[start_date]			DATE NULL,
	[end_date]				DATE NULL,
	
);

CREATE INDEX idx_startdate ON articlepropertys ([start_date]);
CREATE INDEX idx_enddate ON articlepropertys ([end_date]);