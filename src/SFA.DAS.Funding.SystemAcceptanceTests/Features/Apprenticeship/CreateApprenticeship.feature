Feature: CreateApprenticeship

Test various apprenticeship created behaviours

Scenario: Only inform Approvals of the course with the earliest start date when a learner has multiple onprogramme deliveries
	Given SLD inform us of a learner with with 2 onprogramme deliveries
	Then only inform Approvals of the course with the earliest start date

