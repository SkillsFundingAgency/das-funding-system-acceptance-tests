Feature: ReleasePaymentsNonLevyFor22YearOld

As the DfE 
I want to provide sufficient flexible payment data to Payments v2
So that payments can be generated

@releasesPayments
Scenario: Set values required for payments v2 to make a payment - 1 payment for current collection period - Non-Levy employer - Apprentice age at 22
	Given an apprenticeship has a start date in the current month with a duration of 12 months
	And the apprenticeship learner's age is at 22
	And apprenticeship employer type is NonLevy
	And the apprenticeship commitment is approved
	And Payments Generated Events are published
	And the user wants to process payments for the current collection Period
	When the scheduler triggers Unfunded Payment processing
	And the unpaid unfunded payments for the current Collection Month and 0 rollup payments are sent to be paid
	Then 1 Calculated Required Levy Amount event is published with required values
