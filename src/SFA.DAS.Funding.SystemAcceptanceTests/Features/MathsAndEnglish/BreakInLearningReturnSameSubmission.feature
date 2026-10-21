Feature: BreakInLearningReturnSameSubmission

---------- LearnAimRef used ----------
60075880 => Amount: 471.00000

Background:
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And a Maths and English learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880 and learning support from currentAY-08-05 to currentAY-04-10
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R08 with instalment amount 58.88 for learnAimRef 60075880
	And maths and english learning support earnings are generated from periods currentAY-R01 to currentAY-R08

#FLP-1421 - AC2	
@regression @ignoreInPREPRODandPP
Scenario: Training provider records a return from E&M break in learning - Pause and return in same ilr submission
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with new start date currentAY-01-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R08 with regular instalment amount 78.50 for learnAimRef 60075880

#---------------------- FLP-1421 - Additional Scenarios -------------------------------------------------------------------
@regression @ignoreInPREPRODandPP
Scenario: Training provider records a return from E&M break in learning with expected end date pushed back
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and end date currentAY-06-20
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R10 with instalment amount 47.10 for learnAimRef 60075880

@regression @ignoreInPREPRODandPP
Scenario: Training provider records a return from E&M break in learning with withdrawal date during second period in learning
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and withdrawal date currentAY-03-25
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R07 with instalment amount 78.50 for learnAimRef 60075880


@regression @ignoreInPREPRODandPP
Scenario: Training provider records a return from E&M break in learning with completion date during second period in learning
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-12-25, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with a new start date currentAY-01-28 and completion date currentAY-03-25
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R04 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R06 to currentAY-R07 with instalment amount 78.50 for learnAimRef 60075880
	And a Maths and English balancing earning of 78.50 is generated for learnAimRef 60075880 for period currentAY-R08

@regression @ignoreInPREPRODandPP
Scenario: Training provider records multiple retuns from E&M break in learning
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-10-25, learning support from currentAY-08-05 to currentAY-04-10
	And SLD record a return from break in learning for English and Maths course with new start date currentAY-11-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R02 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R04 to currentAY-R08 with instalment amount 70.65 for learnAimRef 60075880
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-10-25, learning support from currentAY-08-05 to currentAY-04-10
	And English and Maths learning is recorded from currentAY-11-15 to currentAY-04-10 with learnAimRef 60075880, pause date currentAY-02-17, learning support from currentAY-08-05 to currentAY-04-10
	When SLD record a return from break in learning for English and Maths course with new start date currentAY-03-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R02 with regular instalment amount 58.88 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R04 to currentAY-R06 with regular instalment amount 70.65 for learnAimRef 60075880
	And Maths and English earnings are generated from periods currentAY-R08 to currentAY-R08 with instalment amount 141.30 for learnAimRef 60075880