Feature: BreakInLearningBreakAndReturnInSameSubmission

As the Dfe
I want the apprenticeship earnings to be recalculated when a break in learning and a return from that break in learning are recorded together
So that the provider acquires earnings only when the learner is not on a break

#todo remove learning support from these tests to prove the earnings fix to re-calc event as part of BIL

#FLP-1360 AC2 SLD informs us of break and return at the same time previous AY
@regression
Scenario: Training provider records a break and return at the same time in previous academic year
	Given a learning has a start date of currentAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And learning support is recorded from currentAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-03-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 558.82352 between currentAY-R08 and nextAY-R12 are maintained

#@regression
#FLP-1360 AC2 SLD informs us of break and return at the same time current AY
@regression
Scenario: Training provider records a break and return at the same time in current academic year
	Given a learning has a start date of currentAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And learning support is recorded from currentAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date nextAY-03-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 1900 between nextAY-R08 and nextAY-R12 are maintained