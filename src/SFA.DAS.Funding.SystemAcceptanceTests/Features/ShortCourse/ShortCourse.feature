Feature: Short Course Additional Scenarios

@regression
Scenario: Learner with historical apprenticeship and current short course
	Given an apprenticeship has a start date of previousAY-08-01, a planned end date of previousAY-07-31, an agreed price of 10000, and a training code 241
	And the apprenticeship commitment is approved
	When SLD informs us of a short course for the learner in the current academic year
	Then the basic short course earnings are generated