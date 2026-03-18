Feature: Short Course Get

@regression
Scenario: Get short course learners for academic year
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course learners for academic year currentAY
	Then the short course learner is returned in the response without duplicates

@regression
Scenario: Get short course learners for academic year does not return learners from other academic years
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course learners for academic year previousAY
	Then the short course learner is not returned in the response

@regression
Scenario: Get short course learners (continuing) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course learners for academic year currentAY
	Then the short course learner is returned in the response without duplicates

@regression
Scenario: Get short course learners (continuing but now completed last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	When SLD informs us the short course learning has completed on previousAY-07-01
	And the short course is approved
	When SLD requests short course learners for academic year currentAY
	Then the short course learner is not returned in the response