Feature: ApprenticeshipUnapprovedChanges

As Dfe finance
I want an early view of potential earnings for apprenticeships prior to employer approval
So that I can gauge the possible funding required

#FLP-1937
@regression
@draft-apprenticeships
@ignoreInPREPRODandPP
Scenario: Calculate unapproved apprenticeship earnings
	Given SLD inform us of a learner with apprenticeship, english and maths, incentives and learning support having start date currentAY-08-01, expected end date currentAY-07-31, standard code 615 and agreed price 15000
	Then store the apprenticeship, english and maths and learning support details in learning db in a draft state
	And calculate 12 unapproved earnings for programme aim with amount 1000 
	And Maths and English earnings are generated from periods currentAY-R01 to currentAY-R12 with regular instalment amount 83.33 for course English Foundation
	And the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
	And learning support earnings are generated from periods currentAY-R01 to currentAY-R12
	And learner is not returned from get learners endpoint

@ignoreInPREPRODandPP
Scenario: Prevent duplication of “unapproved apprenticeship earnings”
	Given SLD inform us of a learner with apprenticeship, english and maths, incentives and learning support having start date currentAY-08-01, expected end date currentAY-07-31, standard code 615 and agreed price 15000
	And an earning profile is created
	When SLD inform us that the training provider has resubmitted the same learner
	Then the earning profile has not changed
	And calculate 12 unapproved earnings for programme aim with amount 1000 

@ignoreInPREPRODandPP
Scenario: Prevent duplication of “unapproved apprenticeship earnings” when learning changes
	Given SLD inform us of a learner with apprenticeship, english and maths, incentives and learning support having start date currentAY-08-01, expected end date currentAY-07-31, standard code 615 and agreed price 15000
	And an earning profile is created
	When SLD inform us that the training provider has resubmitted the same learner with price change
	Then the earning profile has not changed
	And calculate 12 unapproved earnings for programme aim with amount 500 