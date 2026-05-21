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
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with a new learning start date previousAY-03-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a break in learning with pause date previousAY-06-15
	And SLD submit updated learners details
	And earnings are recalculated
	And SLD inform us of a return from break in learning with a new learning start date currentAY-09-01
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of 558.82353 between previousAY-R08 and previousAY-R10 are maintained
	And the earnings of 711.22995 between currentAY-R02 and currentAY-R12 are maintained

#BiL, Return then Completion 
@regression
Scenario: Training provider records break in learning, return, then completion
	Given a learning has a start date of previousAY-08-20, a planned end date of currentAY-07-31 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-08-20 to date currentAY-07-31
	And SLD inform us of a break in learning with pause date previousAY-01-15
	And SLD inform us of a return from break in learning with both a new learning start date <return_start_date> and new expected end date <new_end_date>
	And Learning Completion is recorded on <completion_date>
	And SLD submit updated learners details
	And earnings are recalculated
	Then the earnings of 500 between previousAY-R01 and previousAY-R05 are maintained
	And the earnings of <new_instalment> between <new_instal_start> and <new_instal_end> are maintained
	And an earning of <balancing_amount> of type Balancing is generated for period <balancing_period>
	And an earning of <completion_amount> of type Completion is generated for period <completion_period>

Examples:
	| return_start_date | completion_date | new_end_date    | new_instalment | new_instal_start | new_instal_end | balancing_amount | balancing_period | completion_amount | completion_period |
	#168 and above Duration
	| currentAY-09-01   | currentAY-12-01 | currentAY-07-31 |      863.63686 | currentAY-R02    | currentAY-R04  |          6909.09 | currentAY-R05    |              3000 | currentAY-R05     |
	| currentAY-09-01   | currentAY-09-01 | currentAY-07-31 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-09-01   | currentAY-09-30 | currentAY-07-31 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-09-01   | currentAY-10-11 | currentAY-07-31 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R03    |              3000 | currentAY-R03     |
	| currentAY-09-01   | currentAY-10-12 | currentAY-07-31 |      863.63686 | currentAY-R02    | currentAY-R02  |       8636.36314 | currentAY-R03    |              3000 | currentAY-R03     |
	| currentAY-09-30   | currentAY-07-31 | currentAY-07-31 |      863.63686 | currentAY-R02    | currentAY-R11  |        863.63640 | currentAY-R12    |              3000 | currentAY-R12     |
	#14 to 167 Days Duration
	| currentAY-09-01   | currentAY-09-13 | currentAY-09-14 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-09-01   | currentAY-09-14 | currentAY-09-14 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-09-30   | currentAY-10-13 | currentAY-10-13 |           9500 | currentAY-R02    | currentAY-R02  |                0 | currentAY-R03    |              3000 | currentAY-R03     |
	| currentAY-09-30   | currentAY-10-13 | currentAY-03-15 |     1583.33333 | currentAY-R02    | currentAY-R02  |       7916.66667 | currentAY-R03    |              3000 | currentAY-R03     |
	| currentAY-09-30   | currentAY-11-25 | currentAY-03-15 |     1583.33333 | currentAY-R02    | currentAY-R03  |       6333.33334 | currentAY-R04    |              3000 | currentAY-R04     |
	#Less than 14 Days Duration 
	| currentAY-09-01   | currentAY-09-01 | currentAY-09-13 |              0 | currentAY-R02    | currentAY-R02  |             9500 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-09-30   | currentAY-10-12 | currentAY-10-12 |           9500 | currentAY-R02    | currentAY-R02  |                0 | currentAY-R03    |              3000 | currentAY-R03     |
	| currentAY-09-30   | currentAY-10-31 | currentAY-10-12 |           9500 | currentAY-R02    | currentAY-R02  |                0 | currentAY-R03    |              3000 | currentAY-R03     |


#End date pushed back to account for BIL with no price change
@regression
Scenario: Training provider pushes end date back to account for break in learning with no price change
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-12-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 1000 between previousAY-R10 and currentAY-R05 are maintained

