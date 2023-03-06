-- ----------------------------
-- Table structure for Authenticator_Chains
-- ----------------------------
CREATE TABLE authenticator_chains (
  id serial NOT NULL,
  service_provider_id integer not null,
  order_level_1 integer not null,
  order_level_2 integer not null,
  authenticator_type integer not null,
  next_chain_start_delay integer not null,
  created_at timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at timestamptz(6) NOT NULL DEFAULT CURRENT_TIMESTAMP
);