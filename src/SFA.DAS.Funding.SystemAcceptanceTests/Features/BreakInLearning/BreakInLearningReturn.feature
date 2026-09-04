Feature: BreakInLearningReturn

As the Dfe
I want the apprenticeship earnings to be recalculated when a return from break in learning is recorded
So that the provider acquires earnings once the learner has returned from a break

Background:
	Given a learning has a start date of currentAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 05 and academic year currentAY are soft deleted


#FLP-1360 AC1 currentAY return
@regression
Scenario: Training provider records a return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	When SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 558.82353 between currentAY-R08 and nextAY-R12 are maintained
	And earnings are updated with first period in learning from currentAY-08-01 to currentAY-01-15
	And earnings are updated with second period in learning from currentAY-03-01 to null

#FLP-1360 AC1 current AY return
@regression
Scenario: Training provider records a return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date nextAY-05-01
	When SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 3166.66667 between nextAY-R10 and nextAY-R12 are maintained

#FLP-1360 AC2 see BreakInLearningBreakAndReturnInSameSubmission.feature

#FLP-1360 AC3 training provider corrects previously recorded return currentAY
@regression
Scenario: Training provider corrects a previous recorded return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us of a correction to a previously recorded return from break in learning with a new learning start date currentAY-06-01
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 678.57143 between currentAY-R11 and nextAY-R12 are maintained

#FLP-1360 AC3 training provider corrects previously recorded return nextAY
@regression
Scenario: Training provider corrects a previous recorded return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date nextAY-05-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us of a correction to a previously recorded return from break in learning with a new learning start date nextAY-06-01
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 4750.00000 between nextAY-R11 and nextAY-R12 are maintained

#FLP-1360 AC4 training provider removes previously recorded return currentAY
@regression
Scenario: Training provider removes a previously recorded return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that a previously recorded return from a break in learning is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained

#FLP-1360 AC4 training provider removes previously recorded return nextAY
@regression
Scenario: Training provider removes a previously recorded return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date nextAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that a previously recorded return from a break in learning is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained

#@regression
#FLP-1360 AC4 training provider removes previously recorded return & entire break currentAY
@regression
Scenario: Training provider removes a previously recorded return from, and break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that an entire previously recorded break in learning and return is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and nextAY-R12 are maintained

#FLP-1360 AC4 training provider removes previously recorded return & entire break nextAY
@regression
Scenario: Training provider removes a previously recorded return from, and break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date nextAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that an entire previously recorded break in learning and return is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and nextAY-R12 are maintained