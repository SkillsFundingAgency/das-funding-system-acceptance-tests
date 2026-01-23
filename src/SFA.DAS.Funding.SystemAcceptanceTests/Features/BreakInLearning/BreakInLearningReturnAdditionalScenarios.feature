Feature: BreakInLearningReturnAdditionalScenarios

As the Dfe
I want the apprenticeship earnings to be recalculated when a return from break in learning is recorded in more complex scenarios
So that the provider acquires earnings once the learner has returned from a break

#NB This file has been added as it's own feature as the Background section which sets up a simple BIL in the main feature file is not relevant for these more complex scenarios.


#TODO when earnings event is fixed
#BIL followed by another BiL a few months later (BIL and return same time -> BiL again after 3 months -> return (in my head) )
@regression
Scenario: Training provider records multiple breaks in learning with returns
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date previousAY-03-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a break in learning with pause date previousAY-06-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a return from break in learning with a new learning start date currentAY-09-01
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 558.82353 between previousAY-R08 and previousAY-R10 are maintained
	And the earnings of 711.22995 between currentAY-R02 and currentAY-R12 are maintained

#BiL , return then Completion 
@regression
Scenario: Training provider records break in learning, return, then completion
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-09-01
	And Learning Completion is recorded on currentAY-12-01
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 863.63686 between currentAY-R02 and currentAY-R04 are maintained
	#6x new payment post break of 863.63636 to cover R05 to R12 inclusive = 6909.09
	And an earning of 6909.09 of type Balancing is generated for period currentAY-R05
	And an earning of 3000 of type Completion is generated for period currentAY-R05


#End date pushed back to account for BIL with no price change
@regression
Scenario: Training provider pushes end date back to account for break in learning with no price change
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-12-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 1000 between previousAY-R10 and currentAY-R05 are maintained

#Apprenticeship duration is increased after BIL with no price change
@regression
Scenario: Training provider increases duration after break in learning with no price change
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-03-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 727.27 between previousAY-R10 and currentAY-R08 are maintained

#Apprenticeship duration is increased after BIL with price increase
@regression
Scenario: Training provider increases duration after break in learning with price increase
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-03-31
	And SLD record latest on-programme cost as total price 17000
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 872.72727 between previousAY-R10 and currentAY-R08 are maintained

#Withdrawal on the same day after BIL return
@regression
Scenario: Training provider withdraws apprenticeship on the same day as return from break in learning
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with a new learning start date previousAY-05-01
	And Learning withdrawal date is recorded on previousAY-05-01
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained

#Withdrawal after 3 months of their return from BIL
@regression
Scenario: Training provider withdraws apprenticeship 3 months after return from break in learning
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with a new learning start date previousAY-05-01
	And Learning withdrawal date is recorded on currentAY-08-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 1600 between previousAY-R10 and previousAY-R12 are maintained
