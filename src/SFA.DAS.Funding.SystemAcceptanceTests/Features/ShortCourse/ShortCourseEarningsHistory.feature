Feature: Short Course Earnings History

@regression
Scenario: History is maintained after multiple updates
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    And the training provider also recorded that the learner completed
	Then 3 earnings profile history records are created for the short course