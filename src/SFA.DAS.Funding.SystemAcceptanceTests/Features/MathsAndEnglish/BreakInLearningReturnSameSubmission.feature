Feature: BreakInLearningReturnSameSubmission

Background:
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And a Maths and English learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, learning support from currentAY-08-05 to currentAY-04-10
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R08 with instalment amount 100 for course English Foundation
	And learning support earnings are generated from periods currentAY-R01 to currentAY-R08

#FLP-1421 - AC2	
@regression
Scenario: Training provider records a return from E&M break in learning - Pause and return in same ilr submission
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with new start date currentAY-01-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R08 with regular instalment amount 133.33000 for course English Foundation

#---------------------- FLP-1421 - Additional Scenarios -------------------------------------------------------------------
@regression
Scenario: Training provider records a return from E&M break in learning with expected end date pushed back
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and end date currentAY-06-20
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R10 with instalment amount 80 for course English Foundation

@regression
Scenario: Training provider records a return from E&M break in learning with withdrawal date during second period in learning
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and withdrawal date currentAY-03-25
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R07 with instalment amount 133.33000 for course English Foundation


@regression
Scenario: Training provider records a return from E&M break in learning with completion date during second period in learning
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60342843, course English Foundation, amount 800, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and completion date currentAY-03-25
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 100 for course English Foundation
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R07 with instalment amount 133.33000 for course English Foundation
	And a Maths and English balancing earning of 133.33000 is generated for course English Foundation for period currentAY-R08