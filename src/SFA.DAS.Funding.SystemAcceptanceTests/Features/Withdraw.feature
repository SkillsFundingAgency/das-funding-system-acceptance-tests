Feature: Withdraw


@regression
Scenario: Test Withdraw
	Given there is an apprenticeship
	And the apprenticeship commitment is approved
	When the apprenticeship is withdrawn
	Then the apprenticeship is marked as withdrawn

# further tests such as cancelling pending requests