Feature: Short Course Progression

Scenario: Learner completes a course and starts a new one with the same provider in the same academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	And the training provider recorded that the 30% milestone has been reached
	And the training provider also recorded that the learner completed
	When SLD submits a progression PUT for a new course alongside the existing course
	Then unapproved earnings are generated for the new course
	And the original course earnings are unaffected
