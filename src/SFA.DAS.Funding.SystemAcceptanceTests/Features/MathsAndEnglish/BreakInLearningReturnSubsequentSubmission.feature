Feature: BreakInLearningReturnSubsequentSubmission

As the Dfe
I want the English and Maths earnings to be recalculated when a return from break in learning is recorded
So that the provider acquires earnings once the learner has returned from a break

Background: 
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with instalment amount 100 for course English Foundation
	And learning support earnings are generated from periods currentAY-R01 to currentAY-R04

#FLP-1421 - AC1
@regression
Scenario: Training provider records a return from E&M break in learning
	Given SLD record a return from break in learning for English and Maths course with new start date currentAY-01-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R08 with instalment amount 133.33000 for course English Foundation

#FLP-1421 - AC3
@regression
Scenario: Training provider corrects a return from E&M break in learning 
	Given SLD record a return from break in learning for English and Maths course with new start date currentAY-01-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R08 with instalment amount 133.33000 for course English Foundation
	When SLD inform us of a correction to an English and Maths return from break in learning with new start date currentAY-02-05
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R07 to currentAY-R08 with instalment amount 200 for course English Foundation

#FLP-1421 - AC4
@regression
Scenario: Training provider removes a previously recorded return from E&M break in learning 
	Given SLD record a return from break in learning for English and Maths course with new start date currentAY-01-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R08 with instalment amount 133.33000 for course English Foundation
	When SLD inform us that a previously recorded english and maths return from break in learning is removed
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with instalment amount 100 for course English Foundation
