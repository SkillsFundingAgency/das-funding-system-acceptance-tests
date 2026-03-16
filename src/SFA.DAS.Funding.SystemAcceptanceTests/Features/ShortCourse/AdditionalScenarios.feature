Feature: Short Course Additional Scenarios

@regression
Scenario: Learner with historical apprenticeship and current short course
	Given an apprenticeship has a start date of previousAY-08-01, a planned end date of previousAY-07-31, an agreed price of 10000, and a training code 241
	And the apprenticeship commitment is approved
	When SLD informs us of a short course for the learner starting on currentAY-08-01
	#And the short course is approved
	Then the basic short course earnings are generated

@regression
Scenario: Learner with historical completed apprenticeship and current short course
	Given an apprenticeship has a start date of previousAY-08-01, a planned end date of previousAY-07-31, an agreed price of 10000, and a training code 241
	And the apprenticeship commitment is approved
	And Learning Completion is recorded on previousAY-07-31
	When SLD informs us of a short course for the learner starting on currentAY-08-01
	Then the basic short course earnings are generated

@regression
Scenario: Learner with historical withdrawn apprenticeship and current short course
	Given an apprenticeship has a start date of previousAY-08-01, a planned end date of previousAY-07-31, an agreed price of 10000, and a training code 241
	And the apprenticeship commitment is approved
	And Learning withdrawal date is recorded on previousAY-09-01
	When SLD informs us of a short course for the learner starting on currentAY-08-01
	Then the basic short course earnings are generated