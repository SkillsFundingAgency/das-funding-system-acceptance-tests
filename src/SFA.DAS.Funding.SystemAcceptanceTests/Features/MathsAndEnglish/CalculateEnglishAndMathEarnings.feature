Feature: CalculateMathsAndEnglishEarnings

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

@regression
Scenario: Calculate Math and English earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	Then Maths and English earnings are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with instalment amount <instalment>
	And Maths and English payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with amount <instalment>

Examples:
	| start_date       | end_date        | course                           | amount | expected_first_payment_period | expected_last_payment_period | instalment |
	| currentAY-08-25  | currentAY-12-15 | Entry level English and/or Maths | 931    | currentAY-R01                 | currentAY-R04                | 232.75     |
	| currentAY-08-01  | currentAY-07-31 | Entry level English and/or Maths | 12000  | currentAY-R01                 | currentAY-R12                | 1000       |
	| previousAY-08-01 | currentAY-07-31 | Entry level English and/or Maths | 12000  | currentAY-R01                 | currentAY-R12                | 500        |

	@regression
Scenario: Learning Support for Maths and English Earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from <start_date> to <maths_and_english_end_date> with course <course> and amount 12000
	And the payments event is disregarded
	And learning support is recorded from <start_date> to <maths_and_english_end_date>
	Then learning support earnings are generated from periods <expected_first_earning_period> to <expected_last_earning_period>
	And learning support payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period>

Examples:
	| start_date       | end_date        | maths_and_english_end_date | course                           | expected_first_earning_period | expected_last_earning_period | expected_first_payment_period | expected_last_payment_period |
	| previousAY-08-01 | currentAY-01-31 | currentAY-07-31            | Entry level English and/or Maths | previousAY-R01                | currentAY-R12                | currentAY-R01                 | currentAY-R12                |