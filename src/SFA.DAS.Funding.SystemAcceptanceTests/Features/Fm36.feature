Feature: Fm36

Retrieve Fm36 data

@regression
Scenario: Retrieve Valid Fm36 data
	Given there is an apprenticeship
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then fm36 data exists for that apprenticeship
