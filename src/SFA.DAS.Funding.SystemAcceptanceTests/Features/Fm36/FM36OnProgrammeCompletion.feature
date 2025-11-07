@nonparallelizable
Feature: FM36OnProgrammeCompletion

Fm36 Withdrawl tests

@regression
Scenario: On programme completion
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of <agreed_price>
	When Learning Completion is recorded on <completion_date>
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And SLD submit updated learners details
	And the fm36 data is retrieved for previousAY-07-25
	Then PriceEpisodeActualEndDateIncEPA is <completion_date>
	And PriceEpisodeBalancePayment for period <expected_balancing_period> is amount <expected_balancing_amount>
	And PriceEpisodeCompletionPayment for period <expected_completion_period> is amount <expected_completion_amount>

Examples:
	| start_date       | end_date         | agreed_price | completion_date  | expected_balancing_period | expected_balancing_amount | expected_completion_period | expected_completion_amount |
	| previousAY-08-01 | previousAY-07-31 | 15000        | previousAY-06-15 | previousAY-R11            | 2000                      | previousAY-R11             | 3000                       |

