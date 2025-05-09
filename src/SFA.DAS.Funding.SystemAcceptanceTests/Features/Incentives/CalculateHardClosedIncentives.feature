Feature: Calculate incentives for learners in a hard closed academic year

As a Training provider & Employer
I want monthly incentive earnings & payments to be calculated
So we both get paid incentives correctly

# User stories/ACs covered in this file:
# FLP-1036 AC2

@regression
Scenario: Don't Generate Incentive Earnings & Payments For Hard Closed Years
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	When the apprenticeship commitment is approved
	Then the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
	And the first incentive payment is not generated for provider & employer
	And the second incentive payment is not generated for provider & employer

Examples:
	| start_date | planned_end_date | agreed_price | training_code | age |
	| 2022-08-01 | 2024-07-31       | 15,000       | 614           | 17  |
	| 2022-08-01 | 2024-07-31       | 15,000       | 614           | 20  |
