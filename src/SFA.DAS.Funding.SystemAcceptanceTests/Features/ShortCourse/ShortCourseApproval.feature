Feature: Short Course Approval

Scenario: Short course is approved
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When the short course is approved
	Then the short course earnings are set to approved
	And the learning domain is updated correctly
	And the short course is set to approved
	And the episode keys match between the learning and earnings databases

Scenario: Only process the earliest OnProg for a learner
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And SLD informs us of a the same learner with a short course starting on currentAY-09-01
	When both short courses are approved
	Then only earnings are generated for the earliest short course
