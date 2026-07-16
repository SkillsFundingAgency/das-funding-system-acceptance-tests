Feature: Short Course Approval

Scenario: Short course is approved
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	When the short course is approved
	Then the short course earnings are set to approved
	And the learning domain is updated correctly
	And the short course is set to approved
	And the episode keys match between the learning and earnings databases

Scenario: Short course is approved with Transfer
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	When the short course is approved through a transfer with employer type <employer_type>
	Then the learning domain is updated correctly
	And the short course is set to approved

Examples:
	| employer_type |
	| Levy          |
	| NonLevy       |

