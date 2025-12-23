Feature: Removal

As a earnings calc
I want to know if an English/maths course has been removed from the ILR
So that earnings can be removed

@regression
Scenario: English and Maths course is removed
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 to currentAY-01-07 with course Entry level English and amount 1000
	And SLD submit updated learners details
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And the maths and english courses are removed
	And SLD submit updated learners details
	Then Maths and English earnings for course Entry level English are removed