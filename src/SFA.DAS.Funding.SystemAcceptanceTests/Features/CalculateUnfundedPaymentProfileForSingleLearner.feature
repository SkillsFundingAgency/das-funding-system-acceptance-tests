﻿Feature: CalculateCoreOnProgUnfundedPaymentProfile

As the Apprenticeship Service SRO
I want payments to be calculated using the learner data collected on the Apprenticeship Service 
So that I minimise the resources required to calculate payments by calculating future payments ahead of time and only recalculating when there is a change to the earnings

@regression
Scenario: Unfunded Payment profile for single learner - Same Academic Year and below Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created in the following month
		| AcademicYear | DeliveryPeriod | Amount | PaymentYear | PaymentPeriod |
		| 2324         | August         | 1000   | 2324        | September     |
		| 2324         | September      | 1000   | 2324        | October       |
		| 2324         | October        | 1000   | 2324        | November      |
		| 2324         | November       | 1000   | 2324        | December      |
		| 2324         | December       | 1000   | 2324        | January       |
		| 2324         | January        | 1000   | 2324        | February      |
		| 2324         | February       | 1000   | 2324        | March         |
		| 2324         | March          | 1000   | 2324        | April         |
		| 2324         | April          | 1000   | 2324        | May           |
		| 2324         | May            | 1000   | 2324        | June          |
		| 2324         | June           | 1000   | 2324        | July          |
		| 2324         | July           | 1000   | 2425        | August        |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2023-08-01 | 2024-07-31       | 15,000       | 614           |


@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and below Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created in the following month
		| AcademicYear | DeliveryPeriod | Amount | PaymentYear | PaymentPeriod |
		| 2223         | January        | 500    | 2223        | February      |
		| 2223         | February       | 500    | 2223        | March         |
		| 2223         | March          | 500    | 2223        | April         |
		| 2223         | April          | 500    | 2223        | May           |
		| 2223         | May            | 500    | 2223        | June          |
		| 2223         | June           | 500    | 2223        | July          |
		| 2223         | July           | 500    | 2324        | August        |
		| 2324         | August         | 500    | 2324        | September     |
		| 2324         | September      | 500    | 2324        | October       |
		| 2324         | October        | 500    | 2324        | November      |
		| 2324         | November       | 500    | 2324        | December      |
		| 2324         | December       | 500    | 2324        | January       |
		| 2324         | January        | 500    | 2324        | February      |
		| 2324         | February       | 500    | 2324        | March         |
		| 2324         | March          | 500    | 2324        | April         |
		| 2324         | April          | 500    | 2324        | May           |
		| 2324         | May            | 500    | 2324        | June          |
		| 2324         | June           | 500    | 2324        | July          |
		| 2324         | July           | 500    | 2425        | August        |
		| 2425         | August         | 500    | 2425        | September     |
		| 2425         | September      | 500    | 2425        | October       |
		| 2425         | October        | 500    | 2425        | November      |
		| 2425         | November       | 500    | 2425        | December      |
		| 2425         | December       | 500    | 2425        | January       |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2023-01-01 | 2024-12-31       | 15,000       | 614           |


#Note that in the below test the training code used is '8' with funding band max as 12000

@regression
Scenario: Unfunded Payment profile for single learner - Different Academic Years and above Funding Band Max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined
	Then the Unfunded Payments for every earning is created in the following month
		| AcademicYear | DeliveryPeriod | Amount    | PaymentYear | PaymentPeriod |
		| 2122         | January        | 533.33333 | 2122        | February      |
		| 2122         | February       | 533.33333 | 2122        | March         |
		| 2122         | March          | 533.33333 | 2122        | April         |
		| 2122         | April          | 533.33333 | 2122        | May           |
		| 2122         | May            | 533.33333 | 2122        | June          |
		| 2122         | June           | 533.33333 | 2122        | July          |
		| 2122         | July           | 533.33333 | 2223        | August        |
		| 2223         | August         | 533.33333 | 2223        | September     |
		| 2223         | September      | 533.33333 | 2223        | October       |
		| 2223         | October        | 533.33333 | 2223        | November      |
		| 2223         | November       | 533.33333 | 2223        | December      |
		| 2223         | December       | 533.33333 | 2223        | January       |
		| 2223         | January        | 533.33333 | 2223        | February      |
		| 2223         | February       | 533.33333 | 2223        | March         |
		| 2223         | March          | 533.33333 | 2223        | April         |
		| 2223         | April          | 533.33333 | 2223        | May           |
		| 2223         | May            | 533.33333 | 2223        | June          |
		| 2223         | June           | 533.33333 | 2223        | July          |

Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2022-01-01 | 2023-06-30       | 15,000       | 8             |