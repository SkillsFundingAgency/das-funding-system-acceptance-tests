Feature: Healthcheck

A short summary of the feature

@ignore
Scenario: Check test can call healthcheck endpoint

	When Call Learner Data API healthcheck endpoint
	Then Healthcheck returns 200 OK
