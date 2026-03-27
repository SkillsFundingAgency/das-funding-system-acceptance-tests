Feature: Short Course Approvals Integration

@regression
Scenario: Send created short course data to approvals
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	Then the short course data is sent to approvals