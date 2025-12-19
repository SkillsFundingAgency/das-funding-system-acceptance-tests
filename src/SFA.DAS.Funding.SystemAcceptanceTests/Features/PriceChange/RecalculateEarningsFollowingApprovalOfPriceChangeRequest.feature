Feature: RecalculateEarningsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my earnings to reflect approved changes to the total price
So that I am paid the correct amount

** Note that the training code used in the below examples have Funding Band Max value set as below: **
** training_code 1 = 17,000 **
** training_code 2 = 18,000 **

Example 1: Price Rise in year 1 - New Price at Funding Band Max
Example 2: Price Rise in year 1 - New Price above Funding Band Max
Example 3: Price Rise in year 2 - New Price at Funding Band Max
Example 4: Price Drop in year 2 - New Price below Funding Band Max
Example 5: Price Rise in R13 of year 1 - New Price at Funding Band Max

@regression
Scenario: Price change - Total price change ONLY - recalc earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And SLD record on-programme cost as total price <new_total_price> from date <pc_from_date> to date <end_date>
	And SLD submit updated learners details
	Then the history of old learning is maintained 
	And the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year_string>
	And earnings prior to <delivery_period> and <academic_year_string> are frozen with <old_inst_amount>
	And the history of old earnings is maintained with <old_inst_amount>
	And the AgreedPrice on the earnings entity is updated to <new_total_price>
	And old and new earnings maintain their initial Profile Id
Examples:
	| start_date      | end_date     | agreed_price | training_code | pc_from_date    | new_total_price | new_inst_amount | academic_year_string | old_inst_amount | delivery_period |
	| currentAY-08-23 | nextAY-04-23 |        15000 |             2 | currentAY-08-29 |           18000 |             720 | currentAY            |             600 |               1 |
	| currentAY-08-24 | nextAY-04-24 |        15000 |             1 | currentAY-09-23 |           18000 |          684.21 | currentAY            |             600 |               2 |
	| currentAY-08-25 | nextAY-04-25 |        15000 |             2 | nextAY-12-01    |           18000 |            1200 | nextAY               |             600 |               5 |
	| currentAY-08-26 | nextAY-04-26 |        15000 |             1 | nextAY-08-01    |            8000 |            -100 | nextAY               |             600 |               1 |
	| currentAY-08-27 | nextAY-04-27 |        15000 |             2 | currentAY-08-29 |           18000 |             720 | currentAY            |             600 |               1 |

@regression
Scenario: Price change; Costs array combinations
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	When SLD record on-programme training price <training_price> with epao as <epao> from date <pc_from_date> to date <end_date>
	And SLD record on-prog start date as <start_date>
	And SLD submit updated learners details
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year_string>
	And the AgreedPrice on the earnings entity is updated to <new_total_price>
Examples:
	| start_date       | end_date         | agreed_price | training_code | pc_from_date     | new_total_price | training_price | epao | new_inst_amount | academic_year_string | delivery_period |
	| previousAY-07-23 | nextAY-03-23     |        15000 |             2 | previousAY-07-23 |           18000 |          18000 | null |             750 | previousAY           |              12 |
	| previousAY-07-23 | nextAY-03-23     |        15000 |             2 | null             |               0 | null           | null |             750 | previousAY           |              12 |
	| previousAY-08-01 | previousAY-07-31 |        15000 |             2 | null             |           18000 |          15000 | 3000 |            1200 | previousAY           |               1 |

@regression
Scenario: Price change; new total price is the same but training and epao costs changed
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	When SLD record on-programme training price <training_price> with epao as <epao> from date <pc_from_date> to date <end_date>
	And SLD record on-prog start date as <start_date>
	And SLD submit updated learners details
	Then the AgreedPrice on the earnings entity is updated to <new_total_price>
Examples:
	| start_date       | end_date         | agreed_price | training_code | pc_from_date     | new_total_price | training_price | epao | new_inst_amount | academic_year_string | delivery_period |
	| previousAY-08-01 | previousAY-07-31 |        15000 |             2 | null             |           15000 |          15000 | null |            1000 | previousAY           |               1 |



@regression
Scenario: Price change; Empty Costs array
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD record on-prog start date as <start_date>
	And SLD record expected end date <end_date>
	And SLD record standard code as <training_code>
	And SLD record an empty on-programme costs array
	And SLD submit updated learners details
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year_string>
	And the AgreedPrice on the earnings entity is updated to <new_total_price>
Examples:
	| start_date       | end_date     | agreed_price | training_code | new_total_price | new_inst_amount | academic_year_string | delivery_period |
	| previousAY-07-23 | nextAY-03-23 |        15000 |             2 |               0 |               0 | currentAY            |               2 |

@regression
Scenario: Price change; Both total price and start date changed
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	When SLD record on-programme cost as total price <agreed_price> from date <new_start_date> to date <end_date>
	And SLD record on-programme cost as total price <new_total_price> from date <pc_from_date> to date <end_date>
	And SLD submit updated learners details
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year_string>
	And earnings prior to <delivery_period> and <academic_year_string> are frozen with <old_inst_amount>
	And the AgreedPrice on the earnings entity is updated to <new_total_price>
Examples:
	| start_date       | end_date     | agreed_price | training_code | new_start_date  | pc_from_date    | new_total_price | new_inst_amount | academic_year_string | old_inst_amount | delivery_period |
	| previousAY-07-23 | nextAY-03-23 |        15000 |             2 | currentAY-08-15 | currentAY-09-29 |           18000 |             765 | currentAY            |       631.57895 |               2 |


