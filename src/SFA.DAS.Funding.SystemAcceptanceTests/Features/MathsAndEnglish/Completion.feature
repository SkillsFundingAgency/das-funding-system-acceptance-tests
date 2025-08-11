Feature: Maths and English Completion

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

When M&E is marked as "Complete", earnings for subsequent delivery periods are "rolled up" into a single Balancing earning
	
@regression
Scenario: Balancing earnings for Maths and English on Completion
	Given a learning has a start date of <start_date>, a planned end date of <end_date> and an agreed price of 12000
	When Maths and English learning is recorded from <start_date> to <end_date> with course <course>, amount <amount> and completion on <completion_date>
	Then Maths and English earnings are generated from periods <expected_first_earning_period> to <expected_last_period> with regular instalment amount <instalment> for course <course>
	And a Maths and English earning of <balancing_amount> is generated for course <course> for period <balancing_period>

Examples:
	| start_date      | end_date        | course              | completion_date | amount | expected_first_earning_period | expected_last_period | instalment | balancing_amount | balancing_period |
	| currentAY-09-25 | currentAY-04-15 | Entry level English | currentAY-01-01 | 931    | currentAY-R02                 | currentAY-R05        | 133        | 399              | currentAY-R06    |


