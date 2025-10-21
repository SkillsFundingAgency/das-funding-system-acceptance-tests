Feature: Fm36Paging

This feature tests paging functionality of endpoint

@tag1
Scenario: No paging parameters provided 
	Given that there is at least 15 records available from FM36 endpoint
	When a request is made without paging parameters
	Then the response should be unpaged

Scenario: Paging parameters provided
	Given that there is at least 15 records available from FM36 endpoint
	When a request is made with page number 2 and page size 5
	Then the response should contain 5 records for page 2
	And link to previous page should be present
