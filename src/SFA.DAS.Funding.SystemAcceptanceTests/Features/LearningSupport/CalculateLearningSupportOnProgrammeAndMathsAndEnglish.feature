Feature: CalculateLearningSupportOnProgrammeAndMathsAndEnglish

As the DfE
I want to pay Learning Support only once per Learning for a given period
Even when it is claimed against both the On programme Learning and Maths & English at the same time

@regression
Scenario: Learning support not duplicated when claimed against On programme learning and Maths & English at the same time
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <end_date> with learnAimRef 60342844, course Maths, amount 1000, learning support from <ls_start_date> to <ls_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| nextAY-09-25 | nextAY-04-15 | nextAY-11-15 | nextAY-03-10 | nextAY-R04            | nextAY-R07           |


@regression
Scenario: Learning support for a Maths & English beyond end of On Programme Learning
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When learning support is recorded from <start_date> to <end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <me_end_date> with learnAimRef 60342844, course Maths, amount 1000, learning support from <me_ls_start_date> to <me_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | me_end_date  | me_ls_start_date | expected_first_ls_period | expected_last_ls_period |
	| currentAY-08-01 | currentAY-07-31 | nextAY-10-31 | nextAY-08-01     | currentAY-R01            | nextAY-R03              |

@regression
Scenario: Learning support across multiple Maths & English courses with overlap
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When a Maths and English learning is recorded from <maths_start_date> to <maths_end_date> with learnAimRef 60342844, course Maths, amount 1000, learning support from <maths_start_date> to <maths_end_date>
	And a Maths and English learning is recorded from <english_start_date> to <english_end_date> with learnAimRef 60342843, course English, amount 2000, learning support from <english_start_date> to <english_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | maths_start_date | maths_end_date  | english_start_date | english_end_date | expected_first_ls_period | expected_last_ls_period |
	| nextAY-08-01 | nextAY-07-31 | nextAY-08-01  | nextAY-12-31 | nextAY-12-01    | nextAY-07-31  | nextAY-R01            | nextAY-R12           |


@regression
Scenario: Dont pay learning support after on-programme completion
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	When learning support is recorded from nextAY-08-15 to nextAY-07-20
	And Learning Completion is recorded on nextAY-04-15
	And SLD submit updated learners details
	Then learning support earnings are generated from periods nextAY-R01 to nextAY-R8

@regression
Scenario: Dont pay learning support after english and maths completion
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	When an English and Maths learning is recorded from nextAY-08-15 to nextAY-07-20 with learnAimRef 60342844, course Maths, amount 1000, completion date as nextAY-04-10, learning support from nextAY-08-25 to nextAY-07-20
	And SLD submit updated learners details
	Then learning support earnings are generated from periods nextAY-R01 to nextAY-R8

@regression
Scenario: Learning support continues to be paid for English and Maths course after on-prog withdrawal
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And a Maths and English learning is recorded from currentAY-08-15 to currentAY-07-20 with learnAimRef 60342844, course Maths, amount 1000, learning support from currentAY-08-15 to currentAY-01-31
	When SLD submit updated learners details
	Then learning support continues to be paid from periods currentAY-R01 to currentAY-R06

@regression
Scenario: Learning support moved from English and Maths to On programme 
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And a Maths and English learning is recorded from currentAY-08-15 to currentAY-07-20 with learnAimRef 60342844, course Maths, amount 1000, learning support from currentAY-08-01 to currentAY-01-31
	And SLD submit updated learners details
	And learning support continues to be paid from periods currentAY-R01 to currentAY-R06
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And learning support is recorded from currentAY-08-01 to currentAY-01-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And Maths and English learning is recorded from currentAY-08-01 to currentAY-07-31 with learnAimRef 6034284, course Maths Foundation and amount 931
	And SLD submit updated learners details
	Then learning support earnings are generated from periods currentAY-R01 to currentAY-R03

@regression
Scenario: Learning support continues to be paid for On-prog after English and Maths is withdrawn
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And learning support is recorded from currentAY-08-01 to currentAY-01-31
	When English and Maths learning is recorded from currentAY-08-01 to currentAY-07-31 with learnAimRef 60342843, course English Foundation, amount 2000, withdrawal date currentAY-11-15, learning support from currentAY-08-01 to currentAY-07-31
	And SLD submit updated learners details
	Then learning support continues to be paid from periods currentAY-R01 to currentAY-R06

