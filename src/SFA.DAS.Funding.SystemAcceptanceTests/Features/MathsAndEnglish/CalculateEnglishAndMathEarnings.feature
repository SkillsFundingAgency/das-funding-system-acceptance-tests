Feature: CalculateMathsAndEnglishEarnings

As the DfE
I want to generate earnings for English and Maths qualifications
So that we know how much to pay providers when they deliver English and/or maths courses

@regression
Scenario: Calculate Single Math and English earnings
	Given an apprenticeship has a start date of currentAY-09-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Payments Generated Events are published
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	Then Maths and English earnings are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with instalment amount <instalment> for course <course>
	And Payments Generated Events are published
	And Maths and English payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with amount <instalment>

Examples:
	| start_date       | end_date        | course              | amount | expected_first_payment_period | expected_last_payment_period | instalment |
	| currentAY-09-25  | currentAY-01-15 | Entry level English | 931    | currentAY-R02                 | currentAY-R05                | 232.75     |
	| currentAY-08-01  | currentAY-07-31 | Entry level Maths   | 12000  | currentAY-R01                 | currentAY-R12                | 1000       |
	| previousAY-08-01 | currentAY-07-31 | GCSE English        | 864    | currentAY-R01                 | currentAY-R12                | 36         |
	| currentAY-08-01  | nextAY-07-31    | GCSE Maths          | 864    | currentAY-R01                 | nextAY-R12                   | 36         |
	| currentAY-02-01  | currentAY-02-26 | Level 2 English     | 724    | currentAY-R07                 | currentAY-R07                | 724        |

@regression
Scenario: Calculate Multiple Math and English earnings
	Given an apprenticeship has a start date of currentAY-08-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 22
	When the apprenticeship commitment is approved
	And Payments Generated Events are published
	And the first course is recorded from <course1_start_date> to <course1_end_date> with course <course1_name> and amount <course1_amount> and the second course from <course2_start_date> to <course2_end_date> with course <course2_name> and amount <course2_amount>
	Then Maths and English earnings are generated from periods <course1_first_payment_period> to <course1_last_payment_period> with instalment amount <course1_instalment> for course <course1_name>
	And Maths and English earnings are generated from periods <course2_first_payment_period> to <course2_last_payment_period> with instalment amount <course2_instalment> for course <course2_name>
	And Payments Generated Events are published
	And Maths and English payments are generated from periods <course1_first_payment_period> to <course1_last_payment_period> with amount <course1_instalment>
	And Maths and English payments are generated from periods <course2_first_payment_period> to <course2_last_payment_period> with amount <course2_instalment>

Examples:
	| course1_start_date | course1_end_date | course1_name        | course1_amount | course2_start_date | course2_end_date | course2_name | course2_amount | course1_first_payment_period | course1_last_payment_period | course1_instalment | course2_first_payment_period | course2_last_payment_period | course2_instalment |
	| currentAY-09-25    | currentAY-01-15  | Entry level English | 931            | currentAY-02-15    | currentAY-05-27  | GCSE Maths   | 864            | currentAY-R02                | currentAY-R05               | 232.75             | currentAY-R07                | currentAY-R09               | 288                |
	| currentAY-09-25    | currentAY-01-15  | Entry level English | 931            | currentAY-11-15    | currentAY-02-27  | GCSE Maths   | 864            | currentAY-R02                | currentAY-R05               | 232.75             | currentAY-R04                | currentAY-R06               | 288                |


@regression
Scenario: Do Not calculate Maths and English payments for a hard closed academic year
	Given an apprenticeship has a start date of TwoYearsAgoAY-08-01, a planned end date of CurrentAY-07-31, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Payments Generated Events are published
	And Maths and English learning is recorded from <start_date> to <end_date> with course <course> and amount <amount>
	Then Maths and English earnings are generated from periods <expected_first_payment_period> to <expected_last_payment_period> with instalment amount <instalment> for course <course>
	And Payments Generated Events are published
	And no Maths and English payments are generated

Examples:
	| start_date          | end_date            | course              | amount | expected_first_payment_period | expected_last_payment_period | instalment |
	| TwoYearsAgoAY-09-25 | TwoYearsAgoAY-01-15 | Entry level English | 931    | TwoYearsAgoAY-R02             | TwoYearsAgoAY-R05            | 232.75     |

@regression
Scenario: Learning Support for Maths and English Earnings
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And Maths and English learning is recorded from <start_date> to <maths_and_english_end_date> with course <course> and amount 12000
	And the payments event is disregarded
	And learning support is recorded from <start_date> to <maths_and_english_end_date>
	Then learning support earnings are generated from periods <expected_first_earning_period> to <expected_last_earning_period>
	And Payments Generated Events are published
	And learning support payments are generated from periods <expected_first_payment_period> to <expected_last_payment_period>

Examples:
	| start_date       | end_date        | maths_and_english_end_date | course                           | expected_first_earning_period | expected_last_earning_period | expected_first_payment_period | expected_last_payment_period |
	| previousAY-08-01 | currentAY-01-31 | currentAY-07-31            | Entry level English and/or Maths | previousAY-R01                | currentAY-R12                | currentAY-R01                 | currentAY-R12                |