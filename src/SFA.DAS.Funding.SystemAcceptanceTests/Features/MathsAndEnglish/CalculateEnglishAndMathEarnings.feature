Feature: CalculateMathsAndEnglishEarnings

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

@regression
Scenario: Calculate Single Math and English earnings
	Given an apprenticeship has a start date of currentAY-09-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date nextAY-08-23
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for course <course>

Examples:
	| start_date       | end_date        | course              | amount | expected_first_earning_period | expected_first_payment_period | expected_last_period | instalment |
	| currentAY-09-25  | currentAY-01-15 | Entry level English |    931 | currentAY-R02                 | currentAY-R02                 | currentAY-R05        |     232.75 |
	| currentAY-08-01  | currentAY-07-31 | Entry level Maths   |  12000 | currentAY-R01                 | currentAY-R01                 | currentAY-R12        |       1000 |
	| previousAY-08-01 | currentAY-07-31 | GCSE English        |    864 | previousAY-R01                | currentAY-R01                 | currentAY-R12        |         36 |
	| currentAY-08-01  | nextAY-07-31    | GCSE Maths          |    864 | currentAY-R01                 | currentAY-R01                 | nextAY-R12           |         36 |
	| currentAY-02-01  | currentAY-02-26 | Level 2 English     |    724 | currentAY-R07                 | currentAY-R07                 | currentAY-R07        |        724 |

@regression
Scenario: Calculate Multiple Math and English earnings
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 22
	When the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-23 to date nextAY-08-23
	And Maths and English learning is recorded from <course1_start_date> to <course1_end_date> with course <course1_name> and amount <course1_amount>
	And Maths and English learning is recorded from <course2_start_date> to <course2_end_date> with course <course2_name> and amount <course2_amount>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <course1_first_payment_period> to <course1_last_payment_period> with instalment amount <course1_instalment> for course <course1_name>
	And Maths and English earnings are generated from periods <course2_first_payment_period> to <course2_last_payment_period> with instalment amount <course2_instalment> for course <course2_name>

Examples:
	| course1_start_date | course1_end_date | course1_name        | course1_amount | course2_start_date | course2_end_date | course2_name | course2_amount | course1_first_payment_period | course1_last_payment_period | course1_instalment | course2_first_payment_period | course2_last_payment_period | course2_instalment |
	| currentAY-09-25    | currentAY-01-15  | Entry level English |            931 | currentAY-02-15    | currentAY-05-27  | GCSE Maths   |            864 | currentAY-R02                | currentAY-R05               |             232.75 | currentAY-R07                | currentAY-R09               |                288 |
	| currentAY-09-25    | currentAY-01-15  | Entry level English |            931 | currentAY-11-15    | currentAY-02-27  | GCSE Maths   |            864 | currentAY-R02                | currentAY-R05               |             232.75 | currentAY-R04                | currentAY-R06               |                288 |

@regression
Scenario: Learning Support for Maths and English Earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <maths_and_english_end_date> with course <course>, amount 12000, learning support from <start_date> to <maths_and_english_end_date>
	And SLD submit updated learners details
	Then learning support earnings are generated from periods <expected_first_earning_period> to <expected_last_earning_period>

Examples:
	| start_date       | end_date        | maths_and_english_end_date | course                           | expected_first_earning_period | expected_last_earning_period | 
	| previousAY-08-01 | currentAY-01-31 | currentAY-07-31            | Entry level English and/or Maths | previousAY-R01                | currentAY-R12                | 
	| previousAY-08-01 | currentAY-01-31 | currentAY-07-30            | Entry level English and/or Maths | previousAY-R01                | currentAY-R11                | 

@regression
Scenario: Maths and English instalments removed if maths and english courses removed
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 22
	When the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for course <course>
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And the maths and english courses are removed
	And SLD submit updated learners details
	Then no maths and english earnings are generated

Examples:
	| start_date      | end_date        | course              | amount | expected_first_earning_period | expected_first_payment_period | expected_last_period | instalment |
	| currentAY-09-25 | currentAY-01-15 | Entry level English |    931 | currentAY-R02                 | currentAY-R02                 | currentAY-R05        |     232.75 |


