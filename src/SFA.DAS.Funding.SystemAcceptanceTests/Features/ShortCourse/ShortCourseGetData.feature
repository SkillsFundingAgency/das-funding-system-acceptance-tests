Feature: Short Course Get Data

@regression
Scenario: Get short course learners (continuing) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (continuing but now completed last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	When SLD informs us the short course learning has completed on previousAY-07-01
	And the short course is approved
	When SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is not returned in the earnings response