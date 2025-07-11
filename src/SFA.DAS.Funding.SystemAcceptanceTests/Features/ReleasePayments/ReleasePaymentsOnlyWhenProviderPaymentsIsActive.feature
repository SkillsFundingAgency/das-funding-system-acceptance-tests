﻿Feature: ReleasePaymentsOnlyWhenProviderPaymentsStatusIsActive

The purpose of this test is to hold back provider payments when the payment status is set to "Inactive" by Employer.
When provider payment status is set back to "Active" release payments for current collection period as well as 
previous unpaid periods.

Scenario: Delay The Following Test as it is the only release payments test that is not run in the current time period
	Given pause 180 seconds

@regression @releasesPayments
Scenario: ReleasePaymentsOnlyWhenProviderPaymentsStatusIsActive
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of currentAYPlusTwo-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	And the user wants to process payments for the current collection Period 
	When Employer has frozen provider payments 
	And the scheduler triggers Unfunded Payment processing
	Then do not make an on-programme payment to the training provider for that apprentice
	And Employer has unfrozen provider payments 
	And validate payments are not frozen in the payments entity
	And the scheduler triggers Unfunded Payment processing for following collection period
	And make any on-programme payments to the provider that were not paid whilst the payment status was Inactive