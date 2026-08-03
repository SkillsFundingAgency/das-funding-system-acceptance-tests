Feature: Short Course Progression

@regression
Scenario: Learner completes a course and starts a new one with the same provider in the same academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	And the training provider recorded that the 30% milestone has been reached
	And the training provider also recorded that the learner completed
	When SLD submits a progression PUT for a new course with start date currentAY-02-05 alongside the existing course
	Then unapproved earnings are generated for the new course
	And both original course earnings are unaffected

@regression
Scenario: Learner withdraws a course and starts a new one with the same provider in the same academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	And the training provider recorded that the 30% milestone has been reached
	And SLD inform us that the learner has withdrawn
	When SLD submits a progression PUT for a new course with start date currentAY-02-05 alongside the existing course
	Then unapproved earnings are generated for the new course
	And 30% milestone earning is unaffected

@regression
Scenario: Learner completes a course and starts a new one with the same provider in the subsequent academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	And the training provider also recorded that the learner completed pre-approval
	And the short course is approved
	When SLD submits a progression POST for a new course in academic year nextAY with start date nextAY-08-20 
	Then unapproved earnings are generated for the new course
	And both original course earnings are unaffected

@regression
Scenario: Learner withdraws from a course and starts a new one with the same provider in the subsequent academic year
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	And the training provider also recorded that the learner has withdrawn pre-approval
	And the short course is approved
	When SLD submits a progression POST for a new course in academic year nextAY with start date nextAY-08-20 
	Then unapproved earnings are generated for the new course
	And 30% milestone earning is unaffected

#FLP-1860
@regression
Scenario: Learner completes a course and starts a new one with the same provider in the same academic year - Payable Earnings
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the short course is approved
	And the training provider recorded that the 30% milestone has been reached
	And the training provider also recorded that the learner completed
	When SLD submits a progression PUT for a new course with start date currentAY-02-05 alongside the existing course
	And the new short course is approved
	Then approved earnings are generated for the new course
	And both original course earnings are unaffected
