Feature: CalculateMathsAndEnglishEarnings

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

---------- LearnAimRef used ----------
60075880 => Amount: 471.00000
60147003 => Amount: £864.00000
60342651 => Amount £941.00000
60342869 => Amount £724.00000


@regression @ignoreInPREPRODandPP
Scenario: Calculate Single Math and English earnings
	Given an apprenticeship has a start date of currentAY-09-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 19
	When a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date nextAY-08-23
	And Maths and English learning is recorded from <start_date> to <end_date> with learnAimRef <learnAimRef>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for learnAimRef <learnAimRef>

Examples:
	| start_date      | end_date        | learnAimRef | expected_first_earning_period | expected_last_period | instalment |
	| currentAY-09-25 | currentAY-01-15 |    60075880 | currentAY-R02                 | currentAY-R05        |     117.75 |
	| currentAY-08-01 | currentAY-07-31 |    60075880 | currentAY-R01                 | currentAY-R12        |      39.25 |
	| currentAY-08-01 | nextAY-07-31    |    60147003 | currentAY-R01                 | nextAY-R12           |         36 |
	| currentAY-08-01 | nextAY-07-31    |    60342651 | currentAY-R01                 | nextAY-R12           |      39.21 |
	| currentAY-02-01 | currentAY-02-26 |    60342869 | currentAY-R07                 | currentAY-R07        |        724 |

@regression @ignoreInPREPRODandPP
Scenario: Calculate Multiple Math and English earnings
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 22
	When a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-23 to date nextAY-08-23
	And Maths and English learning is recorded from <course1_start_date> to <course1_end_date> with learnAimRef <learnAimRef1>
	And Maths and English learning is recorded from <course2_start_date> to <course2_end_date> with learnAimRef <learnAimRef2>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <course1_first_payment_period> to <course1_last_payment_period> with instalment amount <course1_instalment> for learnAimRef <learnAimRef1>
	And Maths and English earnings are generated from periods <course2_first_payment_period> to <course2_last_payment_period> with instalment amount <course2_instalment> for learnAimRef <learnAimRef2>

Examples:
	| course1_start_date | course1_end_date | learnAimRef1 | course2_start_date | course2_end_date | learnAimRef2 | course1_first_payment_period | course1_last_payment_period | course1_instalment | course2_first_payment_period | course2_last_payment_period | course2_instalment |
	| currentAY-09-25    | currentAY-01-15  |     60075880 | currentAY-02-15    | currentAY-05-27  |     60147003 | currentAY-R02                | currentAY-R05               |             117.75 | currentAY-R07                | currentAY-R09               |                288 |
	| currentAY-09-25    | currentAY-01-15  |     60075880 | currentAY-11-15    | currentAY-02-27  |     60147003 | currentAY-R02                | currentAY-R05               |             117.75 | currentAY-R04                | currentAY-R06               |                288 |

@regression @ignoreInPREPRODandPP
Scenario: Learning Support for Maths and English Earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And a Maths and English learning is recorded from <start_date> to <maths_and_english_end_date> with learnAimRef <learnAimRef> and learning support from <start_date> to <maths_and_english_end_date>
	And SLD submit updated learners details
	Then maths and english learning support earnings are generated from periods <expected_first_earning_period> to <expected_last_earning_period>

Examples:
	| start_date      | end_date     | maths_and_english_end_date | learnAimRef | expected_first_earning_period | expected_last_earning_period |
	| currentAY-08-01 | nextAY-01-31 | nextAY-07-31               |    60342651 | currentAY-R01                 | nextAY-R12                   |
	| currentAY-08-01 | nextAY-01-31 | nextAY-07-30               |    60342869 | currentAY-R01                 | nextAY-R11                   |

@regression @ignoreInPREPRODandPP
Scenario: Maths and English instalments removed if maths and english courses removed
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 22
	When a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And Maths and English learning is recorded from <start_date> to <end_date> with learnAimRef <learnAimRef>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for learnAimRef <learnAimRef>
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date <start_date> to date <end_date>
	And the maths and english courses are removed
	And SLD submit updated learners details
	Then no maths and english earnings are generated

Examples:
	| start_date      | end_date        | learnAimRef | expected_first_earning_period | expected_last_period | instalment |
	| currentAY-09-25 | currentAY-01-15 |    60342651 | currentAY-R02                 | currentAY-R05        |     235.25 |


