Feature: RecalculatePaymentsFollowingApprovalOfStartDateChangeRequest

As a Provider
I want my payments to reflect approved changes to the original start date
So that I am paid the correct amount

Example 1: Start date moved forwards within year 1
Example 2: Start date moved backwards within year 1
Example 3: Start date moved forwards into year 2

@regression
Scenario: Start date change approved; recalc payments
	Given payments have been paid for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And a start date change request was sent with an approval date of <sdc_approved_date> with a new start date of <new_start_date>
	And funding band max 18000 is determined for the training code
	When the start date change is approved
	Then for all the past census periods since <start_date>, where the payment has already been made, the amount is still same as previous earnings <previous_earnings> and are flagged as sent for payment
	And for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new earnings <new_expected_earnings> and old earnings
	And for all payments for future collection periods are equal to the new earnings


Examples:
	| start_date | end_date   | agreed_price | training_code | new_start_date | sdc_approved_date | previous_earnings | new_expected_earnings |
	| 2023-08-23 | 2025-08-23 | 15000        | 2             | 2023-12-23     | 2024-06-10        | 500               | 600                   |
	| 2024-03-24 | 2026-03-24 | 18000        | 2             | 2023-09-24     | 2024-06-10        | 600               | 400                   |
	| 2023-08-26 | 2025-04-26 | 15000        | 2             | 2024-10-26     | 2024-10-10        | 600               | 2000                  |