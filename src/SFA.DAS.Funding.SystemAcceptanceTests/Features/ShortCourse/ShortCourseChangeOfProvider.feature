Feature: ShortCourseChangeOfProvider

A short summary of the feature

Scenario: Inform approvals when the same learner/course is created by different training providers
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	Then notify approvals of learner for provider B

Scenario: Same learner/course is created by different training providers - Provider B’s record is not approved
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has not approved the Provider B short course
	Then learning contains an epidose for Provider A and an episode for Provider B
	And earnings contains an episode for Provider A and an episode for Provider B
	And calculate "unapproved earnings" (as if Provider A does not exist) - see FLP-1524.

Scenario: Same learner/course is created by different training providers - Provider B’s record is approved  - milestone(s) are not recorded
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has approved the Provider B short course
	And Provider B has not recorded either the 30% milestone or completion
	Then learning contains an epidose for Provider A and an episode for Provider B
	And earnings contains an episode for Provider A and an episode for Provider B
	And calculate "approved provisional earnings" (as if Provider A does not exist) - see FLP-1415.

Scenario: Same learner/course is created by different training providers - Provider B’s record is approved - milestone(s) are recorded
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has approved the Provider B short course
	And Provider B has recorded the 30% and/or completion milestone
	Then learning contains an epidose for Provider A and an episode for Provider B
	And calculate "approved actual earnings" (as if Provider A does not exist) - see FLP-1624.

Scenario: Provider A's record is unaffected 
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When SLD informs us of the creation of the same learner/course by Provider B following a change of provider (POST)
	Then Provider A's short course record continues to behave as a regular short course, independently of Provider B's record 
	#- Provider A's approved learners are returned to SLD
	#- Provider A's approved earnings are returned to SLD
	#- Payments are informed of Provider A's approved actual earnings
	#- Provider A can withdraw, remove, reinstate the learner


Scenario: Provider B's record behaves as a regular short course
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When SLD informs us of the creation of the same learner/course by Provider B following a change of provider (POST)
	Then Provider B's short course record behaves identically to a regular short course, independently of Provider A's record
	#- Provider B's approved learners are returned to SLD
	#- Provider B's approved earnings are returned to SLD
	#- Payments are informed of Provider B's approved actual earnings
	#- Provider B can withdraw, remove, reinstate the learner