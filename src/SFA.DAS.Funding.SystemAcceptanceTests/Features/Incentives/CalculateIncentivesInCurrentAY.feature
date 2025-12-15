Feature: Calculate incentives for learners in current academic year

As a Training provider & Employer
I want monthly incentive earnings & payments to be calculated
So we both get paid incentives correctly

# User stories/ACs covered in this file:
# FLP-1036 AC2

@regression
Scenario: Generate Incentive Earnings For current Academic Year 19-24
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  20 |

@regression
Scenario: Generate Incentive Earnings For Current Academic Year 16-18
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer

Examples:
	| start_date      | planned_end_date | agreed_price | training_code | age |
	| currentAY-08-01 | currentAY-07-31  |       15,000 |           614 |  17 |
