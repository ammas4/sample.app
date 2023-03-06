CREATE TABLE IF NOT EXISTS settings
(
    name text NOT NULL,
    value text ,
    CONSTRAINT settings_pkey PRIMARY KEY (name)
);

INSERT INTO settings (name, value) VALUES ('Migration.Force.SmsUrl', false);