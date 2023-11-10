Feature: RecalculatePaymentsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my payments to reflect approved changes to the original price
So that I am paid the correct amount

** Note that the training code used in the below examples have Funding Band Max value set as below: **
** training_code 1 = 17,000 **
** training_code 2 = 18,000 **

Example 1: Price Rise in year 1 - New Price at Funding Band Max

@regression
Scenario: Price change approved in the year it was requested; recalc payments
	Given payments have been paid for an apprenticeship with <start_date>, <end_date>, <agreed_price>, and <training_code>
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	When the price change is approved
	Then for all the past census periods, where the payment has already been made, the amount is still same as previous earnings <previous_earnings> and are flagged as sent for payment
	And for all the past census periods, new payments entries are created and marked as Not sent for payment in the durable entity with the difference <difference> between new and old earnings
	And for all payments for future collection periods are equal to the new earnings <new_earnings>

Examples:
	| start_date | end_date   | agreed_price | training_code | pc_from_date | new_total_price | pc_approved_date | new_earnings | previous_earnings | difference |
	| 2023-08-23 | 2024-08-23 | 15000        | 2             | 2023-08-29   | 18000           | 2024-06-10       | 1200         | 1000              | 200        |

