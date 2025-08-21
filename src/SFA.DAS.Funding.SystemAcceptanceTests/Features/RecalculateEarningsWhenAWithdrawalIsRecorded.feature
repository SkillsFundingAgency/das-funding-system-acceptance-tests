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
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	Then the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <new_num_of_instalments>
	And the earnings after the delivery period <delivery_period_string> and academic year <academic_year_string> are soft deleted

Examples:
	| start_date | end_date   | agreed_price | training_code | planned_number_of_months | reason                 | last_day_of_delivery | new_num_of_instalments | delivery_period_string | academic_year_string |
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 12                       | WithdrawDuringLearning | 2024-12-15           | 1                      | 4                      | 2425                 |
	| 2024-11-15 | 2025-11-20 | 24000        | 254           | 12                       | WithdrawDuringLearning | 2024-12-25           | 0                      | 4                      | 2425                 |
	| 2024-12-05 | 2025-12-20 | 15000        | 91            | 12                       | WithdrawFromStart      | 2024-12-05           | 0                      | null                   | null                 |
	| 2023-10-05 | 2025-06-10 | 18000        | 2             | 20                       | WithdrawDuringLearning | 2024-06-02           | 8                      | 10                     | 2324                 |
	| 2023-10-05 | 2025-06-10 | 18000        | 91            | 20                       | WithdrawFromStart      | 2023-10-05           | 0                      | null                   | null                 |

@regression
Scenario: Withdrawal is recorded before the end of the qualifying period; there will be no earnings retained
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	Then the apprenticeship is marked as withdrawn
	And earnings are recalculated
	And the expected number of earnings instalments after withdrawal are 0

Examples:
	| start_date | end_date   | agreed_price | training_code | planned_number_of_months | reason                 | last_day_of_delivery |
	| 2020-08-15 | 2021-07-31 | 15000        | 2             | 12                       | WithdrawDuringLearning | 2020-09-15           |
	| 2020-01-31 | 2020-02-13 | 15000        | 254           | 1                        | WithdrawDuringLearning | 2020-02-12           |