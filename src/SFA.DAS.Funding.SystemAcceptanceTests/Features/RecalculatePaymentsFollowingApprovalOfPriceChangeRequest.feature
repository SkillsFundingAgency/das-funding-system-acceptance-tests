

Feature: RecalculatePaymentsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my payments to reflect approved changes to the original price
So that I am paid the correct amount

Example 1: Price Rise in year 1 - New Price at Funding Band Max
Example 2: Price Fall in year 1 - New Price below Funding Band Max
Example 3: Price Rise in year 1 - New Price above Funding Band Max
Example 4: Price Rise in year 2 - New price at Funding Band Max

@regression @releasesPayments
Scenario: Price change approved; recalc payments
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And Payments Generated Events are published
	And payments have been paid for an apprenticeship with <start_date>, <end_date>
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	And funding band max <funding_band_max> is determined for the training code
	When the price change is approved
	And Payments Generated Events are published
	Then for all the past census periods since <start_date>, where the payment has already been made, the amount is still same as previous earnings <previous_earnings> and are flagged as sent for payment
	And for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new and old earnings
	And for all payments for future collection periods are equal to the new earnings


Examples:
	| start_date      | end_date               | agreed_price | training_code | pc_from_date    | new_total_price | pc_approved_date | funding_band_max | previous_earnings |
	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | currentAY-08-25 | 18000           | currentAY-06-10  | 18000            | 500               |
	| currentAY-08-24 | currentAYPlusTwo-08-24 | 18000        | 2             | currentAY-08-26 | 9000            | currentAY-06-10  | 18000            | 600               |
	| currentAY-08-25 | nextAY-04-25           | 15000        | 1             | currentAY-08-27 | 18000           | currentAY-06-10  | 17000            | 600               |
	| currentAY-08-26 | nextAY-04-26           | 15000        | 2             | currentAY-08-28 | 18000           | nextAY-10-10     | 18000            | 600               |

@ignore
Scenario: Publish Price Change Approved Event
	Given a price change event is approved
		| apprenticeship_key                   | apprenticeship_id | training_price | assessment_price | effective_from_date | approved_date | employer_account_id | provider_id |
		| d11b7c14-c94f-4665-b9ba-a7c5c3b89d9d | 169               | 14000          | 2000             | 2023-10-23          | 2024-02-25    | 3871                | 197         |

