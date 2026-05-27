Feature: Removal

At the Dfe
I want to know when an apprentice is removed from the ILR
So that earnings and payments can be recalculated based on the latest data

Background:
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And the age at the start of the apprenticeship is 17
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details

@regression
Scenario: Apprentice removed from the ILR
	Given a Maths and English learning is recorded from currentAY-08-01 to nextAY-08-23 with learnAimRef 60342843, course Entry level English, amount 1200, learning support from currentAY-10-12 to currentAY-02-15
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-02-20
	And SLD submit updated learners details
	When sld inform us that the learner is to removed
	Then earnings are recalculated
	And Maths and English earnings for course Entry level English are removed
	And there are 0 on-programme earnings instalments
	And the first incentive earning is_not generated for provider & employer
	And the second incentive earning is_not generated for provider & employer
	And no learning support earnings are generated
	And Break in Learning record is removed from earnings db
	And a learning removed event is published to approvals

#Commented - to be added in FLP-1781
#
#Scenario: Apprentice removed from the ILR and Get Learners endpoint called
#	When sld inform us that the learner is to removed
#	And SLD want to know the learners already on Apprenticeship service for a provider
#	Then the apprentice is not returned in the GetLearners response
#
#Scenario: Apprentice removed from the ILR and Get Earnings endpoint called
#	When sld inform us that the learner is to removed
#	And the fm36 data is retrieved for currentDate
#	Then fm36 data does not exist for that apprenticeship
