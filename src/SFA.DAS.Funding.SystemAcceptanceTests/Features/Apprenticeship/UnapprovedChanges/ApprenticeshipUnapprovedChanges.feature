Feature: ApprenticeshipUnapprovedChanges

As Dfe finance
I want an early view of potential earnings for apprenticeships prior to employer approval
So that I can gauge the possible funding required

#FLP-1937
@regression
@draft-apprenticeships
Scenario: Calculate unappred apprenticeship earnings
	Given SLD inform us of a learner with apprenticeship, english and maths, incentives and learning support having start date currentAY-08-01, expected end date currentAy-07-31, standard code 615 and agreed price 15000
	Then store the apprenticeship, english and maths, incentives and learning support details in learning db in a draft state
