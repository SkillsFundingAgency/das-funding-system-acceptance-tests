Feature: CalculateMathsAndEnglishEarnings

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

@regression
Scenario: Calculate Math and English earnings
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	Then Maths and English earnings are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with instalment amount <instalment>


Examples:
	| start_date      | end_date        | course                           | amount | expected_first_payment_period | expected_last_payment_period | instalment |
	| currentAY-08-25 | currentAY-12-15 | Entry level English and/or Maths | 931    | currentAY-R01                 | currentAY-R04                | 232.75     |