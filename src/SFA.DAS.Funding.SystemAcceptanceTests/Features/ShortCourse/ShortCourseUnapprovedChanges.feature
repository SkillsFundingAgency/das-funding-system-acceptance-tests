Feature: Changes to a short course pre-approval

@regression
Scenario: Short course dates are changed pre-approval
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When SLD informs us of a change to the short course dates pre approval
	Then the basic short course earnings are generated
	And the learning domain is updated correctly
	And the short course is set to unapproved