Feature: Short Course Milestone Earnings

@regression
Scenario: Milestone Earnings - 30% milestone reached
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    When the training provider recorded that the 30% milestone has been reached
    Then the 30% milestone earning is generated and the completion earning is not generated

@regression
Scenario: Milestone Earnings - completion reached
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    When the training provider recorded that the learner completed
    Then the 30% milestone earning is not generated and the completion earning is generated

@regression
Scenario: Milestone Earnings - 30% milestone and completion reached
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    When the training provider also recorded that the learner completed
    Then the 30% milestone earning is generated and the completion earning is generated

@regression
Scenario: Milestone Earnings - no milestones reached
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When the short course is approved
    Then the 30% milestone earning is not generated and the completion earning is not generated

@regression
Scenario: Milestone Earnings - milestones recorded pre-approval
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
    And the training provider recorded that the 30% milestone has been reached pre-approval
    And the training provider also recorded that the learner completed pre-approval
    When the short course is approved
    Then the 30% milestone earning is generated and the completion earning is generated