Feature: RecalculateEarningsFollowingApprovalOfStartDateChangeRequest

As a Provider
I want my earnings to reflect approved changes to the start date
So that I am paid the correct amount

** Note that the training code used in the below examples have Funding Band Max value set as below: **
** training_code 1 = 17,000 **
** training_code 2 = 18,000 **

Example 1: Start date moves earlier, stays in-year (normal recalc)
Example 2: Start date moves later, stays in-year (normal recalc)
Example 3: Start date move forwards, into next year 

@regression
Scenario: Start Date change approved; recalc earnings
	Given earnings have been calculated for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And a start date change request was sent with an approval date of <sdc_approved_date> with a new start date of <new_start_date> and end date of <end_date>
	And funding band max 18000 is determined for the training code
	When the start date change is approved
	Then the earnings are recalculated based on the new expected earnings <new_expected_earnings>
	And the history of old earnings is maintained with <old_inst_amount>
	And the ActualStartDate on the earnings entity is updated to <new_start_date>
	And old earnings maintain their initial Profile Id and new earnings have a new profile id

Examples:
	| start_date | end_date   | agreed_price | training_code | new_start_date | sdc_approved_date | old_inst_amount | new_expected_earnings |
	| 2023-08-23 | 2025-08-23 | 15000        | 2             | 2023-12-23     | 2024-06-10        | 500             | 600                   |
	| 2024-03-24 | 2026-03-24 | 18000        | 2             | 2023-09-24     | 2024-06-10        | 600             | 480                   |
	| 2023-08-26 | 2025-04-26 | 15000        | 2             | 2024-10-26     | 2024-10-10        | 600             | 2000                  |





