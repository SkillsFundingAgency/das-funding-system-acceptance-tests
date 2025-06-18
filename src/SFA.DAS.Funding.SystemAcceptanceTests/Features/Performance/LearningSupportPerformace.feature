Feature: LearningSupportPerformace

The below test calls the Maths and English earnings inner endpoint for a duration of 5 years. 
Then it calls the Learning Support endpoint for the same duration a number of times.
With this test we want to ensure that the call of Learning Support inner endpoint does not timeout.

@regression @Performance
Scenario: Learning Support for Maths and English Earnings over 5 years
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from <start_date> to <end_date> with course course and amount 12000
	And the payments event is disregarded
	And learning support is recorded from 2023-08-01 to <end_date>
	And the payments event is disregarded
	And learning support is recorded from 2023-08-01 to <end_date>
	And the payments event is disregarded
	And learning support is recorded from <start_date> to <end_date>
	Then learning support earnings are generated from periods <expected_first_earning_period> to <expected_last_earning_period>
	And Payments Generated Events are published
	And learning support payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period>
Examples:
	| start_date          | end_date     | course              | expected_first_earning_period | expected_last_earning_period | expected_first_payment_period | expected_last_payment_period |
	| TwoYearsAgoAY-08-01 | nextAY-07-31 | Entry level English | TwoYearsAgoAY-R01             | nextAY-R12                   | currentAY-R01                 | nextAY-R12                   |
	

