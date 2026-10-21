Feature: CalculateLearningSupportOnProgrammeAndMathsAndEnglish

As the DfE
I want to pay Learning Support only once per Learning for a given period
Even when it is claimed against both the On programme Learning and Maths & English at the same time

@regression @ignoreInPREPRODandPP
Scenario: Learning support for a Maths & English beyond end of On Programme Learning
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When learning support is recorded from <start_date> to <end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <me_end_date> with learnAimRef 60075880 and learning support from <me_ls_start_date> to <me_end_date>
	And SLD submit updated learners details
	Then combined learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | me_end_date  | me_ls_start_date | expected_first_ls_period | expected_last_ls_period |
	| currentAY-08-01 | currentAY-07-31 | nextAY-10-31 | nextAY-08-01     | currentAY-R01            | nextAY-R03              |

@regression @ignoreInPREPRODandPP
Scenario: Dont pay learning support after on-programme completion
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	When learning support is recorded from currentAY-08-15 to currentAY-07-20
	And Learning Completion is recorded on currentAY-04-15
	And SLD submit updated learners details
	Then learning support earnings are generated from periods currentAY-R01 to currentAY-R8

@regression @ignoreInPREPRODandPP
Scenario: Dont pay learning support after english and maths completion
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	When an English and Maths learning is recorded from currentAY-08-15 to currentAY-07-20 with learnAimRef 60075880, completion date as currentAY-04-10, learning support from currentAY-08-25 to currentAY-07-20
	And SLD submit updated learners details
	Then maths and english learning support earnings are generated from periods currentAY-R01 to currentAY-R8

@regression @ignoreInPREPRODandPP
Scenario: Learning support continues to be paid for English and Maths course after on-prog withdrawal
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And a Maths and English learning is recorded from currentAY-08-15 to currentAY-07-20 with learnAimRef 60075880 and learning support from currentAY-08-15 to currentAY-01-31
	When SLD submit updated learners details
	Then maths and english learning support earnings are generated from periods currentAY-R01 to currentAY-R06

@regression @ignoreInPREPRODandPP
Scenario: Learning support moved from English and Maths to On programme 
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And a Maths and English learning is recorded from currentAY-08-15 to currentAY-07-20 with learnAimRef 60075880 and learning support from currentAY-08-01 to currentAY-01-31
	And SLD submit updated learners details
	And maths and english learning support earnings are generated from periods currentAY-R01 to currentAY-R06
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And learning support is recorded from currentAY-08-01 to currentAY-01-31
	And Learning withdrawal date is recorded on currentAY-11-15
	And Maths and English learning is recorded from currentAY-08-01 to currentAY-07-31 with learnAimRef 60075880
	And SLD submit updated learners details
	Then learning support earnings are generated from periods currentAY-R01 to currentAY-R03

@regression @ignoreInPREPRODandPP
Scenario: Learning support continues to be paid for On-prog after English and Maths is withdrawn
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And learning support is recorded from currentAY-08-01 to currentAY-01-31
	When English and Maths learning is recorded from currentAY-08-01 to currentAY-07-31 with learnAimRef 60075880, withdrawal date currentAY-11-15, learning support from currentAY-08-01 to currentAY-07-31
	And SLD submit updated learners details
	Then learning support continues to be paid from periods currentAY-R01 to currentAY-R06

