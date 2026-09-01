Feature: BreakInLearningIsRecordedNotIncludingReturn

As the Dfe
I want the apprenticeship earnings to be recalculated when a break in learning is recorded 
So that the provider does not accure earnings while the learner is on a break

@regression
#FLP-728 AC1
Scenario: Training provider records a break in learning without specifying a return
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And learning support is recorded from nextAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date nextAY-05-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 09 and academic year nextAY are soft deleted
	And learning support continues to be paid from periods nextAY-R01 to nextAY-R09

@regression
#FLP-728 AC2
Scenario: Training provider corrects a previous break in learning without specifying a return
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And learning support is recorded from nextAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date nextAY-05-15
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And learning support is recorded from nextAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date nextAY-06-20
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 10 and academic year nextAY are soft deleted
	And learning support continues to be paid from periods nextAY-R01 to nextAY-R10

@regression
#FLP-728 AC3
Scenario: Training provider removes a previous break in learning
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And learning support is recorded from nextAY-08-01 to nextAY-07-31
	And SLD inform us of a break in learning with pause date nextAY-05-15
	And SLD submit updated learners details
	And earnings are recalculated
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And learning support is recorded from nextAY-08-01 to nextAY-07-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And earnings of 1000 are generated from periods nextAY-R01 to nextAY-R12
	And learning support earnings are generated from periods nextAY-R01 to nextAY-R12

@regression
#FLP-728 AC1 - Added this scenario to cover BIL without learning support as there is an issue whereby the earnings recalculated event is nont published for pause, pause remove, and BIL return, to be fixed as part of FLP-1360.
Scenario: Training provider records a break in learning without specifying a return (no learning support)
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD inform us of a break in learning with pause date nextAY-05-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 09 and academic year nextAY are soft deleted