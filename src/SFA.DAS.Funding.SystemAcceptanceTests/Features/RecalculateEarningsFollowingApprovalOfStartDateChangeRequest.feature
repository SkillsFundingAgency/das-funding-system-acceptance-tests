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
	And a start date change request was sent with an approval date of <sdc_approved_date> with a new start date of <new_start_date> and end date of <new_end_date>
	And funding band max 18000 is determined for the training code
	When the start date change is approved
	Then the earnings are recalculated based on the new expected earnings <new_expected_earnings>
	And the history of old earnings is maintained with <old_inst_amount>
	And the ActualStartDate <new_start_date> and PlannedEndDate <new_end_date> are updated on earnings entity
	And old earnings maintain their initial Profile Id and new earnings have a new profile id

Examples:
	| start_date      | end_date               | agreed_price | training_code | new_start_date  | new_end_date           | sdc_approved_date | old_inst_amount | new_expected_earnings |
	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | currentAY-12-23 | currentAYPlusTwo-08-23 | currentAY-06-10   | 500             | 600                   |
	| currentAY-03-24 | currentAYPlusTwo-03-24 | 18000        | 2             | currentAY-09-24 | currentAYPlusTwo-03-24 | currentAY-06-10   | 600             | 480                   |
	| currentAY-08-26 | nextAy-04-26           | 15000        | 2             | nextAY-10-26    | nextAy-04-26           | currentAY-10-10   | 600             | 2000                  |
	| currentAY-09-15 | currentAYPlusTwo-09-15 | 15000        | 2             | currentAY-01-20 | currentAYPlusTwo-01-20 | currentAY-06-10   | 500             | 500                   |





