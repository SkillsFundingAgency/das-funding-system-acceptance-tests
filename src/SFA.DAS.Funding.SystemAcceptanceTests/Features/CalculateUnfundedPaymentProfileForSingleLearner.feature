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
		| 3536         | August         | 1000   | 3536        | August        |
		| 3536         | September      | 1000   | 3536        | September     |
		| 3536         | October        | 1000   | 3536        | October       |
		| 3536         | November       | 1000   | 3536        | November      |
		| 3536         | December       | 1000   | 3536        | December      |
		| 3536         | January        | 1000   | 3536        | January       |
		| 3536         | February       | 1000   | 3536        | February      |
		| 3536         | March          | 1000   | 3536        | March         |
		| 3536         | April          | 1000   | 3536        | April         |
		| 3536         | May            | 1000   | 3536        | May           |
		| 3536         | June           | 1000   | 3536        | June          |
		| 3536         | July           | 1000   | 3536        | July          |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2035-08-01 | 2036-07-31       | 15,000       | 614           |


@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and below Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created
		| AcademicYear | DeliveryPeriod | Amount | PaymentYear | PaymentPeriod |
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
		| 2526         | January        | 500    | 2526        | January       |
		| 2526         | February       | 500    | 2526        | February      |
		| 2526         | March          | 500    | 2526        | March         |
		| 2526         | April          | 500    | 2526        | April         |
		| 2526         | May            | 500    | 2526        | May           |
		| 2526         | June           | 500    | 2526        | June          |
		| 2526         | July           | 500    | 2526        | July          |
		| 2627         | August         | 500    | 2627        | August        |
		| 2627         | September      | 500    | 2627        | September     |
		| 2627         | October        | 500    | 2627        | October       |
		| 2627         | November       | 500    | 2627        | November      |
		| 2627         | December       | 500    | 2627        | December      |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2025-01-01 | 2026-12-31       | 15,000       | 614           |


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