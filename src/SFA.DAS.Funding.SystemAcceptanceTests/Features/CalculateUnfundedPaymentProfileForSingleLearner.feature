Feature: CalculateCoreOnProgUnfundedPaymentProfile

As the Apprenticeship Service SRO
I want payments to be calculated using the learner data collected on the Apprenticeship Service 
So that I minimise the resources required to calculate payments by calculating future payments ahead of time and only recalculating when there is a change to the earnings

#Note that in the below test the training code used is '8' with funding band max as 12000

@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and above Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created
		| AcademicYear | DeliveryPeriod | Amount    | PaymentYear | PaymentPeriod |
		| 2425         | January        | 533.33333 | 2425        | January       |
		| 2425         | February       | 533.33333 | 2425        | February      |
		| 2425         | March          | 533.33333 | 2425        | March         |
		| 2425         | April          | 533.33333 | 2425        | April         |
		| 2425         | May            | 533.33333 | 2425        | May           |
		| 2425         | June           | 533.33333 | 2425        | June          |
		| 2425         | July           | 533.33333 | 2425        | July          |
		| 2526         | August         | 533.33333 | 2526        | August        |
		| 2526         | September      | 533.33333 | 2526        | September     |
		| 2526         | October        | 533.33333 | 2526        | October       |
		| 2526         | November       | 533.33333 | 2526        | November      |
		| 2526         | December       | 533.33333 | 2526        | December      |
		| 2526         | January        | 533.33333 | 2526        | January       |
		| 2526         | February       | 533.33333 | 2526        | February      |
		| 2526         | March          | 533.33333 | 2526        | March         |
		| 2526         | April          | 533.33333 | 2526        | April         |
		| 2526         | May            | 533.33333 | 2526        | May           |
		| 2526         | June           | 533.33333 | 2526        | June          |
	And the newly calculated Unfunded Payments are marked as not sent to payments BAU

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2025-01-01 | 2026-06-30       | 15,000       | 8             |