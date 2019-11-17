CREATE TABLE Member (
  id INTEGER UNSIGNED  NOT NULL  AUTO_INCREMENT,
  name VARCHAR(30) NOT NULL,
  date_added  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  role  VARCHAR(30) NOT NULL,
  profile_photo INTEGER UNSIGNED,
PRIMARY KEY(id),
CONSTRAINT unique_member_name_ck UNIQUE(name),
INDEX ProfilePhoto_FK(profile_photo),
  FOREIGN KEY(profile_photo)
    REFERENCES Photo(id)
      ON DELETE NO ACTION
      ON UPDATE NO ACTION);
);



CREATE TABLE Photo (
  id INTEGER UNSIGNED  NOT NULL  AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  file_path VARCHAR(100) NOT NULL,
PRIMARY KEY(id),
INDEX Photo_FKIndex1(member_id),
  FOREIGN KEY(member_id)
    REFERENCES Member(id)
      ON DELETE NO ACTION
      ON UPDATE NO ACTION);



CREATE TABLE Fingerprint (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  fingerprint_id INTEGER UNSIGNED  NOT NULL,
PRIMARY KEY(id),
CONSTRAINT unique_member_fingerprint_ck UNIQUE(member_id),
CONSTRAINT unique_fingerprint_member_ck UNIQUE(fingerprint_id),
INDEX Fingerprint_FKIndex1(member_id),
  FOREIGN KEY(member_id)
    REFERENCES Member(id)
      ON DELETE NO ACTION
      ON UPDATE NO ACTION);



CREATE TABLE Administrator (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL  ,
  password_hash TINYBLOB NOT NULL,
  date_added  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
PRIMARY KEY(id),
CONSTRAINT unique_member_administrator_ck UNIQUE(member_id),
INDEX Administrator_FKIndex1(member_id),
  FOREIGN KEY(member_id)
    REFERENCES Member(id)
      ON DELETE NO ACTION
      ON UPDATE NO ACTION);



CREATE TABLE Entry (
  id INTEGER UNSIGNED  NOT NULL   AUTO_INCREMENT,
  member_id INTEGER UNSIGNED  NOT NULL,
  date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
PRIMARY KEY(id)  ,
INDEX Entry_FKIndex1(member_id),
  FOREIGN KEY(member_id)
    REFERENCES Member(id)
      ON DELETE NO ACTION
      ON UPDATE NO ACTION);




