Feature: Maths and English Withdrawal

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

Earnings for English and Maths when the learner has withdrawn depends upon whether they have been in learning for a sufficient period, as denoted by the Qualifying Period. 
The Qualifying Period varies depending upon the length of the course:
	42-day qualifying period for 168 days or more
	14-day qualifying period for 14-167 days
	1-day qualifying period for <14 days

@regression
Scenario: Earnings for Maths and English after Withdrawal after Qualifying Period
	Given a learning has a start date of <start_date>, a duration of <duration_days> and an agreed price of <agreed_price>
	When Maths and English learning is recorded from <start_date> for <duration_days> days with learnAimRef 60342843, course <course>, amount <agreed_price> and withdrawal after <withdrawal_on_day> days
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> with duration <duration_days>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for course <course>

Examples:
	| start_date      | duration_days | course              | agreed_price | withdrawal_on_day | expected_first_earning_period | expected_last_period | instalment |
	| currentAY-09-25 |           240 | Entry level English |         5000 |                42 | currentAY-R02                 | currentAY-R03        |        625 |
	| currentAY-09-25 |           180 | Entry level English |         5000 |                42 | currentAY-R02                 | currentAY-R03        |     833.33 |
	| currentAY-09-25 |            14 | Entry level English |         5000 |                14 | currentAY-R02                 | currentAY-R02        |       5000 |
	#| currentAY-09-25 |            13 | Entry level English |         5000 |                 1 | currentAY-R02                 | currentAY-R02        |       5000 | -- Uncomment this example as part of FLP-1424 


@regression
Scenario: Withdrawal for Maths and English can be after Planned end date
	Given a learning has a start date of <start_date>, a duration of <duration_days> and an agreed price of <agreed_price>
	When Maths and English learning is recorded from <start_date> for <duration_days> days with learnAimRef 60342843, course <course>, amount <agreed_price> and withdrawal after <withdrawal_on_day> days
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> with duration <duration_days>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for course <course>

Examples:
	| start_date      | duration_days | course              | agreed_price | withdrawal_on_day | expected_first_earning_period | expected_last_period | instalment |
	| currentAY-09-25 |           240 | Entry level English |         5000 |               270 | currentAY-R02                 | currentAY-R09        |        625 |
	

@regression
Scenario: Earnings for Maths and English are recalculated when withdrawal details have changed
	Given a learning has a start date of <start_date>, a duration of <duration_days> and an agreed price of <agreed_price>
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> with duration <duration_days>
	And Maths and English learning is recorded from <start_date> for <duration_days> days with learnAimRef 60342843, course <course>, amount <agreed_price> and withdrawal after <first_withdrawal_on_day> days
	And SLD submit updated learners details
	And Maths and English earnings are generated from periods <first_withdrawal_earnings_start_period> to <first_withdrawal_earnings_end_period> with instalment amount <instalment> for course <course>
	When SLD inform us that Maths and English details have changed
	And Maths and English learning is recorded from <start_date> for <duration_days> days with learnAimRef 60342843, course <course>, amount <agreed_price> and withdrawal after <second_withdrawal_on_day> days
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> with duration <duration_days>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <second_withdrawal_earnings_start_period> to <second_withdrawal_earnings_end_period> with instalment amount <instalment> for course <course>

Examples:
	| start_date      | duration_days | course              | agreed_price | first_withdrawal_on_day | first_withdrawal_earnings_start_period | first_withdrawal_earnings_end_period | instalment | second_withdrawal_on_day | second_withdrawal_earnings_start_period | second_withdrawal_earnings_end_period |
	| currentAY-09-25 |           240 | Entry level English |         5000 |                      42 | currentAY-R02                          | currentAY-R03                        |        625 |                       70 | currentAY-R02                           | currentAY-R04                         |

@regression
Scenario: Earnings for Maths and English after Withdrawal during Qualifying Period
	Given a learning has a start date of <start_date>, a duration of <duration_days> and an agreed price of <agreed_price>
	When Maths and English learning is recorded from <start_date> for <duration_days> days with learnAimRef 60342843, course <course>, amount <agreed_price> and withdrawal after <withdrawal_on_day> days
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> with duration <duration_days>
	And SLD submit updated learners details
	Then Maths and English earnings for course <course> are removed

Examples:
	| start_date      | duration_days | course              | agreed_price | withdrawal_on_day |
	| currentAY-09-25 |           180 | Entry level English |         5000 |                41 |
	| currentAY-09-25 |            14 | Entry level English |         5000 |                13 |


@regression
Scenario: English and Maths course withdrawn from the start
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 to currentAY-01-07 with learnAimRef 60342843, course Entry level English and amount 931
	And SLD submit updated learners details
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 for 156 days with learnAimRef 60342843, course Entry level English, amount 931 and withdrawal after 1 days
	And SLD submit updated learners details
	Then Maths and English earnings for course Entry level English are removed

@regression
Scenario: English and Maths course withdrawn from the start then removed
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 to currentAY-01-07 with learnAimRef 60342843, course Entry level English and amount 1000
	And SLD submit updated learners details
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 for 156 days with learnAimRef 60342843, course Entry level English, amount 1000 and withdrawal after 1 days
	And SLD submit updated learners details
	Then Maths and English earnings for course Entry level English are removed
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And the maths and english courses are removed
	And SLD submit updated learners details
	Then Maths and English earnings for course Entry level English are removed

@regression
Scenario: English and Maths course withdrawn from the start then reinstated
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 to currentAY-01-07 with learnAimRef 60342843, course Entry level English and amount 1000
	And SLD submit updated learners details
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 for 156 days with learnAimRef 60342843, course Entry level English, amount 1000 and withdrawal after 1 days
	And SLD submit updated learners details
	Then Maths and English earnings for course Entry level English are removed
	When SLD resubmits ILR
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date nextAY-08-23
	And Maths and English learning is recorded from currentAY-08-05 to currentAY-01-07 with learnAimRef 60342843, course Entry level English and amount 1000
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods currentAY-R01 to currentAY-R05 with instalment amount 200 for course Entry level English
