Feature: RecalculateEarningsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my earnings to reflect approved changes to the total price
So that I am paid the correct amount

@regression
Scenario: Price change approved in the year it was requested, below or at funding band max; recalc earnings
	Given earnings have been calculated for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And the total price is below or at the funding band maximum
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	When the price change is approved
	Then the earnings are recalculated based on the new instalment amount of <new_inst_amount> from <delivery_period> and <academic_year>
	And earnings prior to <delivery_period> and <academic_year> are frozen with <old_inst_amount>
	And the history of old and new earnings is maintained with <old_inst_amount> from instalment period <delivery_period>
Examples:
	| start_date | end_date   | agreed_price | training_code | pc_from_date | new_total_price | pc_approved_date | new_inst_amount | academic_year | old_inst_amount | delivery_period |
	| 2022-08-15 | 2023-08-15 | 22500        | 614           | 2022-08-15   | 27000           | 2023-10-15       | 1800            | 2223          | 1500            | 1               |
	| 2022-08-15 | 2024-08-15 | 22500        | 614           | 2023-02-01   | 27000           | 2023-10-15       | 950             | 2223          | 750             | 7               |




