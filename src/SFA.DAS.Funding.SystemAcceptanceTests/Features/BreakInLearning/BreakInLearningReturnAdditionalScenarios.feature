Feature: BreakInLearningReturnAdditionalScenarios

As the Dfe
I want the apprenticeship earnings to be recalculated when a return from break in learning is recorded in more complex scenarios
So that the provider acquires earnings once the learner has returned from a break

#NB This file has been added as it's own feature as the Background section which sets up a simple BIL in the main feature file is not relevant for these more complex scenarios.


#TODO when earnings event is fixed
#BIL followed by another BiL a few months later (BIL and return same time -> BiL again after 3 months -> return (in my head) )
@regression
Scenario: Training provider records multiple breaks in learning with returns
	Given a learning has a start date of currentAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date currentAY-03-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a break in learning with pause date currentAY-06-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a return from break in learning with a new learning start date nextAY-09-01
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of 558.82353 between currentAY-R08 and currentAY-R10 are maintained
	And the earnings of 711.22995 between nextAY-R02 and nextAY-R12 are maintained

#BiL, Return then Completion 
@regression
Scenario: Training provider records break in learning, return, then completion
	Given a learning has a start date of currentAY-08-20, a planned end date of nextAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-08-20 to date nextAY-07-31
	And SLD inform us of a break in learning with pause date currentAY-01-15
	And SLD inform us of a return from break in learning with both a new learning start date <return_start_date> and new expected end date <new_end_date>
	And Learning Completion is recorded on <completion_date>
	And Learning Achievement date is recorded on <completion_date>
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between currentAY-R01 and currentAY-R05 are maintained
	And the earnings of <new_instalment> between <new_instal_start> and <new_instal_end> are maintained
	And an earning of <balancing_amount> of type Balancing is generated for period <balancing_period>
	And an earning of <completion_amount> of type Completion is generated for period <completion_period>

Examples:
	| return_start_date | completion_date | new_end_date    | new_instalment | new_instal_start | new_instal_end | balancing_amount | balancing_period | completion_amount | completion_period |
	#168 and above Duration
	| nextAY-09-01   | nextAY-12-01 | nextAY-07-31 |      863.63686 | nextAY-R02    | nextAY-R04  |          6909.09 | nextAY-R05    |              3000 | nextAY-R05     |
	| nextAY-09-01   | nextAY-09-01 | nextAY-07-31 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R02    |              3000 | nextAY-R02     |
	| nextAY-09-01   | nextAY-09-30 | nextAY-07-31 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R02    |              3000 | nextAY-R02     |
	| nextAY-09-01   | nextAY-10-11 | nextAY-07-31 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R03    |              3000 | nextAY-R03     |
	| nextAY-09-01   | nextAY-10-12 | nextAY-07-31 |      863.63686 | nextAY-R02    | nextAY-R02  |       8636.36314 | nextAY-R03    |              3000 | nextAY-R03     |
	| nextAY-09-30   | nextAY-07-31 | nextAY-07-31 |      863.63686 | nextAY-R02    | nextAY-R11  |        863.63640 | nextAY-R12    |              3000 | nextAY-R12     |
	#14 to 167 Days Duration
	| nextAY-09-01   | nextAY-09-13 | nextAY-09-14 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R02    |              3000 | nextAY-R02     |
	| nextAY-09-01   | nextAY-09-14 | nextAY-09-14 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R02    |              3000 | nextAY-R02     |
	| nextAY-09-30   | nextAY-10-13 | nextAY-10-13 |           9500 | nextAY-R02    | nextAY-R02  |                0 | nextAY-R03    |              3000 | nextAY-R03     |
	| nextAY-09-30   | nextAY-10-13 | nextAY-03-01 |     1583.33333 | nextAY-R02    | nextAY-R02  |       7916.66667 | nextAY-R03    |              3000 | nextAY-R03     |
	| nextAY-09-30   | nextAY-11-25 | nextAY-03-01 |     1583.33333 | nextAY-R02    | nextAY-R03  |       6333.33334 | nextAY-R04    |              3000 | nextAY-R04     |
	#Less than 14 Days Duration 
	| nextAY-09-01   | nextAY-09-01 | nextAY-09-13 |              0 | nextAY-R02    | nextAY-R02  |             9500 | nextAY-R02    |              3000 | nextAY-R02     |
	| nextAY-09-30   | nextAY-10-12 | nextAY-10-12 |           9500 | nextAY-R02    | nextAY-R02  |                0 | nextAY-R03    |              3000 | nextAY-R03     |
	| nextAY-09-30   | nextAY-10-31 | nextAY-10-12 |           9500 | nextAY-R02    | nextAY-R02  |                0 | nextAY-R03    |              3000 | nextAY-R03     |


