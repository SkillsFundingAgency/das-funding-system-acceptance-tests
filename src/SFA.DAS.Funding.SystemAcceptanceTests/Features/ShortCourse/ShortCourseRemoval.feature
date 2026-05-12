Feature: Short Course Removal

Background:
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached

@regression
Scenario: Short Course Learner removed from the ILR   
    When SLD inform us that the learner has been removed
    Then short course learning is removed from learning and earning dbs
    And remove all earnings for that "short course"
    And a learning removed event is published to approvals
    And the 30% milestone earning is not generated and the completion earning is not generated

Scenario: Short Course Learner removed from the ILR and Get Learners endpoint called
    When SLD inform us that the learner has been removed
    And SLD requests short course approved ulns for academic year currentAY
	Then the short course learner is not returned in the approved ulns response

Scenario: Short Course Learner removed from the ILR and Get Earnings endpoint called
    When SLD inform us that the learner has been removed
    And  SLD requests short course earnings data for collection period currentAY-01
    Then the short course learner is not returned in the earnings response

Scenario: Short Course Learner removed from the ILR and then reinstated 
    When SLD inform us that the learner has been removed
    And SLD inform us that the same learner has been reinstated by the training provider
    And the training provider recorded that the 30% milestone has been reached
	Then a learning reinstated event is published to approvals
    And short course learning is reinstated in learning and earning dbs
    And the 30% milestone earning is generated and the completion earning is not generated

Scenario: Short Course Learner removed from the ILR then reinstated and Get Learners endpoint called
    When SLD inform us that the learner has been removed
    And SLD inform us that the same learner has been reinstated by the training provider
    And SLD requests short course approved ulns for academic year currentAY
	Then the short course learner is returned in the approved ulns response without duplicates

Scenario: Short Course Learner removed from the ILR then reinstated and Get Earnings endpoint called
    When SLD inform us that the learner has been removed
    And SLD inform us that the same learner has been reinstated by the training provider
    And SLD requests short course earnings data for collection period currentAY-01
    Then the short course learner is returned in the earnings response without duplicates