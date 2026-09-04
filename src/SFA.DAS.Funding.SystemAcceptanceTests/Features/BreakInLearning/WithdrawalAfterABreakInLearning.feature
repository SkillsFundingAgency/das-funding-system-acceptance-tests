Feature: WithdrawalReplacesABreakInLearning

Background:
	Given a learning has a start date of currentAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings after the delivery period 05 and academic year currentAY are soft deleted

#FLP-1429 AC1 training provider replaces a previously recorded Bil with Withdrawal
@regression
Scenario: Training provider withdraws a learner after recording a break in learning - withdrawal replaces break in learning
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And Learning withdrawal date is recorded on <withdrawal_date>
	And SLD submit updated learners details
	Then the earnings of <instal_amount> between <instal_start> and <instal_end> are maintained
	And the earnings after the delivery period <final_instal_period> and academic year <academic_year> are soft deleted

Examples:
	| withdrawal_date  | instal_amount | instal_start   | instal_end     | final_instal_period | academic_year |
	| currentAY-01-15 |           500 | currentAY-R01 | currentAY-R05 |                  05 | currentAY    |
	| currentAY-02-25 |           500 | currentAY-R01 | currentAY-R06 |                  06 | currentAY    |
	| nextAY-09-05  |           500 | currentAY-R01 | nextAY-R01  |                  01 | nextAY     |
