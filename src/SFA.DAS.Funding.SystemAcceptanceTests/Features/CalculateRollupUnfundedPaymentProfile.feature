Feature: CalculateRollupUnfundedPaymentProfile

As the Apprenticeship Service SRO
I want to ensure On-Programme Payments for providers can be calculated using the learner data collected on the Apprenticeship Service 
when there are earnings in the past that have not yet been paid 
So that I can ensure we can bring the provider’s payments up to date (i.e. make ‘rollup’ payments)

@regression
Scenario: Rollup Payment profile for single learner - start declared retrospectively
	Given an apprenticeship with start date over 2 months ago and duration of 12 months and an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then Unfunded Payments for the appreticeship including rollup payments are calculated as below
		| DeliveryPeriod | PaymentPeriod   | Amount |
		| CurrentMonth-2 | CurrentMonth+1  | 1000   |
		| CurrentMonth1  | CurrentMonth+1  | 1000   |
		| CurrentMonth+0 | CurrentMonth+1  | 1000   |
		| CurrentMonth+1 | CurrentMonth+2  | 1000   |
		| CurrentMonth+2 | CurrentMonth+3  | 1000   |
		| CurrentMonth+3 | CurrentMonth+4  | 1000   |
		| CurrentMonth+4 | CurrentMonth+5  | 1000   |
		| CurrentMonth+5 | CurrentMonth+6  | 1000   |
		| CurrentMonth+6 | CurrentMonth+7  | 1000   |
		| CurrentMonth+7 | CurrentMonth+8  | 1000   |
		| CurrentMonth+8 | CurrentMonth+9  | 1000   |
		| CurrentMonth+9 | CurrentMonth+10 | 1000   |

Examples:
	| agreed_price | training_code |
	| 15,000       | 614           |