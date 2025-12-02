Feature: BreakInLearningReturn

As the Dfe
I want the apprenticeship earnings to be recalculated when a return from break in learning is recorded
So that the provider acquires earnings once the learner has returned from a break

Background: 
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD submit updated learners details
	Then earnings are recalculated
	#pay for December R05 in previous AY not beyond - todo AY tokenisation
	And the earnings after the delivery period 05 and academic year 2425 are soft deleted
	And learning support continues to be paid from periods previousAY-R01 to previousAY-R05

@regression
#FLP-1360 AC1 previousAY return
Scenario: Training provider records a return from a break in learning
	Given SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	#todo write this step def
	And SLD inform us of a return from break in learning with a new learning start date previousAY-03-01
	And SLD submit updated learners details
	Then earnings are recalculated
	#todo these 2 steps need to assert the pre-break and post-break periods
	And the earnings after the delivery period 05 and academic year 2425 are soft deleted
	And learning support continues to be paid from periods previousAY-R01 to previousAY-R05

#@regression
#FLP-1360 AC1 current AY return

#@regression
#FLP-1360 AC2 SLD informs us of break and return at the same time previous AY TODO NEW FEATURE - background break won't be needed, just background happy path standard learning, as we need to do both in same submission
#@regression
#FLP-1360 AC2 SLD informs us of break and return at the same time current AY TODO NEW FEATURE - background break won't be needed, just background happy path standard learning, as we need to do both in same submission

#@regression
#FLP-1360 AC3 training provider corrects previously recorded return previousAY
#@regression
#FLP-1360 AC3 training provider corrects previously recorded return currentAY

#@regression
#FLP-1360 AC4 training provider removes previously recorded return previousAY
#@regression
#FLP-1360 AC4 training provider removes previously recorded return currentAY
#@regression
#FLP-1360 AC4 training provider removes previously recorded return & entire break previousAY
#@regression
#FLP-1360 AC4 training provider removes previously recorded return & entire break currentAY