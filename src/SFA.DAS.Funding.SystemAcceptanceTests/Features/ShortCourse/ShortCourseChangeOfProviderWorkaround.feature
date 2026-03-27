Feature: Short Course Change of Provider Workaround

@regression
Scenario: Pre-approval change of provider
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When SLD informs us the short course changes provider
	Then the second POST call returns gracefully
	And the earnings are still recorded against the first provider