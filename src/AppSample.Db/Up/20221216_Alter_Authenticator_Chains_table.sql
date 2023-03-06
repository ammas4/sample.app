ALTER TABLE authenticator_chains 
ADD CONSTRAINT uc_authenticator_chains
UNIQUE (service_provider_id, order_level_1, order_level_2);	