#Apprenticeship duration is increased after BIL with no price change
@regression
Scenario: Training provider increases duration after break in learning with no price change
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-03-31
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 727.27 between previousAY-R10 and currentAY-R08 are maintained

#Apprenticeship duration is increased after BIL with price increase
@regression
Scenario: Training provider increases duration after break in learning with price increase
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date previousAY-05-01 and new expected end date currentAY-03-31
	And SLD record latest on-programme cost as total price 17000
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 872.72727 between previousAY-R10 and currentAY-R08 are maintained

#Withdrawal after BIL return
@regression
Scenario: Training provider withdraws apprenticeship after return from break in learning
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with both a new learning start date <return_start_date> and new expected end date <new_end_date>
	And Learning withdrawal date is recorded on <withdrawal_date>
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of <new_instal_amount> between <new_instal_start> and <new_instal_end> are created

Examples:
	| return_start_date | withdrawal_date  | new_end_date     | new_instal_amount | new_instal_start | new_instal_end |
	# 168 and above Duration
	| previousAY-05-01  | previousAY-05-01 | currentAY-11-30  |                 0 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-01  | previousAY-06-10 | currentAY-11-30  |                 0 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-01  | previousAY-06-11 | currentAY-11-30  |        1142.85714 | previousAY-R10   | previousAY-R10 |
	# Less than 14 Days Duration
	| previousAY-05-01  | previousAY-05-01 | previousAY-05-13 |                 0 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-31  | previousAY-05-31 | previousAY-06-12 |              8000 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-31  | previousAY-06-20 | previousAY-06-12 |              8000 | previousAY-R10   | previousAY-R10 |
	# 14-167 Days Duration
	| previousAY-05-01  | previousAY-05-01 | previousAY-05-14 |                 0 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-31  | previousAY-06-12 | previousAY-06-13 |                 0 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-31  | previousAY-06-13 | previousAY-06-13 |              8000 | previousAY-R10   | previousAY-R10 |
	| previousAY-05-31  | previousAY-06-13 | currentAY-11-13  |        1333.33333 | previousAY-R10   | previousAY-R10 |

#Withdrawal replaces BIL return - FLP-1429 AC2
@regression
Scenario: Training provider replaces return from break in learning with withdrawal
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD inform us of a return from break in learning with a new learning start date <return_start_date>
	And SLD submit updated learners details
	And SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And Learning withdrawal date is recorded on <withdrawal_date>
	And SLD submit updated learners details
	Then the earnings of <instal_amount> between <instal_start> and <instal_end> are maintained
	And the earnings after the delivery period <final_instal_period> and academic year <academic_year> are soft deleted
Examples:
	| return_start_date | withdrawal_date  | instal_amount | instal_start   | instal_end     | final_instal_period | academic_year |
	| previousAY-03-15  | previousAY-05-01 |          1000 | previousAY-R03 | previousAY-R09 |                  09 | previousAY    |
	| previousAY-03-15  | currentAY-08-31  |          1000 | previousAY-R03 | currentAY-R01  |                  01 | currentAY     |


#Withdrawal after 3 months of their return from BIL
@regression
Scenario: Training provider withdraws apprenticeship 3 months after return from break in learning
	Given a learning has a start date of previousAY-10-01, a planned end date of currentAY-09-30 and an agreed price of 15000
	When SLD record on-programme cost as total price 15000 from date previousAY-10-01 to date currentAY-09-30
	And SLD inform us of a break in learning with pause date previousAY-02-01
	And SLD submit updated learners details
	And SLD inform us of a return from break in learning with a new learning start date previousAY-05-01
	And Learning withdrawal date is recorded on currentAY-08-15
	And SLD submit updated learners details
	Then earnings are recalculated
	And the earnings of 1000 between previousAY-R03 and previousAY-R06 are maintained
	And the earnings of 1600 between previousAY-R10 and previousAY-R12 are maintained