Feature: BreakInLearningIsRecordedNotIncludingReturn

As the Dfe
I want the apprenticeship earnings to be recalculated when a break in learning is recorded 
So that the provider does not accure earnings while the learner is on a break

@regression
Scenario: Training provider records a break in learning without specifying a return
	Given a learning has a start date of 2025-08-01, a planned end date of 2026-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date 2025-08-01 to date 2026-07-31
	And learning support is recorded from 2025-08-01 to 2026-07-31
	And SLD inform us of a break in learning with pause date 2026-06-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 10 and academic year 2526 are soft deleted
	And learning support continues to be paid from periods currentAY-R01 to currentAY-R10
