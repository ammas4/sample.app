-- ----------------------------
-- Table structure for Service_Providers
-- ----------------------------
CREATE TABLE Service_Providers (
  Id serial NOT NULL,
  Name text,
  Client_Id text,
  Client_Secret text,
  TTL int4,
  Jwks_Content text,
  Notification_Urls text,
  Redirect_Urls text,
  Active bool NOT NULL,
  Deleted bool NOT NULL,
  Auth_Mode int4 NOT NULL,
  Created_At timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  Updated_At timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP
);
