-- ----------------------------
-- Add columns otp_key. Add index on otp_key
-- ----------------------------
ALTER TABLE authorization_requests
  ADD COLUMN otp_key uuid;

CREATE INDEX IF NOT EXISTS "IX_SiAuthorizationRequests_OtpKey"
    ON public.authorization_requests USING btree (otp_key);