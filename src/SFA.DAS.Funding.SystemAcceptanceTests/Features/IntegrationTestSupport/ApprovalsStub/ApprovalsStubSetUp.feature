Feature: Approvals stub set-up

Checks that the tests can set up das-api-stub to stand in for Approvals' PUT approvals/{learningKey}.
The mapping is removed again when the scenario ends.

Scenario: Set up an auto-approval for a start date change
	Given Approvals will auto-approve a start date change from currentAY-08-01 to currentAY-09-01 for a new learning key
