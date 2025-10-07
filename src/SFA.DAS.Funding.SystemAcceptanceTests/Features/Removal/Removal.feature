Feature: Removal

At the Dfe
I want to know when an apprentice is removed from the ILR
So that earnings and payments can be recalculated based on the latest data 

@regression
Scenario: Apprentice removed from the ILR
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When sld inform us that the learner is to removed
	Then the apprenticeship is marked as withdrawn
	And last day of learning is set to currentAY-08-01 in learning db
	And the expected number of earnings instalments after withdrawal are 0
	And a learning removed event is published to approvals with reason withdrawn and last day of learning as currentAY-08-01