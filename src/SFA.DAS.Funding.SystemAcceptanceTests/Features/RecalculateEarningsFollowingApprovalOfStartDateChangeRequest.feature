#Feature: RecalculateEarningsFollowingApprovalOfStartDateChangeRequest
#
#As a Provider
#I want my earnings to reflect approved changes to the start date
#So that I am paid the correct amount
#
#** Note that the training code used in the below examples have Funding Band Max value set as below: **
#** training_code 1 = 17,000 **
#** training_code 2 = 18,000 **
#
#Example 1: Start date moves earlier, stays in-year (normal recalc)
#Example 2: Start date moves later, stays in-year (normal recalc)
#Example 3: Start date move forwards, into next year 
#
#@regression
#Scenario: Start Date change approved; recalc earnings
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	And the apprenticeship commitment is approved
#	When SLD record on-programme cost as total price <agreed_price> from date <new_start_date> to date <end_date>
#	And SLD submit updated learners details
#	Then the earnings are recalculated based on the new expected earnings <new_expected_earnings>
#	And the history of old earnings is maintained with <old_inst_amount>
#	And the ActualStartDate <new_start_date> and PlannedEndDate <end_date> are updated on earnings entity
#	And old and new earnings maintain their initial Profile Id
#
#Examples:
#	| start_date      | end_date               | agreed_price | training_code | new_start_date  | old_inst_amount | new_expected_earnings |
#	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | currentAY-12-23 | 500             | 600                   |
#	| currentAY-03-24 | currentAYPlusTwo-03-24 | 18000        | 2             | currentAY-09-24 | 600             | 480                   |
#	| currentAY-08-26 | nextAy-04-26           | 15000        | 2             | nextAY-10-26    | 600             | 2000                  |
#	| currentAY-09-15 | currentAYPlusTwo-09-15 | 15000        | 2             | currentAY-01-20 | 500             | 600                   |
#
#
#
#
#
