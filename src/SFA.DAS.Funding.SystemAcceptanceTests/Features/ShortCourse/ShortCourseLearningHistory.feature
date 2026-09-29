Feature: ShortCourseLearningHistory

As a funding platform consumer
I want short course changes to be auditable
So that create/approve/update/remove operations are captured in history

@regression
Scenario: Create, approve, update and remove a short course are logged in history
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	When the training provider recorded that the 30% milestone has been reached
	And SLD inform us that the learner has been removed
    Then the short course learning history contains 1 "Created" operations
	And the short course learning history contains 1 "Approved" operations
	And the short course learning history contains 1 "Updated" operations
	And the short course learning history contains 1 "Removed" operations

@regression
Scenario: Multiple updates are logged as separate history entries
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	When the training provider recorded that the 30% milestone has been reached
	And the training provider recorded that the learner completed
  Then the short course learning history contains 2 "Updated" operations
