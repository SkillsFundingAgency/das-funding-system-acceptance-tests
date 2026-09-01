@nonparallelizable
Feature: FM36OnProgrammeCompletion

Fm36 Withdrawl tests

@regression
Scenario: On programme completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When Learning Completion is recorded on currentAY-06-15
	And Learning Achievement date is recorded on currentAY-07-01
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	And the fm36 data is retrieved for currentAY-07-25
	Then PriceEpisodeActualEndDateIncEPA is currentAY-06-15
	And PriceEpisodeBalancePayment for period currentAY-R11 is amount 2000
	And PriceEpisodeCompletionPayment for period currentAY-R12 is amount 3000

