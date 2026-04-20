Feature: Short Course Withdrawal

@regression
Scenario: Learner withdrawn - 30% milestone reached (and subsequently removed)
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    When SLD inform us that the learner has withdrawn
    And SLD also inform us that the 30% milestone was removed
    Then remove all earnings for that "short course"
    And inform approvals that the learner has been withdrawn from the short course
    And inform payments that the learner has been withdrawn from the short course

@regression
Scenario: Learner withdrawn - 30% milestone reached (and retained despite the withdrawal)
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    When SLD inform us that the learner has withdrawn
    And SLD also inform us that the 30% milestone was not removed
    Then remove the remaining completion earning
    And retain the 30% milestone earning
    And inform approvals that the learner has been withdrawn from the short course
    And inform payments that the learner has been withdrawn from the short course

@regression
Scenario: Learner withdrawn - milestone(s) not reached 
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    When SLD inform us that the learner has withdrawn
    Then remove all earnings for that "short course"
    And inform approvals that the learner has been withdrawn from the short course
    And inform payments that the learner has been withdrawn from the short course

@regression
Scenario: Learner recorded as “Completed” and subsequently withdrawn with 30% milestone
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider recorded that the 30% milestone has been reached
    And the training provider also recorded that the learner completed
    When SLD inform us that the learner has withdrawn
    Then remove the completion earning
    And retain the 30% milestone earning
    And inform approvals that the learner has been withdrawn from the short course
    And inform payments that the learner has been withdrawn from the short course

@regression
Scenario: Learner recorded as “Completed” and subsequently withdrawn without 30% milestone
    Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	And the short course is approved
    And the training provider also recorded that the learner completed
    When SLD inform us that the learner has withdrawn
    Then remove the completion earning
    And remove the 30% milestone earning
    And inform approvals that the learner has been withdrawn from the short course
    And inform payments that the learner has been withdrawn from the short course
