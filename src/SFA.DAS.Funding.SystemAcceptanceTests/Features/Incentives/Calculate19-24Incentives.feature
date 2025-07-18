﻿Feature: Calculate incentives for 19-24 learners

As a Training provider & Employer
I want monthly 19-24 incentive earnings to be calculated
So we both get paid incentives correctly

# User stories/ACs covered in this file:
# FLP-1044 AC0,AC1,AC2
# FLP-1036 AC1(19-24 part)

@regression
Scenario: 19-24 Incentive Earnings 
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 19  |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 21  |
	| currentAY-08-01 | currentAY-07-31  | 15,000       | 614           | 24  |


@regression
Scenario: 19-24 Incentive Earnings (duration only long enough for first earning only)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is not generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-10-29  | 15,000       | 614           | 20  |
	| currentAY-08-01 | currentAY-07-30  | 15,000       | 614           | 20  |


@regression
Scenario: 19-24 Incentive Earnings (duration too short)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	Then no incentive earning is generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-10-28  | 15,000       | 614           | 20  |
