Feature: Short Course Creation

@regression
Scenario: New learner is created with a short course
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	Then the basic short course earnings are generated
	And the learning domain is updated correctly
	And the short course is set to unapproved

@regression
Scenario: Existing learner is updated when a short course is added with updated learner details
	Given an apprenticeship has a start date of previousAY-08-01, a planned end date of previousAY-07-31, an agreed price of 10000, and a training code 241
	And the apprenticeship commitment is approved
	When SLD informs us of a short course for the learner starting on currentAY-08-01 with updated learner details
		| FirstName | LastName | DateOfBirth | EmailAddress      |
		| Shaun     | Murphy   | 1999-09-09  | shaun@example.com |
	Then the learning domain is updated correctly

@regression
Scenario: Initial short course earnings are calculated for a learner in a hard closed academic year
	Given SLD informs us of a new learner with a short course starting on TwoYearsAgoAY-08-01
	Then the basic short course earnings are generated
	And the learning domain is updated correctly
	And the short course is set to unapproved