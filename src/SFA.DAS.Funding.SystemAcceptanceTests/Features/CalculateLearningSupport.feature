Feature: Calculate learning support earnings and payments

As a Training provider
I want learning support earnings & payments to be calculated
So I get paid learning support correctly

@regression
Scenario: Learning Support Earnings & Payments
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And learning support is recorded from <learning_support_start> to <learning_support_end>
	Then learning support earnings are generated from periods <expected_first_payment_period> to <expected_last_payment_period>
	And learning support payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period>

Examples:
	| learning_support_start | learning_support_end | expected_first_payment_period | expected_last_payment_period |
	| currentAY-08-01        | currentAY-12-15      | currentAY-R01                 | currentAY-R04                |
	| currentAY-09-01        | currentAY-12-15      | currentAY-R02                 | currentAY-R04                |
	| nextAY-09-01           | nextAY-05-15         | nextAY-R02                    | nextAY-R09                   |