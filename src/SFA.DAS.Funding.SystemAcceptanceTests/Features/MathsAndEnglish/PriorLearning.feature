Feature: Maths and English Prior Learning

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

SLD can indicate a percentage adjustment to make for prior learning.
M&E Earnings must be multipled by this percentage amount.

@regression
Scenario: Earnings for Maths and English with prior learning %
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of <agreed_price>
	When Maths and English learning is recorded from <start_date> to <end_date> with course <course>, amount <amount> and prior learning adjustment of <prior_learning> percent
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with instalment amount <instalment> for course <course>

Examples:
	| start_date      | end_date        | course              | agreed_price | completion_date | amount | expected_first_earning_period | expected_last_period | prior_learning | instalment |
	| currentAY-09-25 | currentAY-04-15 | Entry level English | 5000         | currentAY-01-01 | 931    | currentAY-R02                 | currentAY-R09        | 100            | 133        |
	| currentAY-09-25 | currentAY-04-15 | Entry level English | 5000         | currentAY-01-01 | 931    | currentAY-R02                 | currentAY-R09        | 0              | 133        |
	| currentAY-09-25 | currentAY-04-15 | Entry level English | 5000         | currentAY-01-01 | 931    | currentAY-R02                 | currentAY-R09        | 10             | 13.3       |
	| currentAY-09-25 | currentAY-04-15 | Entry level English | 5000         | currentAY-01-01 | 931    | currentAY-R02                 | currentAY-R09        | 110            | 146.3      |

	