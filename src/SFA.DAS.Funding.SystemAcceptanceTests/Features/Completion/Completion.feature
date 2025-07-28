Feature: Completion

When the SLD inform us of a Learning's Completion
Then we should roll-up future earnings into a single balancing payment
And record the completion payment as earned

@regression
Scenario: Balancing and Completion earnings on Completion
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of <agreed_price>
	When SLD inform us that the Learning Completed on <completion_date>
	Then earnings of <expected_earning_amount> are generated from periods <expected_first_earning_period> to <expected_last_earning_period>
	Then an earning of <expected_balancing_amount> of type Balancing is generated for period <expected_balancing_period>
	And an earning of <expected_completion_amount> of type Completion is generated for period <expected_completion_period>

Examples:
	| start_date      | end_date        | agreed_price | completion_date | expected_first_earning_period | expected_last_earning_period | expected_earning_amount | expected_balancing_period | expected_balancing_amount | expected_completion_period | expected_completion_amount |
	| currentAY-08-01 | currentAY-07-31 | 15000        | currentAY-06-15 | currentAY-R01                 | currentAY-R10                | 1000                    | currentAY-R11             | 2000                      | currentAY-R11              | 3000                       |

