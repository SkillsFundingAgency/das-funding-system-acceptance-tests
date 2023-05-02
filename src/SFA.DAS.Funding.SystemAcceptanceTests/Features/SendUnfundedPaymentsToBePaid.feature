Feature: SendUnfundedPaymentsToBePaid

As a AS SRO 
I want payments Unfunded Payments that are due to be paid to be sent for payment
So that Providers are paid what they are owed at the right time


# Question 1. where are the Objects on the conluence page

Background: 
    Given  an apprenticeship with start date over 2 months ago and duration of 12 months and an agreed price of 15,000, and a training code 614
	And the apprenticeship commitment is approved
	When the Unfunded Payments for the remainder of the apprenticeship are determined

@regression
Scenario: Send unfunded payments for the collection Period sepecified 
	Given the user wants to process payments for the current collection Period 
	When the scheduler triggers Unfunded Payment processing
	Then all the unpaid unfunded payments for the specified Collection Month are sent to be paid 
	And the amount of 1000 is sent to be paid for the current apprenticeship
