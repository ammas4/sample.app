
BEGIN TRANSACTION;

ALTER TABLE Authorization_Requests ADD Redirect_Uri text;

ALTER TABLE Authorization_Requests ADD State text;

ALTER TABLE Authorization_Requests ADD Mode smallint;

UPDATE Authorization_Requests SET Mode=1; /* MobileIdMode.SI */

ALTER TABLE Authorization_Requests ALTER COLUMN Mode SET NOT NULL;

COMMIT TRANSACTION;