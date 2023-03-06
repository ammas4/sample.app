ALTER TABLE authenticator_chains 
ADD CONSTRAINT order_level_1_positive CHECK (order_level_1 >= 0);
ALTER TABLE authenticator_chains 
ADD CONSTRAINT order_level_2_positive CHECK (order_level_2 >= 0);
