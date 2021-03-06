SET FOREIGN_KEY_CHECKS = 0; 
DROP TABLE IF EXISTS Member;
CREATE TABLE Member (
  id INTEGER UNSIGNED  NOT NULL  AUTO_INCREMENT,
  name VARCHAR(30) NOT NULL,
  date_added  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  role  VARCHAR(30) NOT NULL,
  profile_photo_format INTEGER NOT NULL DEFAULT 0, /* 0:NO_PHOTO, 1:JPG, 2:PNG */
  
  PRIMARY KEY(id),
  CONSTRAINT unique_member_name_ck UNIQUE(name)
);
ALTER TABLE Member AUTO_INCREMENT=51;

DROP TABLE IF EXISTS Photo;
/*CREATE TABLE Photo (
  id INTEGER UNSIGNED  NOT NULL  AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  file_path VARCHAR(100) NOT NULL,
  
  PRIMARY KEY(id),
  INDEX Photo_FKIndex1(member_id),
  FOREIGN KEY(member_id) REFERENCES Member(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);*/

/*
ALTER TABLE Member ADD CONSTRAINT ProfilePhoto_FK FOREIGN KEY(profile_photo) REFERENCES Photo(id) ON DELETE NO ACTION ON UPDATE NO ACTION;
*/

DROP TABLE IF EXISTS Fingerprint;
CREATE TABLE Fingerprint (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  fingerprint_id INTEGER UNSIGNED  NOT NULL,
  
  PRIMARY KEY(id),
  CONSTRAINT unique_member_fingerprint_ck UNIQUE(member_id),
  CONSTRAINT unique_fingerprint_member_ck UNIQUE(fingerprint_id),
  INDEX Fingerprint_FKIndex1(member_id),
  FOREIGN KEY(member_id) REFERENCES Member(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);

DROP TABLE IF EXISTS Administrator;
CREATE TABLE Administrator (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL  ,
  password_hash TINYBLOB NOT NULL,
  date_added  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  PRIMARY KEY(id),
  CONSTRAINT unique_member_administrator_ck UNIQUE(member_id),
  INDEX Administrator_FKIndex1(member_id),
  FOREIGN KEY(member_id) REFERENCES Member(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);


DROP TABLE IF EXISTS Entry;
CREATE TABLE Entry (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  info  VARCHAR(30) NOT NULL,
  
  PRIMARY KEY(id),
  INDEX Entry_FKIndex1(member_id),
  FOREIGN KEY(member_id) REFERENCES Member(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);
      
DROP TABLE IF EXISTS EntryRead;
CREATE TABLE EntryRead (
  member_id INTEGER UNSIGNED  NOT NULL,
  entry_id INTEGER UNSIGNED  NOT NULL,

  PRIMARY KEY(member_id, entry_id),
  INDEX EntryRead_Member_FKIndex1(member_id),
  FOREIGN KEY(member_id) REFERENCES Member(id) ON DELETE NO ACTION ON UPDATE NO ACTION,
  INDEX EntryRead_Entry_FKIndex1(entry_id),
  FOREIGN KEY(entry_id) REFERENCES Entry(id) ON DELETE NO ACTION ON UPDATE NO ACTION
 );
SET FOREIGN_KEY_CHECKS = 1; 

