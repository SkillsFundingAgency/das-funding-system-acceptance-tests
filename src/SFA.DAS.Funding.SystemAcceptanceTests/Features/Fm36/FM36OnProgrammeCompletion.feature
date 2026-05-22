@nonparallelizable
Feature: FM36OnProgrammeCompletion

Fm36 Withdrawl tests

@regression
Scenario: On programme completion
	Given a learning has a start date of previousAY-08-01, a planned end date of previousAY-07-31 and an agreed price of 15000
	When Learning Completion is recorded on previousAY-06-15
	And Learning Achievement date is recorded on previousAY-07-01
	And SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date previousAY-07-31
	And SLD submit updated learners details
	And the fm36 data is retrieved for previousAY-07-25
	Then PriceEpisodeActualEndDateIncEPA is previousAY-06-15
	And PriceEpisodeBalancePayment for period previousAY-R11 is amount 2000
	And PriceEpisodeCompletionPayment for period previousAY-R12 is amount 3000

