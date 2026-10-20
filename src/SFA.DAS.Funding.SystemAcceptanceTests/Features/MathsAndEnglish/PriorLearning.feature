Feature: Maths and English Prior Learning

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

SLD can indicate a percentage adjustment to make for prior learning.
M&E Earnings must be multipled by this percentage amount.


---------- LearnAimRef used ----------
60342869 => Amount £724.00000

@regression @ignoreInPREPRODandPP
Scenario: Earnings for Maths and English with prior learning %
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When Math and English learning is recorded from <start_date> to <end_date> with learnAimRef 60342869 and prior learning adjustment of <prior_learning> percent and other funding adjustment of <other_funding_adjustment> percent
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for learnAimRef 60342869

Examples:
	| start_date      | end_date        | agreed_price | expected_first_earning_period | expected_last_period | prior_learning | other_funding_adjustment | instalment |
	| currentAY-09-25 | currentAY-04-15 |         5000 | currentAY-R02                 | currentAY-R08        |             50 |                       50 |      25.86 |
	| currentAY-09-25 | currentAY-04-15 |         5000 | currentAY-R02                 | currentAY-R08        |            120 |                        0 |     124.11 |
	| currentAY-09-25 | currentAY-04-15 |         5000 | currentAY-R02                 | currentAY-R08        |              0 |                      120 |     124.11 |
	| currentAY-09-25 | currentAY-04-15 |         5000 | currentAY-R02                 | currentAY-R08        | null           |                      120 |     124.11 |
	| currentAY-09-25 | currentAY-04-15 |         5000 | currentAY-R02                 | currentAY-R08        | null           | null                     |     103.43 |

	