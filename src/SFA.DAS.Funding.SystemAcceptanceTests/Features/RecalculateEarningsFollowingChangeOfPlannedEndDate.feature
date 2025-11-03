#Feature: RecalculateEarningsFollowingChangeOfPlannedEndDate
#
#As the dfe
#I want to know when the learning planned date has changed for an apprenticeship
#So that earnings and [payments can be recalculated based on the latest data 
#
#
#Example 1: Planned End date moves earlier
#Example 2: Planned End date moves later
#
#@regression
#Scenario: Planned End Date change; recalc earnings
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	And the apprenticeship commitment is approved
#	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <new_end_date>
#	And SLD submit updated learners details
#	Then the earnings are recalculated based on the new expected earnings <new_expected_earnings>
#	And the history of old earnings is maintained with <old_inst_amount>
#	And the ActualStartDate <start_date> and PlannedEndDate <new_end_date> are updated on earnings entity
#	And old and new earnings maintain their initial Profile Id
#	And an end date changed event is published to approvals with end date <new_end_date>
#
#Examples:
#	| start_date      | end_date               | agreed_price | training_code | new_end_date           | old_inst_amount | new_expected_earnings |
#	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | nextAY-05-23           | 500             | 571.42857             |
#	| currentAY-08-26 | nextAy-04-26           | 15000        | 2             | currentAYPlusTwo-08-26 | 600             | 500                   |
