Feature: LegacyApprenticeshipRoute

The purpose of this test is to verify that Apprenticeships created via the "legacy" approval route - that is, records that are never
POSTed as a draft into Learning before being approved in Approvals - are still created in Learning, but do not generate earnings at all.

@regression
Scenario: Legacy approval route creates learning but does not generate earnings
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	When the apprenticeship commitment is approved via the legacy route
	Then the learning is created
	And no earnings are generated for the apprenticeship
