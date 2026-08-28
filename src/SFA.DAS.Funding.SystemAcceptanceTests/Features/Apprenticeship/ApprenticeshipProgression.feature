Feature: Apprenticeship Progression

As a provider
I want to submit a new apprenticeship for a learner alongside an update to their existing approved apprenticeship
So that the new apprenticeship details can be sent for "legacy" approval via LearnerData

@regression
Scenario: Learner progresses onto a new apprenticeship while an existing one is updated in the same submission
	Given an apprenticeship has a start date of 2024-11-01, a planned end date of 2025-11-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved


