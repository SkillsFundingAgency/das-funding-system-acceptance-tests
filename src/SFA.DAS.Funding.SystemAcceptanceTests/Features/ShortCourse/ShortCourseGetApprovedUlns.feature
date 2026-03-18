Feature: Short Course Get Approved Ulns

@regression
Scenario: Get short course learners for academic year
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course approved ulns for academic year currentAY
	Then the short course learner is returned in the approved ulns response without duplicates

@regression
Scenario: Get short course learners for academic year does not return learners from other academic years
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course approved ulns for academic year previousAY
	Then the short course learner is not returned in the approved ulns response