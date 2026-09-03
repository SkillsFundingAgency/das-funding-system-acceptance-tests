Feature: NonEnrolment

As DfE finance
I want no earnings calculated outside FLP-2012's opt-in rules
So that only compliant providers and start dates are funded

@regression
Scenario: Draft apprenticeship with a start date before 01 August 2026 generates no earnings
	Given an apprenticeship has a start date of 2026-07-31, a planned end date of nextAY-07-31, an agreed price of 15000, and a training code 2
	When the apprenticeship commitment is approved
	Then the learning is created
	And no earnings are generated for the apprenticeship
