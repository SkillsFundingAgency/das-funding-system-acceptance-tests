Feature: BreakInLearning

As the Dfe
I want the English and/or Maths earnings to be recalculated when a break in learning is recorded 
So that the provider does not accure earnings while the learner is on a break


Background: 
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And a Maths and English learning is recorded from currentAY-08-05 to currentAY-04-10 with course English Foundation, amount 800, learning support from currentAY-08-05 to currentAY-04-10
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R08 with instalment amount 100 for course English Foundation
	And learning support earnings are generated from periods currentAY-R01 to currentAY-R08
	
@regression
Scenario: Training provider records a break in learning without specifying a return
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	When English and Maths learning is recorded from currentAY-08-05 to currentAY-04-10 with course English Foundation, amount 800, pause date currentAY-02-25, learning support from currentAY-08-05 to currentAY-04-10
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R06 with instalment amount 100 for course English Foundation
	#And learning support earnings are generated from periods currentAY-R01 to currentAY-R06
