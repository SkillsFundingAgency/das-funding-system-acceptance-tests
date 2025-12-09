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
	#And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date previousAY-03-15
	And SLD submit updated learners details
	#And earnings are recalculated
	And SLD inform us of a break in learning with pause date previousAY-06-15
	And SLD submit updated learners details
	#And earnings are recalculated
	And SLD inform us of a return from break in learning with a new learning start date currentAY-09-01
	And SLD submit updated learners details
	#And earnings are recalculated
	Then the earnings between previousAY-R01 and previousAY-R05 are maintained
	And the earnings between previousAY-R06 and previousAY-R07 are soft deleted
	And the earnings between previousAY-R08 and previousAY-R10 are maintained
	And the earnings between previousAY-R11 and currentAY-R01 are soft deleted
	And the earnings between currentAY-R02 and currentAY-R12 are maintained

#BiL , return then Completion 
@regression
Scenario: Training provider records break in learning, return, then completion
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-09-01
	#And SLD submit updated learners details
	#And earnings are recalculated
	And Learning Completion is recorded on currentAY-12-01
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings between previousAY-R01 and previousAY-R05 are maintained
	And the earnings between previousAY-R06 and currentAY-R01 are soft deleted
	And the earnings between currentAY-R02 and currentAY-R04 are maintained
	And an earning of 6000 of type Balancing is generated for period currentAY-R05
	And an earning of 3000 of type Completion is generated for period currentAY-R05

	#Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	#And an earning of 2000 of type Balancing is generated for period currentAY-R11
	#And an earning of 3000 of type Completion is generated for period currentAY-R11


#Bil, return then Price increased and end date pushed back
#It will be good to include a withdrawal at the time of return.
#check which of Pawan's spreadsheet examples are yet to be covered and do them too