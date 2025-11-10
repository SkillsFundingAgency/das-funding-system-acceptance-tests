Feature: Calculate incentives for 16-18 learners

As a Training provider & Employer
I want monthly 16-18 incentive earnings  to be calculated
So we both get paid incentives correctly

# User stories/ACs covered in this file:
# FLP-1034 AC1,AC2
# FLP-1036 AC1(16-18 part)

@regression
Scenario: 16-18 Incentive Earnings 
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 16  |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 17  |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 18  |


@regression
Scenario: 16-18 Incentive Earnings (duration only long enough for first earning only)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is not generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-10-29  | 15,000       | 614           | 17  |
	| currentAY-08-01 | currentAY-07-30  | 15,000       | 614           | 17  |


@regression
Scenario: 16-18 Incentive Earnings (duration too short)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is not generated for provider & employer
	And the second incentive earning is not generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-10-28  | 15,000       | 614           | 17  |


@regression
Scenario: No Incentives for 16-18 learner completing before threshold date
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	And SLD record on-programme training price 12000 with epao as 3000 from date currentAY-08-01 to date currentAY-07-31
	And Learning Completion is recorded on <completion_date>
	And SLD submit updated learners details
	Then the first incentive earning <first_earnings_generated> generated for provider & employer
	And the second incentive earning <second_earnings_generated> generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age | completion_date | first_earnings_generated | second_earnings_generated |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  17 | currentAY-10-28 | is not                   | is not                    |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  18 | currentAY-10-29 | is                       | is not                    |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  17 | currentAY-07-29 | is                       | is not                    |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  18 | currentAY-07-31 | is                       | is                        |
