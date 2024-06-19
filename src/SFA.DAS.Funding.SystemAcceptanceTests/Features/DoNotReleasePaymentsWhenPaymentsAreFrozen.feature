Feature: DoNotReleasePaymentsWhenPaymentsAreFrozen

A short summary of the feature

@regression
Scenario: Do not realese payments when payments are frozen
	Given earnings have been calculated for an apprenticeship with currentAY-08-23, currentAYPlusTwo-08-23, 15000, and 2
	And the user wants to process payments for the current collection Period 
	When Employer has frozen provider payments 
	And the scheduler triggers Unfunded Payment processing
	Then do not make an on-programme payment to the training provider for that apprentice