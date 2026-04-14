Feature: Short Course Get Data
#This file contains the 8 scenarios in order for FLP-1673

@regression
Scenario Outline: Get short course learners for academic year
	Given SLD informs us of a new learner with a short course starting on <StartDate> and ending on <PlannedEndDate>
	And the basic short course earnings are generated
	And SLD informs us the short course learning has completed on <ActualEndDate> if applicable
	And the short course is <ApprovalStatus>
	When SLD requests short course earnings data for collection period currentAY-01
	Then <ExpectedResult>

Examples: 
	| StartDate        | PlannedEndDate   | ActualEndDate    | ApprovalStatus | ExpectedResult                                                                   | 
	| currentAY-08-01  | currentAY-09-01  | N/A              | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (continuing) for academic year
	| currentAY-08-01  | currentAY-09-01  | currentAY-09-01  | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (completed) for academic year
	| previousAY-05-01 | previousAY-09-01 | N/A              | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (continuing, planned for last year) for academic year
	| previousAY-05-01 | previousAY-09-01 | previousAY-09-01 | approved       | the short course learner is not returned in the earnings response                | # Get short course learners (last year) for academic year
	| previousAY-05-01 | currentAY-09-01  | previousAY-07-01 | approved       | the short course learner is not returned in the earnings response                | # Get short course learners (planned to end this year but now completed last year) for academic year
	| previousAY-05-01 | previousAY-09-01 | currentAY-09-01  | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (completed this year, planned for last year) for academic year
	| previousAY-05-01 | currentAY-09-01  | currentAY-09-01  | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (spanning both years and completed this year) for academic year
	| previousAY-05-01 | currentAY-09-01  | N/A              | approved       | the short course learner is returned in the earnings response without duplicates | # Get short course learners (spanning both years and continuing) for academic year
	| currentAY-08-01  | currentAY-09-01  | N/A              | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (continuing) for academic year (not approved)
	| currentAY-08-01  | currentAY-09-01  | currentAY-09-01  | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (completed) for academic year (not approved)
	| previousAY-05-01 | previousAY-09-01 | N/A              | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (continuing, planned for last year) for academic year (not approved)
	| previousAY-05-01 | previousAY-09-01 | previousAY-09-01 | not approved   | the short course learner is not returned in the earnings response                | # Get short course learners (last year) for academic year (not approved)
	| previousAY-05-01 | currentAY-09-01  | previousAY-07-01 | not approved   | the short course learner is not returned in the earnings response                | # Get short course learners (planned to end this year but now completed last year) for academic year (not approved)
	| previousAY-05-01 | previousAY-09-01 | currentAY-09-01  | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (completed this year, planned for last year) for academic year (not approved)
	| previousAY-05-01 | currentAY-09-01  | currentAY-09-01  | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (spanning both years and completed this year) for academic year (not approved)
	| previousAY-05-01 | currentAY-09-01  | N/A              | not approved   | the short course learner is returned in the earnings response without duplicates | # Get short course learners (spanning both years and continuing) for academic year (not approved)