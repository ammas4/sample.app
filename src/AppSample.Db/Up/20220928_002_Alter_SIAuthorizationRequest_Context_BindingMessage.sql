-- ----------------------------
-- Add columns context and binding_message to table si_authorization_requests
-- ----------------------------
alter table si_authorization_requests
  add column context text,
  add column binding_message text;

