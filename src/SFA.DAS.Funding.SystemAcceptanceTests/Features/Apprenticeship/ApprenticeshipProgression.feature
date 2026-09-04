Feature: Apprenticeship Progression

As a provider
I want to submit a new apprenticeship for a learner alongside an update to their existing approved apprenticeship
So that the new apprenticeship details can be sent for "legacy" approval via LearnerData

Mid-academic year means this is a PUT operation; we already knew about the learner's original learning, but now
they have progressed onto a new apprenticeship. This new apprenticeship will not be POSTed to us, but will come in via the PUT.

@regression
Scenario: Learner progresses onto a new apprenticeship mid-academic year
	Given an apprenticeship has a start date of 2024-11-01, a planned end date of 2025-11-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved


