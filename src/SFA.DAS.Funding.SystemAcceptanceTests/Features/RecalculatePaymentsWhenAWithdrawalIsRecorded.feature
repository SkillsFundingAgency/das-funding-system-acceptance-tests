Feature: RecalculatePaymentsWhenAWithdrawalIsRecorded

As a provider
I want earnings to be recalculated when a learner is withdrawn
So that the earnings recorded for that learner are up to date.

Example 1: Simple in-year withdrawal - after 45 days - first months payments are retained thereafer clawed back
Example 2: withdraw within qualifying period - on 42nd day - no payments are retained - all clawed back (currently 1 earnings is retained. this will be resolved by FLP-818)
Example 3: app never started - withdraw from start - no payments are retained - all clawed back
Example 4: after hard close - all payments made this year are clawed back
Example 5:  after hard close - app never started - all payments made this year are clawed back

@regression
Scenario: Withdrawal is recorded; recalc payments
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And payments have been paid for an apprenticeship with <start_date>, <end_date>
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	Then the apprenticeship is marked as withdrawn
	And payments are recalculated
	And for all the past census periods since <start_date>, where the payment has already been made, the amount is still same as previous earnings <previous_earnings_amount> and are flagged as sent for payment
	And new payments with amount <previous_earnings_amount> are marked as Not sent for payment and clawed back
	And all the payments from delivery periods after last payment made are deleted

Examples:
	| start_date       | end_date     | agreed_price | training_code | reason                 | last_day_of_delivery | previous_earnings_amount |
	| currentAY-11-01  | nextAY-11-23 | 15000        | 2             | WithdrawDuringLearning | currentAY-12-15      | 1000                     |
	| currentAY-11-15  | nextAY-11-20 | 24000        | 254           | WithdrawDuringLearning | currentAY-12-26      | 1600                     |
	| currentAY-12-05  | nextAY-12-20 | 15000        | 91            | WithdrawFromStart      | currentAY-12-05      | 1000                     |
	| previousAY-10-05 | nextAY-06-10 | 18000        | 2             | WithdrawDuringLearning | previousAY-06-02     | 450                      |
	| previousAY-10-05 | nextAY-06-10 | 18000        | 91            | WithdrawFromStart      | previousAY-10-05     | 450                      |

