Feature: CreateApprenticeship

Test various apprenticeship created behaviours

Scenario: Only inform Approvals of the course with the earliest start date when a learner has multiple onprogramme deliveries
	Given SLD inform us of a learner with with 2 onprogramme deliveries
	Then only inform Approvals of the course with the earliest start date


Scenario: Learner with approved apprenticeship history does not have unapproved earnings calculated
    Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 241
    And the apprenticeship commitment is approved
    When an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 615
	Then no earnings are calculated for the apprenticeship with training code 615
