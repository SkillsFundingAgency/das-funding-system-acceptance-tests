Feature: Short Course Get Data
#This file contains the 8 scenarios in order for FLP-1673

@regression
Scenario: Get short course learners (continuing) for academic year
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (completed) for academic year
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	And SLD informs us the short course learning has completed on currentAY-09-01
	And the short course is approved
	When SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (continuing, planned for last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on previousAY-09-01
	And the basic short course earnings are generated
	When the short course is approved
	And SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on previousAY-09-01
	And the basic short course earnings are generated
	And SLD informs us the short course learning has completed on previousAY-09-01
	When the short course is approved
	And SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is not returned in the earnings response

@regression
Scenario: Get short course learners (planned to end this year but now completed last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	When SLD informs us the short course learning has completed on previousAY-07-01
	And the short course is approved
	And SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is not returned in the earnings response

@regression
Scenario: Get short course learners (completed this year, planned for last year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on previousAY-09-01
	And the basic short course earnings are generated
	When SLD informs us the short course learning has completed on currentAY-09-01
	When the short course is approved
	And SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (spanning both years and completed this year) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	When SLD informs us the short course learning has completed on currentAY-09-01
	And the short course is approved
	And SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates

@regression
Scenario: Get short course learners (spanning both years and continuing) for academic year
	Given SLD informs us of a new learner with a short course starting on previousAY-05-01 and ending on currentAY-09-01
	And the basic short course earnings are generated
	And the short course is approved
	When SLD requests short course earnings data for collection period currentAY-01
	Then the short course learner is returned in the earnings response without duplicates