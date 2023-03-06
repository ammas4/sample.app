-- ----------------------------
-- Table structure for Admin_Users
-- ----------------------------

CREATE TABLE Admin_Users (
  Id serial NOT NULL,
  Login text,
  Role int4 NOT NULL,
  Active bool NOT NULL,                                       
  Created_At timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  Updated_At timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE Admin_Users ADD CONSTRAINT AdminUsers_pkey PRIMARY KEY (Id);
