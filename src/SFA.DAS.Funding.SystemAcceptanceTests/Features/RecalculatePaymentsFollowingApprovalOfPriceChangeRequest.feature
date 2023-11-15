

Feature: RecalculatePaymentsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my payments to reflect approved changes to the original price
So that I am paid the correct amount

Example 1: Price Rise in year 1 - New Price at Funding Band Max
Example 2: Price Fall in year 1 - New Price below Funding Band Max
Example 3: Price Rise in year 1 - New Price above Funding Band Max
Example 4: Price Rise in year 2 - New price at Funding Band Max

@regression
Scenario: Price change approved; recalc payments
	Given payments have been paid for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	And funding band max <funding_band_max> is determined for the training code
	When the price change is approved
	Then for all the past census periods, where the payment has already been made, the amount is still same as previous earnings <previous_earnings> and are flagged as sent for payment
	And for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new and old earnings
	And for all payments for future collection periods are equal to the new earnings

Examples:
	| start_date | end_date   | agreed_price | training_code | pc_from_date | new_total_price | pc_approved_date | funding_band_max | previous_earnings |
	| 2023-08-23 | 2025-08-23 | 15000        | 2             | 2023-08-29   | 18000           | 2024-06-10       | 18000            | 500               |
	| 2023-08-24 | 2025-08-24 | 18000        | 2             | 2023-08-29   | 9000            | 2024-06-10       | 18000            | 600               |
	| 2023-08-25 | 2025-04-25 | 15000        | 1             | 2023-08-23   | 18000           | 2024-06-10       | 17000            | 600               |
	| 2022-08-26 | 2024-04-26 | 15000        | 2             | 2023-08-23   | 18000           | 2023-10-10       | 18000            | 600               |

@ignore
Scenario: Publish Price Change Approved Event
	Given a price change event is approved
		| apprenticeship_key                   | apprenticeship_id | training_price | assessment_price | effective_from_date | approved_date | employer_account_id | provider_id |
		| 233d516c-9170-4697-986f-27a3f3c0b939 | 184               | 14000          | 2000             | 2023-10-23          | 2024-02-25    | 3871                | 92          |

