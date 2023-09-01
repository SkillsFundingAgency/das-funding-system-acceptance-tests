Feature: CalculateCoreOnProgUnfundedPaymentProfile

As the Apprenticeship Service SRO
I want payments to be calculated using the learner data collected on the Apprenticeship Service 
So that I minimise the resources required to calculate payments by calculating future payments ahead of time and only recalculating when there is a change to the earnings

@regression
Scenario: Unfunded Payment profile for single learner - Same Academic Year and below Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created
		| AcademicYear | DeliveryPeriod | Amount | PaymentYear | PaymentPeriod |
		| 2425         | August         | 1000   | 2425        | August        |
		| 2425         | September      | 1000   | 2425        | September     |
		| 2425         | October        | 1000   | 2425        | October       |
		| 2425         | November       | 1000   | 2425        | November      |
		| 2425         | December       | 1000   | 2425        | December      |
		| 2425         | January        | 1000   | 2425        | January       |
		| 2425         | February       | 1000   | 2425        | February      |
		| 2425         | March          | 1000   | 2425        | March         |
		| 2425         | April          | 1000   | 2425        | April         |
		| 2425         | May            | 1000   | 2425        | May           |
		| 2425         | June           | 1000   | 2425        | June          |
		| 2425         | July           | 1000   | 2425        | July          |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2024-08-01 | 2025-07-31       | 15,000       | 614           |


@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and below Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created
		| AcademicYear | DeliveryPeriod | Amount | PaymentYear | PaymentPeriod |
		| 2324         | January        | 500    | 2324        | January       |
		| 2324         | February       | 500    | 2324        | February      |
		| 2324         | March          | 500    | 2324        | March         |
		| 2324         | April          | 500    | 2324        | April         |
		| 2324         | May            | 500    | 2324        | May           |
		| 2324         | June           | 500    | 2324        | June          |
		| 2324         | July           | 500    | 2324        | July          |
		| 2425         | August         | 500    | 2425        | August        |
		| 2425         | September      | 500    | 2425        | September     |
		| 2425         | October        | 500    | 2425        | October       |
		| 2425         | November       | 500    | 2425        | November      |
		| 2425         | December       | 500    | 2425        | December      |
		| 2425         | January        | 500    | 2425        | January       |
		| 2425         | February       | 500    | 2425        | February      |
		| 2425         | March          | 500    | 2425        | March         |
		| 2425         | April          | 500    | 2425        | April         |
		| 2425         | May            | 500    | 2425        | May           |
		| 2425         | June           | 500    | 2425        | June          |
		| 2425         | July           | 500    | 2425        | July          |
		| 2526         | August         | 500    | 2526        | August        |
		| 2526         | September      | 500    | 2526        | September     |
		| 2526         | October        | 500    | 2526        | October       |
		| 2526         | November       | 500    | 2526        | November      |
		| 2526         | December       | 500    | 2526        | December      |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2024-01-01 | 2025-12-31       | 15,000       | 614           |


#Note that in the below test the training code used is '8' with funding band max as 12000

@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and above Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created
		| AcademicYear | DeliveryPeriod | Amount    | PaymentYear | PaymentPeriod |
		| 2324         | January        | 533.33333 | 2324        | January       |
		| 2324         | February       | 533.33333 | 2324        | February      |
		| 2324         | March          | 533.33333 | 2324        | March         |
		| 2324         | April          | 533.33333 | 2324        | April         |
		| 2324         | May            | 533.33333 | 2324        | May           |
		| 2324         | June           | 533.33333 | 2324        | June          |
		| 2324         | July           | 533.33333 | 2324        | July          |
		| 2425         | August         | 533.33333 | 2425        | August        |
		| 2425         | September      | 533.33333 | 2425        | September     |
		| 2425         | October        | 533.33333 | 2425        | October       |
		| 2425         | November       | 533.33333 | 2425        | November      |
		| 2425         | December       | 533.33333 | 2425        | December      |
		| 2425         | January        | 533.33333 | 2425        | January       |
		| 2425         | February       | 533.33333 | 2425        | February      |
		| 2425         | March          | 533.33333 | 2425        | March         |
		| 2425         | April          | 533.33333 | 2425        | April         |
		| 2425         | May            | 533.33333 | 2425        | May           |
		| 2425         | June           | 533.33333 | 2425        | June          |
	And the newly calculated Unfunded Payments are marked as not sent to payments BAU

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2024-01-01 | 2025-06-30       | 15,000       | 8             |