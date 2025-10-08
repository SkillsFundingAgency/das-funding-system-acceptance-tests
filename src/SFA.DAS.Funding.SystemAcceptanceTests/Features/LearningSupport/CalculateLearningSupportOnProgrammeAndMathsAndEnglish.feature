Feature: CalculateLearningSupportOnProgrammeAndMathsAndEnglish

As the DfE
I want to pay Learning Support only once per Learning for a given period
Even when it is claimed against both the On programme Learning and Maths & English at the same time

@regression
Scenario: Learning support not duplicated when claimed against On programme learning and Maths & English at the same time
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <end_date> with course Maths and amount 1000 and learning support from <ls_start_date> to <ls_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | currentAY-11-15 | currentAY-03-10 | currentAY-R04            | currentAY-R07           |


@regression
Scenario: Learning support for a Maths & English beyond end of On Programme Learning
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <start_date> to <end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <me_end_date> with course Maths and amount 1000 and learning support from <me_ls_start_date> to <me_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | me_end_date  | me_ls_start_date | expected_first_ls_period | expected_last_ls_period |
	| currentAY-08-01 | currentAY-07-31 | nextAY-10-31 | nextAY-08-01     | currentAY-R01            | nextAY-R03              |

@regression
Scenario: Learning support across multiple Maths & English courses with overlap
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When a Maths and English learning is recorded from <maths_start_date> to <maths_end_date> with course Maths and amount 1000 and learning support from <maths_start_date> to <maths_end_date>
	And a Maths and English learning is recorded from <english_start_date> to <english_end_date> with course English and amount 2000 and learning support from <english_start_date> to <english_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | maths_start_date | maths_end_date  | english_start_date | english_end_date | expected_first_ls_period | expected_last_ls_period |
	| currentAY-08-01 | currentAY-07-31 | currentAY-08-01  | currentAY-12-31 | currentAY-12-01    | currentAY-07-31  | currentAY-R01            | currentAY-R12           |


@regression
Scenario: Dont pay learning support after on-programme completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	When learning support is recorded from currentAY-08-15 to currentAY-07-20
	And Learning Completion is recorded on currentAY-04-15
	And SLD submit updated learners details
	Then learning support earnings are generated from periods currentAY-R01 to currentAY-R8

@regression
Scenario: Dont pay learning support after english and maths completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	When an English and Maths learning is recorded from currentAY-08-15 to currentAY-07-20 with course Maths and amount 1000 and completion date as currentAY-04-10 and learning support from currentAY-08-25 to currentAY-07-20
	And SLD submit updated learners details
	Then learning support earnings are generated from periods currentAY-R01 to currentAY-R8

