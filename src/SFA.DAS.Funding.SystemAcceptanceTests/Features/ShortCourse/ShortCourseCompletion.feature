Feature: Short Course Completion

@regression
Scenario: Existing short course learning is completed with a later completion date
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When SLD informs us the short course learning has completed on currentAY-05-01
	Then the second instalment is earnt in period currentAY-R10

@regression
Scenario: Existing short course learning is completed with an earlier completion date
	Given SLD informs us of a new learner with a short course starting on currentAY-08-01
	When SLD informs us the short course learning has completed on currentAY-09-01
	Then the second instalment is earnt in period currentAY-R02