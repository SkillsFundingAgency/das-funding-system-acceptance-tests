#Feature: CalculateOnProgrammeLearningSupport
#
#As the DfE
#I want to know when the details for learning support has changed for an apprentice
#So that earnings and payments can be recalculated based on the latest data
#
#@regression
#Scenario: Learning support added for On programme learning
#	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
#	When learning support is recorded from <ls_start_date> to <ls_end_date>
#	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
#	And SLD submit updated learners details
#	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>
#
#Examples:
#	| start_date      | end_date        | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
#	| currentAY-09-25 | currentAY-04-15 | currentAY-11-15 | currentAY-03-10 | currentAY-R04            | currentAY-R07           |
#	| currentAY-08-01 | currentAY-07-31 | currentAY-08-01 | currentAY-12-15 | currentAY-R01            | currentAY-R04           |
#	| currentAY-08-01 | currentAY-07-31 | currentAY-09-01 | currentAY-12-15 | currentAY-R02            | currentAY-R04           |
#	| currentAY-08-01 | currentAY-07-31 | nextAY-09-01    | nextAY-05-15    | nextAY-R02               | nextAY-R09              |
#
#@regression
#Scenario: Learning support removed for On programme learning
#	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
#	When learning support is recorded from <ls_start_date> to <ls_end_date>
#	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
#	And SLD submit updated learners details
#	And learning support is removed
#	And SLD submit updated learners details
#	Then no learning support earnings are generated
#
#Examples:
#	| start_date      | end_date        | ls_start_date   | ls_end_date     |
#	| currentAY-09-25 | currentAY-04-15 | currentAY-11-15 | currentAY-03-10 |
#
#
