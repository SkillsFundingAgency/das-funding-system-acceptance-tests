Feature: LearningSupportPerformace

The below test calls the Maths and English earnings inner endpoint for a duration of 5 years. 
Then it calls the Learning Support endpoint for the same duration a number of times.
With this test we want to ensure that the call of Learning Support inner endpoint does not timeout.

@regression @Performance
Scenario: Learning Support for Maths and English Earnings over 5 years 
	Given an apprenticeship has a start date of 2022-08-01, a planned end date of 2026-07-31, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from 2022-08-15 to 2026-07-31 with course Entry level English and amount 12000
	And the payments event is disregarded
	And learning support is recorded from 2022-08-01 to 2026-07-31
	And the payments event is disregarded
	And learning support is recorded from 2022-08-01 to 2026-07-31
	And the payments event is disregarded
	And learning support is recorded from 2022-08-01 to 2026-07-31
	And the payments event is disregarded
