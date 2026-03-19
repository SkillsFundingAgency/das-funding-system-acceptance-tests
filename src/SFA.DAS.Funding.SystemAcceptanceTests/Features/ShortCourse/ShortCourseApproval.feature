Feature: Short Course Approval

Scenario: Short course is approved
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When the short course is approved
	Then the short course earnings are set to approved
	And the learning domain is updated correctly
	And the short course is set to approved
