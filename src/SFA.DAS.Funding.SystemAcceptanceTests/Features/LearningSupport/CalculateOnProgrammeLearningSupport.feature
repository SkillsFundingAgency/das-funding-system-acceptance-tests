Feature: CalculateOnProgrammeLearningSupport

As the DfE
I want to know when the details for learning support has changed for an apprentice
So that earnings and payments can be recalculated based on the latest data

@regression
Scenario: Learning support added for On programme learning
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date       | end_date        | ls_start_date    | ls_end_date      | expected_first_ls_period | expected_last_ls_period |
	| nextAY-09-25  | nextAY-04-15 | nextAY-11-15  | nextAY-03-10  | nextAY-R04            | nextAY-R07           |
	| nextAY-08-01  | nextAY-07-31 | nextAY-08-01  | nextAY-12-15  | nextAY-R01            | nextAY-R04           |
	| nextAY-08-01  | nextAY-07-31 | nextAY-09-01  | nextAY-12-15  | nextAY-R02            | nextAY-R04           |
	| currentAY-08-01 | nextAY-07-31 | currentAY-09-01 | currentAY-05-15 | currentAY-R02           | currentAY-R09          |

@regression
Scenario: Learning support removed for On programme learning
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	And learning support is removed
	And SLD submit updated learners details
	Then no learning support earnings are generated

Examples:
	| start_date      | end_date        | ls_start_date   | ls_end_date     |
	| nextAY-09-25 | nextAY-04-15 | nextAY-11-15 | nextAY-03-10 |

@regression
Scenario: No LSF earnings after learner withdraws from the programme aim
	Given a learning has a start date of nextAY-09-25, a planned end date of nextAY-04-15 and an agreed price of 12000
	When learning support is recorded from nextAY-11-15 to nextAY-03-10
	And SLD record on-programme cost as total price 12000 from date nextAY-09-25 to date nextAY-04-15
	And Learning withdrawal date is recorded on nextAY-01-15
	And SLD submit updated learners details
	Then learning support earnings are generated from periods nextAY-R04 to nextAY-R05


