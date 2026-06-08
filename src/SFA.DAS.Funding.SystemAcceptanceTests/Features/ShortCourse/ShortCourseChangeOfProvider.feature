Feature: ShortCourseChangeOfProvider

As the DfE
I want to recalculate earnings when a learner changes training provider and stays on the same course
So that earnings are calculated accurately and meet the funding rules 

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
	And the short course learner is returned in the approved ulns response without duplicates
	When SLD requests short course earnings data for provider A and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as approved in the earnings response
	# Validations for Provider B's record
	When SLD requests short course approved ulns for Provider B in academic year currentAY
	Then the short course learner is not returned in the approved ulns response
	When SLD requests short course earnings data for provider B and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as unapproved in the earnings response


# FLP-1522 - AC2
Scenario: Learner changes provider, Provider A: 30% earning only - calculate "unapproved earnings"
	Given that a “short course” learner has been created by Provider A
	And the employer has approved the Provider A short course
	And Provider A has recorded 30% isPayable and completion isNotPayable
	And SLD inform us that the learner has withdrawn
	When that a “short course” learner has been created by Provider B
	And the employer has not approved the Provider B short course
	Then earnings instalments are calculated as follows
		| Provider | ThirtyPercent | Completion          |
		| B        | DoesNotExist  | ExistsButNotPayable |
	When SLD requests short course earnings data for provider B and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as unapproved in the earnings response

# FLP-1522 - AC3
Scenario: Learner changes provider, Provider A: 30% earning only, Provider B attempts to claim - calculate "unapproved earnings"
	Given that a “short course” learner has been created by Provider A
	And the employer has approved the Provider A short course
	And Provider A has recorded 30% isPayable and completion isNotPayable
	And SLD inform us that the learner has withdrawn
	When that a “short course” learner has been created with 30% milestone by Provider B
	And the employer has not approved the Provider B short course
	Then earnings instalments are calculated as follows
		| Provider | ThirtyPercent | Completion          |
		| B        | DoesNotExist  | ExistsButNotPayable |
	When SLD requests short course earnings data for provider B and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as unapproved in the earnings response

# FLP-1809 - AC2
Scenario: Learner changes provider, Provider A: 30% earning only, Provider B is approved but no milestones claimed - calculate approved provisional earnings
	Given that a “short course” learner has been created by Provider A
	And the employer has approved the Provider A short course
	And Provider A has recorded 30% isPayable and completion isNotPayable
	And SLD inform us that the learner has withdrawn
	When that a “short course” learner has been created by Provider B
	And the employer has approved the Provider B short course
	Then earnings instalments are calculated as follows
		| Provider | ThirtyPercent | Completion          |
		| A        | Payable       | DoesNotExist        |
		| B        | DoesNotExist  | ExistsButNotPayable |
	When SLD requests short course earnings data for provider B and collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates
	And the short course learner is returned as approved in the earnings response