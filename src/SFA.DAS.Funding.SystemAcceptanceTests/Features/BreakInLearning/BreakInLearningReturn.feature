Feature: BreakInLearningReturn

As the Dfe
I want the apprenticeship earnings to be recalculated when a return from break in learning is recorded
So that the provider acquires earnings once the learner has returned from a break

Background:
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 05 and academic year previousAY are soft deleted

#FLP-1360 AC1 previousAY return
@regression
Scenario: Training provider records a return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date previousAY-03-01
	When SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 558.82353 between previousAY-R08 and currentAY-R12 are maintained

#FLP-1360 AC1 current AY return
@regression
Scenario: Training provider records a return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-05-01
	When SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 3166.66667 between currentAY-R10 and currentAY-R12 are maintained

#FLP-1360 AC2 see BreakInLearningBreakAndReturnInSameSubmission.feature

#FLP-1360 AC3 training provider corrects previously recorded return previousAY
@regression
Scenario: Training provider corrects a previous recorded return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date previousAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us of a correction to a previously recorded return from break in learning with a new learning start date previousAY-06-01
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 678.57143 between previousAY-R11 and currentAY-R12 are maintained

#FLP-1360 AC3 training provider corrects previously recorded return currentAY
@regression
Scenario: Training provider corrects a previous recorded return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-05-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us of a correction to a previously recorded return from break in learning with a new learning start date currentAY-06-01
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 4750.00000 between currentAY-R11 and currentAY-R12 are maintained

#FLP-1360 AC4 training provider removes previously recorded return previousAY
@regression
Scenario: Training provider removes a previously recorded return from a break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date previousAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that a previously recorded return from a break in learning is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained

#FLP-1360 AC4 training provider removes previously recorded return currentAY
@regression
Scenario: Training provider removes a previously recorded return from a break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that a previously recorded return from a break in learning is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained

#@regression
#FLP-1360 AC4 training provider removes previously recorded return & entire break previousAY
@regression
Scenario: Training provider removes a previously recorded return from, and break in learning in previous academic year
	Given SLD inform us of a return from break in learning with a new learning start date previousAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that an entire previously recorded break in learning and return is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and currentAY-R12 are maintained

#FLP-1360 AC4 training provider removes previously recorded return & entire break currentAY
@regression
Scenario: Training provider removes a previously recorded return from, and break in learning in current academic year
	Given SLD inform us of a return from break in learning with a new learning start date currentAY-03-01
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD inform us that an entire previously recorded break in learning and return is removed
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between previousAY-R01 and currentAY-R12 are maintained