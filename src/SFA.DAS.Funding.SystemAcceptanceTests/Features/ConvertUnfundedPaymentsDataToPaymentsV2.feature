Feature: ConvertUnfundedPaymentsDataToPaymentsV2

As the DfE 
I want to provide sufficient flexible payment data to Payments v2
So that payments can be generated

@regression
Scenario: Set values required for payments v2 to make a payment
	Given an apprenticeship has a start date in the current month with a duration of 12 months
	And the apprenticeship commitment is approved
	And the Unfunded Payments for the remainder of the apprenticeship are determined
	And the user wants to process payments for the current collection Period
	When the scheduler triggers Unfunded Payment processing
	And the unpaid unfunded payments for the current Collection Month and 0 rollup payments are sent to be paid
	Then 1 Calculated Required Levy Amount event is published with required values