Feature: Calculate incentives for learners that finish before the end of the cut off month

As a Training provider & Employer
I want monthly incentive earnings & payments to be calculated
So we both get paid incentives correctly

# User stories/ACs covered in this file:
# FLP-1036 AC3

@regression
Scenario: Incentive Earnings & Payments for learner ending after 90 days not at the end of the month
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is not generated for provider & employer
	And the first incentive payment is generated for provider & employer
	And the second incentive payment is not generated for provider & employer

Examples:
	| start_date | planned_end_date | agreed_price | training_code | age |
	| 2022-08-15 | 2022-11-25       | 15,000       | 614           | 17	 |
	| 2022-08-15 | 2022-11-25       | 15,000       | 614           | 20	 |


@regression
Scenario: Incentive Earnings & Payments for learner ending after 365 days not at the end of the month
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
	And the first incentive payment is generated for provider & employer
	And the second incentive payment is generated for provider & employer

Examples:
	| start_date | planned_end_date | agreed_price | training_code | age |
	| 2022-08-15 | 2023-08-25       | 15,000       | 614           | 17	 |
	| 2022-08-15 | 2023-08-25       | 15,000       | 614           | 20	 |