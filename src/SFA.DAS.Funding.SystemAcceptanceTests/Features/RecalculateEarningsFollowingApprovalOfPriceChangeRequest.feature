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
	And the total price is above or below or at the funding band maximum
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	When the price change is approved
	## validate that AgreePrice within earnings entity is updated 
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year>
	And earnings prior to <delivery_period> and <academic_year> are frozen with <old_inst_amount>
	And the history of old earnings is maintained with <old_inst_amount>
	## old earnings maintain thair inital Profile Id
	## new earnings have a new Profile Id
Examples:
	| start_date | end_date   | agreed_price | training_code | pc_from_date | new_total_price | pc_approved_date | new_inst_amount | academic_year | old_inst_amount | delivery_period |
	| 2023-08-23 | 2025-04-23 | 15000        | 2             | 2023-08-29   | 18000           | 2024-06-10       | 720             | 2324          | 600             | 1               |
	| 2023-08-24 | 2025-04-24 | 15000        | 1             | 2023-09-23   | 18000           | 2024-06-10       | 684.21          | 2324          | 600             | 2               |
	| 2023-08-25 | 2025-04-25 | 15000        | 2             | 2024-12-01   | 18000           | 2025-03-10       | 1200            | 2425          | 600             | 5               |
	| 2023-08-26 | 2025-04-26 | 15000        | 1             | 2024-08-01   | 8000            | 2025-03-10       | -100            | 2425          | 600             | 1               |
	| 2023-08-27 | 2025-04-27 | 15000        | 2             | 2023-08-25   | 18000           | 2024-08-10       | 720             | 2324          | 600             | 1               |




