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
	And earnings instalments are calculated as follows
		| Provider | ThirtyPercent       | Completion          |
		| A        | ExistsButNotPayable | ExistsButNotPayable |
		| B        | ExistsButNotPayable | ExistsButNotPayable |

Scenario: Same learner/course is created by different training providers - Provider B’s record is approved  - milestone(s) are not recorded
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has approved the Provider B short course
	And Provider B has recorded 30% isNotPayable and completion isNotPayable
	Then learning contains an epidose for Provider A and an episode for Provider B
	And earnings contains an episode for Provider A and an episode for Provider B
	And earnings instalments are calculated as follows
		| Provider | ThirtyPercent       | Completion          |
		| A        | ExistsButNotPayable | ExistsButNotPayable |
		| B        | ExistsButNotPayable | ExistsButNotPayable |

Scenario: Same learner/course is created by different training providers - Provider B’s record is approved - milestone(s) are recorded
	Given that a “short course” learner has been created by Provider A
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has approved the Provider B short course
	And Provider B has recorded 30% isPayable and completion isNotPayable
	Then learning contains an epidose for Provider A and an episode for Provider B
	And earnings instalments are calculated as follows
		| Provider | ThirtyPercent       | Completion          |
		| A        | ExistsButNotPayable | ExistsButNotPayable |
		| B        | Payable             | ExistsButNotPayable |

Scenario: Provider A's record is unaffected 
	Given that a “short course” learner has been created by Provider A
	And the employer has approved the Provider A short course
	And the learner has not completed the course with Provider A
	When that a “short course” learner has been created by Provider B
	And the employer has not approved the Provider B short course
	# Validations for Provider A's record
	Then learning contains an epidose for Provider A and an episode for Provider B
	And SLD requests short course approved ulns for Provider A in academic year currentAY
	Then the short course learner is returned in the approved ulns response without duplicates
	When SLD requests short course earnings data for provider A and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as approved in the earnings response
	# Validations for Provider B's record
	When SLD requests short course approved ulns for Provider B in academic year currentAY
	Then the short course learner is not returned in the approved ulns response
	When SLD requests short course earnings data for provider B and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as unapproved in the earnings response
