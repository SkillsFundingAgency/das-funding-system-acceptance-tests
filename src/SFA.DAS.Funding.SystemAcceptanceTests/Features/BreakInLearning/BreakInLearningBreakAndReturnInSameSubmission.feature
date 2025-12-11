Feature: BreakInLearningBreakAndReturnInSameSubmission

As the Dfe
I want the apprenticeship earnings to be recalculated when a break in learning and a return from that break in learning are recorded together
So that the provider acquires earnings only when the learner is not on a break

#todo remove learning support from these tests to prove the earnings fix to re-calc event as part of BIL

#FLP-1360 AC2 SLD informs us of break and return at the same time previous AY
@regression
Scenario: Training provider records a break and return at the same time in previous academic year
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date previousAY-03-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings between previousAY-R01 and previousAY-R05 are maintained
	And the earnings between previousAY-R06 and previousAY-R07 are soft deleted
	And the earnings between previousAY-R08 and currentAY-R12 are maintained

#@regression
#FLP-1360 AC2 SLD informs us of break and return at the same time current AY
@regression
Scenario: Training provider records a break and return at the same time in current academic year
	Given a learning has a start date of previousAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-01 to date currentAY-07-31
	And learning support is recorded from previousAY-08-01 to currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-03-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings between previousAY-R01 and previousAY-R05 are maintained
	And the earnings between previousAY-R06 and currentAY-R07 are soft deleted
	And the earnings between currentAY-R08 and currentAY-R12 are maintained