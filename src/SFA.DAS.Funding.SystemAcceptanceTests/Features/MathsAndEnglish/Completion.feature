Feature: Maths and English Completion

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

When M&E is marked as "Complete", earnings for subsequent delivery periods are "rolled up" into a single Balancing earning
	
@regression
Scenario: Balancing earnings for Maths and English on Completion
	Given a learning has a start date of currentAY-09-25, a planned end date of currentAY-04-15 and an agreed price of 12000
	When Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 399 is generated for course Entry level English for period currentAY-R06

@regression
Scenario: Balancing earnings for Maths and English on Completion moved earlier
	Given a learning has a start date of currentAY-09-25, a planned end date of currentAY-04-15 and an agreed price of 12000
	When Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 399 is generated for course Entry level English for period currentAY-R06
	And SLD resubmits ILR
	And Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-12-15
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R04 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 532 is generated for course Entry level English for period currentAY-R05

@regression
Scenario: Balancing earnings for Maths and English on Completion moved later
	Given a learning has a start date of currentAY-09-25, a planned end date of currentAY-04-15 and an agreed price of 12000
	When Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 399 is generated for course Entry level English for period currentAY-R06
	And SLD resubmits ILR
	And Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-02-20
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R06 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 266 is generated for course Entry level English for period currentAY-R07

@regression
Scenario: Balancing earnings for Maths and English on Completion - Completion removed
	Given a learning has a start date of currentAY-09-25, a planned end date of currentAY-04-15 and an agreed price of 12000
	When Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English, amount 931 and completion on currentAY-01-01
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods currentAY-R02 to currentAY-R05 with regular instalment amount 133 for course Entry level English
	And a Maths and English balancing earning of 399 is generated for course Entry level English for period currentAY-R06
	And SLD resubmits ILR
	And Maths and English learning is recorded from currentAY-09-25 to currentAY-04-15 with learnAimRef 60342843, course Entry level English and amount 931
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R08 with regular instalment amount 133 for course Entry level English
	And Maths and English balancing earning is removed for course Entry level English

@regression
Scenario: Balancing earnings for Maths and English - Completion in same period as planned end date
	Given a learning has a start date of currentAY-09-25, a planned end date of currentAY-04-15 and an agreed price of 12000
	When Maths and English learning is recorded from currentAY-09-25 to currentAY-04-30 with learnAimRef 60342843, course Entry level English, amount 1000 and completion on currentAY-04-07
	And SLD record on-programme cost as total price 12000 from date currentAY-09-25 to date currentAY-04-15
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R02 to currentAY-R08 with regular instalment amount 125 for course Entry level English
	And a Maths and English balancing earning of 125 is generated for course Entry level English for period currentAY-R09