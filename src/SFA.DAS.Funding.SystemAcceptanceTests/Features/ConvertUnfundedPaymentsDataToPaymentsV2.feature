﻿Feature: ConvertUnfundedPaymentsDataToPaymentsV2

As the DfE 
I want to provide sufficient flexible payment data to Payments v2
So that payments can be generated

@regression
Scenario: Set default values required for payments v2 to make a payment
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the user wants to process payments for the current collection Period 
	When the scheduler triggers Unfunded Payment processing
	Then the Calculated Required Levy Amount event is published with default values

	Examples:
	| start_date | planned_end_date | agreed_price | training_code |
	| 2023-08-01 | 2024-07-31       | 15,000       | 614           |