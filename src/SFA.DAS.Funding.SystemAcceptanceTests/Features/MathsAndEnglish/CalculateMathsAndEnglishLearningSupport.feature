Feature: CalculateMathsAndEnglishLearningSupport

As the DfE
I want to know when the details for learning support has changed for an apprentice
So that earnings and payments can be recalculated based on the latest data

@regression
Scenario: Learning support added for Maths and English course
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When a Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount> and learning support from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | course              | amount | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | Entry level English |    931 | currentAY-10-12 | currentAY-02-15 | currentAY-R03            | currentAY-R06           |

@regression
Scenario: Learning support added for Maths and English course - paid until completion
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When an English and Maths learning is recorded from <start_date> to <end_date> with course <course> and amount <amount> and completion date as <completion_date> and learning support from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | course              | amount | completion_date | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | Entry level English |    931 | currentAY-03-20 | currentAY-10-12 | currentAY-04-15 | currentAY-R03            | currentAY-R07           |

