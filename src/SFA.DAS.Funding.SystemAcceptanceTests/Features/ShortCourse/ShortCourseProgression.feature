Feature: Short Course Progression

Scenario: Learner completes a course and starts a new one with the same provider in the same academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	And the training provider recorded that the 30% milestone has been reached
	And the training provider also recorded that the learner completed
	When SLD submits a progression PUT for a new course with start date currentAY-02-05 alongside the existing course
	Then unapproved earnings are generated for the new course
	And the original course earnings are unaffected

Scenario: Learner completes a course and starts a new one with the same provider in the subsequent academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	And the training provider also recorded that the learner completed pre-approval
	And the short course is approved
	When SLD submits a progression POST for a new course in academic year nextAY with start date nextAY-08-20 
	Then unapproved earnings are generated for the new course
	And the original course earnings are unaffected
