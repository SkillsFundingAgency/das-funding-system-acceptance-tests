Feature: RecalculateEarningsWhenAWithdrawalIsRecorded

As a provider
I want earnings to be recalculated when a learner is withdrawn
So that the earnings recorded for that learner are up to date.

Example 1: Simple in-year withdrawal - after 45 days - first months earnings are retained
Example 2: withdraw within qualifying period - on 42nd day - no earnings are retained
Example 3: app never started - withdraw from start - no earnings are retained
Example 4: after hard close - earnings up-to last complete delivery period before withdrawal are retained
Example 5:  after hard close - app never started - no earnings are retained

@regression
Scenario: Withdrawal is recorded; recalc earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <last_day_of_delivery>
	And SLD submit updated learners details
	Then the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <new_num_of_instalments>
	And the earnings after the delivery period <delivery_period_string> and academic year <academic_year_string> are soft deleted
	And a learning withdrawn event is published to approvals with reason <reason> and last day of learning as <last_day_of_delivery>


Examples:
	| start_date | end_date   | agreed_price | training_code | planned_number_of_months | last_day_of_delivery | new_num_of_instalments | delivery_period_string | academic_year_string | reason                 |
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 12                       | 2024-12-15           | 1                      | 4                      | 2425                 | WithdrawDuringLearning |
	| 2024-11-15 | 2025-11-20 | 24000        | 254           | 12                       | 2024-12-25           | 0                      | 4                      | 2425                 | WithdrawDuringLearning |
	| 2024-12-05 | 2025-12-20 | 15000        | 91            | 12                       | 2024-12-05           | 0                      | null                   | null                 | WithdrawFromStart      |
	| 2023-10-05 | 2025-06-10 | 18000        | 2             | 20                       | 2024-06-02           | 8                      | 10                     | 2324                 | WithdrawDuringLearning |
	| 2023-10-05 | 2025-06-10 | 18000        | 91            | 20                       | 2023-10-05           | 0                      | null                   | null                 | WithdrawFromStart      |

@regression
Scenario: Withdrawal is recorded again; with a different date
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <initial_last_day_of_delivery>
	And SLD submit updated learners details
	And the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <initial_num_of_instalments>
	And the earnings after the delivery period <initial_delivery_period_string> and academic year <academic_year_string> are soft deleted
	And SLD resubmits ILR
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <revised_last_day_of_delivery>
	And SLD submit updated learners details
	And the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <revised_num_of_instalments>
	And the earnings after the delivery period <revised_delivery_period_string> and academic year <academic_year_string> are soft deleted

Examples:
	| start_date | end_date   | agreed_price | training_code | initial_last_day_of_delivery | initial_num_of_instalments | initial_delivery_period_string | academic_year_string | revised_last_day_of_delivery | revised_num_of_instalments | revised_delivery_period_string |
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 2024-12-15                   | 1                          | 4                              | 2425                 | 2025-02-05                   | 3                          | 6                              |
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 2025-05-15                   | 6                          | 9                              | 2425                 | 2025-02-05                   | 3                          | 6                              |

@regression
Scenario: Withdrawal is removed; with date set to null
	Given an apprenticeship has a start date of 2024-08-01, a planned end date of 2025-07-31, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date 2024-08-01 to date 2025-07-31
	And Learning withdrawal date is recorded on 2024-12-15
	And SLD submit updated learners details
	And the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are 4
	And the earnings after the delivery period 4 and academic year 2425 are soft deleted
	And SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date 2024-08-01 to date 2025-07-31
	And SLD submit updated learners details
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are 12
	And a withdrawal reverted event is published to approvals


@regression
Scenario: Withdrawal date can be after planned end date
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <last_day_of_delivery>
	And SLD submit updated learners details
	Then the apprenticeship is marked as withdrawn
	And last day of learning is set to <last_day_of_delivery> in learning db
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <new_num_of_instalments>
Examples:
	| start_date       | end_date        | agreed_price | training_code | last_day_of_delivery | new_num_of_instalments |
	| previousAY-11-01 | currentAY-11-23 | 15000        | 2             | currentAY-12-15      | 12                     |


@regression
Scenario: Withdrawal is recorded before the end of the qualifying period; there will be no earnings retained
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <last_day_of_delivery>
	And SLD submit updated learners details
	Then the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are 0

Examples:
	| start_date | end_date   | agreed_price | training_code | planned_number_of_months | last_day_of_delivery |
	| 2020-08-15 | 2021-07-31 | 15000        | 2             | 12                       | 2020-09-15           |
	| 2020-01-31 | 2020-02-13 | 15000        | 254           | 1                        | 2020-02-12           |