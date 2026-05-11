Feature: Short Course Removal

@regression
Scenario: Learner removed and then reinstated
    Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    When SLD inform us that the learner has been removed
    Then short course learning is removed from learning and earning dbs
    And remove all earnings for that "short course"
    And a learning removed event is published to approvals
    And the 30% milestone earning is not generated and the completion earning is not generated
    #The below steps are to reinstate the learner
    When SLD inform us that the same learner has been reinstated by the training provider
    And the training provider recorded that the 30% milestone has been reached
	Then a learning reinstated event is published to approvals
    And short course learning is reinstated in learning and earning dbs
    And the 30% milestone earning is generated and the completion earning is not generated