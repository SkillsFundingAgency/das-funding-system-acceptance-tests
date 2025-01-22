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
Scenario: Price change approved; recalc earnings
	Given earnings have been calculated for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	When the price change is approved
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year_string>
	And earnings prior to <delivery_period> and <academic_year_string> are frozen with <old_inst_amount>
	And the history of old earnings is maintained with <old_inst_amount>
	And the AgreedPrice on the earnings entity is updated to <new_total_price>
	And old earnings maintain their initial Profile Id and new earnings have a new profile id
Examples:
	| start_date      | end_date     | agreed_price | training_code | pc_from_date    | new_total_price | pc_approved_date | new_inst_amount | academic_year_string | old_inst_amount | delivery_period |
	| currentAY-08-23 | nextAY-04-23 | 15000        | 2             | currentAY-08-29 | 18000           | currentAY-06-10  | 720             | currentAY            | 600             | 1               |
	| currentAY-08-24 | nextAY-04-24 | 15000        | 1             | currentAY-09-23 | 18000           | currentAY-06-10  | 684.21          | currentAY            | 600             | 2               |
	| currentAY-08-25 | nextAY-04-25 | 15000        | 2             | nextAY-12-01    | 18000           | nextAY-03-10     | 1200            | nextAY               | 600             | 5               |
	| currentAY-08-26 | nextAY-04-26 | 15000        | 1             | nextAY-08-01    | 8000            | nextAY-03-10     | -100            | nextAY               | 600             | 1               |
	| currentAY-08-27 | nextAY-04-27 | 15000        | 2             | currentAY-08-29 | 18000           | nextAY-08-10     | 720             | currentAY            | 600             | 1               |




