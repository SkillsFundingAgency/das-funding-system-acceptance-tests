Feature: SendUnfundedPaymentsToBePaid

As a AS SRO 
I want payments Unfunded Payments that are due to be paid to be sent for payment
So that Providers are paid what they are owed at the right time


Background: 
    Given an apprenticeship with start date over 2 months ago and duration of 12 months and an agreed price of 15,000, and a training code 614
	And the apprenticeship commitment is approved
	And the Unfunded Payments for the remainder of the apprenticeship are determined
	And the user wants to process payments for the current collection Period 
	When the scheduler triggers Unfunded Payment processing

@regression
Scenario: Send unfunded payments for the current collection period to be paid
	When the unpaid unfunded payments for the current Collection Month and 2 rollup payments are sent to be paid 
	Then the amount of 1000 is sent to be paid for each payment in the curent Collection Month
	And the relevant payments entities are marked as sent to payments BAU
	And all payments for the following collection periods are marked as not sent to payments BAU


@regression
Scenario: Nothing is sent if all Unfunded Payments for the current Collection Month have already been sent
	When the unpaid unfunded payments for the current Collection Month and 2 rollup payments are sent to be paid
	And the Release Payments command is published again
	Then the unfunded payments that have already been sent to Payments BAU are not sent to be paid again


