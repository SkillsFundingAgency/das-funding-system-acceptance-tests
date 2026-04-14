Feature: Short Course Removal

#TODO this test is for 1615 which is yet to be deployed
@regression
@ignore
Scenario: Learner removed
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    When SLD inform us that the learner has been removed
    Then remove all earnings for that "short course"
