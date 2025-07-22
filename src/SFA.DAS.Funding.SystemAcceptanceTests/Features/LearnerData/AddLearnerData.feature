Feature: AddLearnerData

As the SLD
I want to inform DAS of new Learners added via the ILR
So that Apprenticeships can be created within Approvals

@regression
Scenario: Add a Learner
	When SLD inform us of a new Learner
	Then the learner is added to LearnerData
