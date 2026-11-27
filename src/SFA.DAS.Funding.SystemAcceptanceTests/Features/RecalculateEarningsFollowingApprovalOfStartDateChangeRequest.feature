Feature: RecalculateEarningsFollowingApprovalOfStartDateChangeRequest

As a Provider
I want my earnings to reflect approved changes to the start date
So that I am paid the correct amount

** Note that the training code used in the below examples have Funding Band Max value set as below: **
** training_code 1 = 17,000 **
** training_code 2 = 18,000 **

Example 1: Start date moves earlier, stays in-year (normal recalc)
Example 2: Start date moves later, stays in-year (normal recalc)
Example 3: Start date move forwards, into next year 

** Note: start_date must be on or after the Learning opt-in start date, so the learning is paid by DAS and the start date change requires approval **

@regression @ignoreInPREPRODandPP
Scenario: Start Date change approved; recalc earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When SLD record on-programme cost as total price <agreed_price> from date <new_start_date> to date <end_date>
	And Approvals will auto-approve a start date change from <start_date> to <new_start_date> for the learning
	And SLD submit updated learners details (with autoapprove)
	Then the earnings are recalculated based on the new expected earnings <new_expected_earnings>
	And an approved learning updated event is published to approvals
	And the history of old earnings is maintained with <old_inst_amount>
	And the ActualStartDate <new_start_date> and PlannedEndDate <end_date> are updated on earnings entity

Examples:
	| start_date      | end_date               | agreed_price | training_code | new_start_date  | old_inst_amount | new_expected_earnings |
	| currentAY-10-23 | currentAYPlusTwo-10-23 | 15000        | 2             | currentAY-02-23 | 500             | 600                   |
	| currentAY-03-24 | currentAYPlusTwo-03-24 | 18000        | 2             | currentAY-09-24 | 600             | 480                   |
	| currentAY-10-26 | nextAy-06-26           | 15000        | 2             | nextAY-12-26    | 600             | 2000                  |
	| currentAY-11-15 | currentAYPlusTwo-11-15 | 15000        | 2             | currentAY-03-20 | 500             | 600                   |





