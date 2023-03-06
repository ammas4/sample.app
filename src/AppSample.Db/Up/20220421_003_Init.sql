-- ----------------------------
-- Table structure for Si_Authorization_Requests
-- ----------------------------
CREATE TABLE Si_Authorization_Requests (
  Id bigserial NOT NULL,
  Authorization_Request_Id uuid,
  Notification_Uri text,
  Notification_Token text,
  Service_Provider_Id int4 NOT NULL,
  Scope text,
  Acr_Values text,
  Response_Type text,
  Msisdn text,
  Nonce uuid NOT NULL,
  Consent_Code uuid,
  Created_At timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_SiAuthorizationRequests_AuthorizationRequestId"
    ON public.Si_Authorization_Requests USING btree
    (Authorization_Request_Id);

CREATE INDEX IF NOT EXISTS "IX_SiAuthorizationRequests_ConsentCode"
    ON public.Si_Authorization_Requests USING btree
    (Consent_Code);
