Feature: NonEnrolment

As DfE finance
I want no earnings calculated for Providers who are not enrolled, or for Apprenticeships starting prior to 01-Aug-2026

@regression
Scenario: Apprenticeship with a start date before 01 August 2026 generates no earnings
	Given an apprenticeship has a start date of 2026-07-31, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	When the apprenticeship commitment is approved
	Then the learning is created
	And no earnings are generated for the apprenticeship

@regression
Scenario: Apprenticeship for a non-enrolled provider generates no earnings
	Given an apprenticeship has a start date of 2026-08-01, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	And the provider is not enrolled
	When the apprenticeship commitment is approved
	Then the learning is created
	And no earnings are generated for the apprenticeship
