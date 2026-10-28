Feature: Removal

At the Dfe
I want to know when an apprentice is removed from the ILR
So that earnings and payments can be recalculated based on the latest data

Background:
	Given an apprenticeship has a start date of nextAY-08-01, a planned end date of nextAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And the age at the start of the apprenticeship is 17
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details

@regression @ignoreInPREPRODandPP
Scenario: Apprentice removed from the ILR
	Given a Maths and English learning is recorded from nextAY-08-01 to currentAYPlusTwo-08-23 with learnAimRef 60075880 and learning support from nextAY-10-12 to nextAY-02-15
	And SLD inform us of a break in learning with pause date nextAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date nextAY-02-20
	And SLD submit updated learners details
	When sld inform us that the learner is to removed
	Then earnings are recalculated
	And the apprenticeship earnings are sent to payments 
	And the functionalSkill earnings are sent to payments
	And Maths and English earnings for learnAimRef 60075880 are removed
	And there are 0 on-programme earnings instalments
	And the first incentive earning is_not generated for provider & employer
	And the second incentive earning is_not generated for provider & employer
	And no learning support earnings are generated
	And no apprenticeship earnings are sent to payments
	And no functionalSkill earnings are sent to payments
	And Break in Learning record is removed from earnings db
	And a learning removed event is published to approvals

# FLP-2005 AC5
@regression @ignoreInPREPRODandPP
Scenario: Functional skill learning support earnings are removed from payments when apprenticeship is removed
	Given a Maths and English learning is recorded from nextAY-08-01 to currentAYPlusTwo-04-15 with learnAimRef 60075880 and learning support from currentAYPlusTwo-01-12 to currentAYPlusTwo-04-15
	And SLD submit updated learners details
	Then maths and english learning support earnings are generated from periods currentAYPlusTwo-R06 to currentAYPlusTwo-R08
	And the functionalSkill earnings are sent to payments
	And functionalSkill learning support earnings are sent to payments from periods currentAYPlusTwo-R06 to currentAYPlusTwo-R08
	When sld inform us that the learner is to removed
	Then earnings are recalculated
	And no learning support earnings are generated
	And the functionalSkill earnings are sent to payments
	And no functionalSkill learning support earnings are sent to payments
	And no functionalSkill earnings are sent to payments

# FLP-1781 AC1
@regression @ignoreInPREPRODandPP
Scenario: Apprentice removed from the ILR and Get Learners endpoint called
	When sld inform us that the learner is to removed
	And SLD want to know the learners already on Apprenticeship service for a provider
	Then the apprentice is not returned in the GetLearners response

# FLP-1781 AC2
@regression @ignoreInPREPRODandPP
Scenario: Apprentice removed from the ILR and Get Earnings endpoint called
	When sld inform us that the learner is to removed
	And the fm36 data is retrieved for currentDate
	Then fm36 data does not exist for that apprenticeship

# FLP-2005 AC2
@regression @ignoreInPREPRODandPP
Scenario: Inform Approvals of approved apps reinstated
	Given sld inform us that the learner is to removed
	And a learning removed event is published to approvals
	When sld inform us that learner is reinstated by the provider
	Then a learning reinstated event is published to approvals
