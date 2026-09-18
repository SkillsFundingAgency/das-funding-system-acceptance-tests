Feature: LearningType

A short summary of the feature

@ignoreInPREPRODandPP
Scenario: Store the “learning type” from the Courses API - create (apprenticeships)
	When SLD submit a record where the Training code resolves to a learningType of "Apprenticeship" in the Courses API
	Then we have stored the learningType of "Apprenticeship" for that learning

	
@ignoreInPREPRODandPP
Scenario: Store the “learning type” from the Courses API - create (FoundationApprenticeship)
	When SLD submit a record where the Training code resolves to a learningType of "FoundationApprenticeship" in the Courses API
	Then we have stored the learningType of "FoundationApprenticeship" for that learning
