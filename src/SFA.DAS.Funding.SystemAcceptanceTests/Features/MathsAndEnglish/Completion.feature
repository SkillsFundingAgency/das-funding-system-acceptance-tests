Feature: Maths and English Completion

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

When M&E is marked as "Complete", earnings for subsequent delivery periods are "rolled up" into a single Balancing earning

---------- LearnAimRef used ----------
60075880 => Amount: 471.00000
60147003 => Amount: £864.00000
60342651 => Amount £941.00000
60342869 => Amount £724.00000
	
@regression @ignoreInPREPRODandPP
Scenario: Balancing earnings for Maths and English on Completion
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60075880 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 67.29 for learnAimRef 60075880
	And a Maths and English balancing earning of 201.86 is generated for learnAimRef 60075880 for period currentAY-R06

@regression @ignoreInPREPRODandPP
Scenario: Balancing earnings for Maths and English on Completion moved earlier
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60147003 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 123.43 for learnAimRef 60147003
	And a Maths and English balancing earning of 370.29 is generated for learnAimRef 60147003 for period currentAY-R06
	And SLD resubmits ILR
	And English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60147003 and completion on currentAY-12-15
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R04 with regular instalment amount 123.43 for learnAimRef 60147003
	And a Maths and English balancing earning of 493.71 is generated for learnAimRef 60147003 for period currentAY-R05

@regression @ignoreInPREPRODandPP
Scenario: Balancing earnings for Maths and English on Completion moved later
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342651 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 134.43 for learnAimRef 60342651
	And a Maths and English balancing earning of 403.29 is generated for learnAimRef 60342651 for period currentAY-R06
	And SLD resubmits ILR
	And English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342651 and completion on currentAY-02-20
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R06 with instalment amount 134.43 for learnAimRef 60342651
	And a Maths and English balancing earning of 268.86 is generated for learnAimRef 60342651 for period currentAY-R07

@regression @ignoreInPREPRODandPP
Scenario: Balancing earnings for Maths and English on Completion - Completion removed
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342869 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 103.43 for learnAimRef 60342869
	And a Maths and English balancing earning of 310.29 is generated for learnAimRef 60342869 for period currentAY-R06
	And SLD resubmits ILR
	And Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342869
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R08 with regular instalment amount 103.43 for learnAimRef 60342869
	And Maths and English balancing earning is removed for learnAimRef 60342869

@regression @ignoreInPREPRODandPP
Scenario: Balancing earnings for Maths and English - Completion in same period as planned end date
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-30 with learnAimRef 60342869 and completion on currentAY-04-07
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R08 with instalment amount 90.5 for learnAimRef 60342869
	And a Maths and English balancing earning of 90.5 is generated for learnAimRef 60342869 for period currentAY-R09

@regression @ignoreInPREPRODandPP
Scenario: No Balancing earnings for Maths and English - Completion after planned end date
	Given an apprenticeship has a start date of currentAY-09-25, a planned end date of currentAY-04-15, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from currentAY-09-25 to currentAY-04-30 with learnAimRef 60342869 and completion on currentAY-06-27
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R09 with instalment amount 90.50 for learnAimRef 60342869
	And No Maths and English balancing earning for learnAimRef 60342869 is generated