#End date pushed back to account for BIL with no price change
@regression
Scenario: Training provider pushes end date back to account for break in learning with no price change
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date currentAY-05-01 and new expected end date nextAY-12-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between currentAY-R03 and currentAY-R06 are maintained
	And the earnings of 1000 between currentAY-R10 and nextAY-R05 are maintained

#Apprenticeship duration is increased after BIL with no price change
@regression
Scenario: Training provider increases duration after break in learning with no price change
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date currentAY-05-01 and new expected end date nextAY-03-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between currentAY-R03 and currentAY-R06 are maintained
	And the earnings of 727.27 between currentAY-R10 and nextAY-R08 are maintained

#Apprenticeship duration is increased after BIL with price increase
@regression
Scenario: Training provider increases duration after break in learning with price increase
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date currentAY-05-01 and new expected end date nextAY-03-31
	And SLD record latest on-programme cost as total price 17000
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between currentAY-R03 and currentAY-R06 are maintained
	And the earnings of 872.72727 between currentAY-R10 and nextAY-R08 are maintained

#Withdrawal after BIL return
@regression
Scenario: Training provider withdraws apprenticeship after return from break in learning
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date <return_start_date> and new expected end date <new_end_date>
	And Learning withdrawal date is recorded on <withdrawal_date>
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between currentAY-R03 and currentAY-R06 are maintained
	And the earnings of <new_instal_amount> between <new_instal_start> and <new_instal_end> are created

Examples:
	| return_start_date | withdrawal_date  | new_end_date     | new_instal_amount | new_instal_start | new_instal_end |
	# 168 and above Duration
	| currentAY-05-01  | currentAY-05-01 | nextAY-11-30  |                 0 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-01  | currentAY-06-10 | nextAY-11-30  |                 0 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-01  | currentAY-06-11 | nextAY-11-30  |        1142.85714 | currentAY-R10   | currentAY-R10 |
	# Less than 14 Days Duration
	| currentAY-05-01  | currentAY-05-01 | currentAY-05-13 |                 0 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-31  | currentAY-05-31 | currentAY-06-12 |              8000 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-31  | currentAY-06-20 | currentAY-06-12 |              8000 | currentAY-R10   | currentAY-R10 |
	# 14-167 Days Duration
	| currentAY-05-01  | currentAY-05-01 | currentAY-05-14 |                 0 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-31  | currentAY-06-12 | currentAY-06-13 |                 0 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-31  | currentAY-06-13 | currentAY-06-13 |              8000 | currentAY-R10   | currentAY-R10 |
	| currentAY-05-31  | currentAY-06-13 | nextAY-11-13  |        1333.33333 | currentAY-R10   | currentAY-R10 |

#Withdrawal replaces BIL return - FLP-1429 AC2
@regression
Scenario: Training provider replaces return from break in learning with withdrawal
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD inform us of a return from break in learning with a new learning start date <return_start_date>
	And SLD submit updated learners details
	And SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And Learning withdrawal date is recorded on <withdrawal_date>
	And SLD submit updated learners details
	Then the earnings of <instal_amount> between <instal_start> and <instal_end> are maintained
	And the earnings after the delivery period <final_instal_period> and academic year <academic_year> are soft deleted
Examples:
	| return_start_date | withdrawal_date  | instal_amount | instal_start   | instal_end     | final_instal_period | academic_year |
	| currentAY-03-15  | currentAY-05-01 |          1000 | currentAY-R03 | currentAY-R09 |                  09 | currentAY    |
	| currentAY-03-15  | nextAY-08-31  |          1000 | currentAY-R03 | nextAY-R01  |                  01 | nextAY     |


#Withdrawal after 3 months of their return from BIL
@regression
Scenario: Training provider withdraws apprenticeship 3 months after return from break in learning
	Given a learning has a start date of currentAY-10-01, a planned end date of nextAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date currentAY-10-01 to date nextAY-09-30
	And SLD inform us of a break in learning with pause date currentAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with a new learning start date currentAY-05-01
	And Learning withdrawal date is recorded on nextAY-08-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between currentAY-R03 and currentAY-R06 are maintained
	And the earnings of 1600 between currentAY-R10 and currentAY-R12 are maintained