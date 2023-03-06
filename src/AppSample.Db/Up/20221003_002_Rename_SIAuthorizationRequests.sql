
ALTER TABLE Si_Authorization_Requests
	RENAME TO Authorization_Requests;

ALTER INDEX "IX_SiAuthorizationRequests_AuthorizationRequestId"
	RENAME TO "IX_AuthorizationRequests_AuthorizationRequestId";

ALTER INDEX "IX_SiAuthorizationRequests_ConsentCode"
	RENAME TO "IX_AuthorizationRequests_ConsentCode";