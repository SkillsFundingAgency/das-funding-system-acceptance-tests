Feature: CalculateOnProgrammeLearningSupport

As the DfE
I want to know when the details for learning support has changed for an apprentice
So that earnings and payments can be recalculated based on the latest data

@regression
Scenario: Learning support added for On programme learning
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When SLD inform us of learning support request from <ls_start_date> to <ls_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | currentAY-11-15 | currentAY-03-10 | currentAY-R04            | currentAY-R07           |
