alter table si_authorization_requests alter column nonce type varchar(200);
alter table si_authorization_requests alter column nonce drop not null;
