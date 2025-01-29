Feature: RecalculateEarningsWhenAWithdrawalIsRecorded

As a provider
I want earnings to be recalculated when a learner is withdrawn
So that the earnings recorded for that learner are up to date.

@regression
Scenario: Withdrawal is recorded; recalc earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	Then earnings are recalculated
	And the expected number of earnings instalments after withdrawal are <new_num_of_instalments>
	And the earnings after the delivery period <delivery_period> and academic year <academic_year> are soft deleted


Examples:
	| start_date | end_date   | agreed_price | training_code | planned_number_of_months | reason                 | last_day_of_delivery | new_num_of_instalments | delivery_period | academic_year |
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 12                       | WithdrawDuringLearning | 2024-12-15           | 1                      | 4               | 2425          |

