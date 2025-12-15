Feature: UpdateIncentivesDueDateAfterReturnFromBreakInLearning

As a DfE
I want to ensure that incentive days are recalculated when a training provider records an apprentices return from a break in learning
So that incentive earnings are not paid early (or late)

Background:
	Given an apprenticeship has a start date of 2023-10-01, a planned end date of 2025-07-31, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 18
	And the apprenticeship commitment is approved
	Then the first incentive due date for provider & employer is 2023-12-29
	And the second incentive due date for provider & employer is 2024-09-29


@regression
Scenario: Learner goes on break 1 day before 90th day incentive and returns after 30 day break
	Given SLD record on-programme cost as total price 15000 from date 2023-10-01 to date 2025-07-31
	And SLD inform us of a break in learning with pause date 2023-12-28
	And SLD submit updated learners details
	And the first incentive earning is not generated for provider & employer
	And the second incentive earning is not generated for provider & employer
	When SLD inform us of a return from break in learning with a new learning start date 2024-01-28
	And SLD submit updated learners details
	Then earnings are recalculated
	And the first incentive due date for provider & employer is 2024-01-28
	And the second incentive due date for provider & employer is 2024-10-29

