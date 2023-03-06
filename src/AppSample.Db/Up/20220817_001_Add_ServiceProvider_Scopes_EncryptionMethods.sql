-- ----------------------------
-- Add columns Scopes and EncryptionMethod to table Service_Providers
-- ----------------------------
ALTER TABLE Service_Providers
  ADD Scopes text,
  ADD EncryptionMethod int4 DEFAULT 1 NOT NULL;