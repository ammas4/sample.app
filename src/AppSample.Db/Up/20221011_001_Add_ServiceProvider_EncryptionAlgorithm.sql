-- ----------------------------
-- Add columns encryption_algorithm to table Service_Providers
-- ----------------------------
ALTER TABLE Service_Providers
  ADD encryption_algorithm int4 DEFAULT 1 NOT NULL;