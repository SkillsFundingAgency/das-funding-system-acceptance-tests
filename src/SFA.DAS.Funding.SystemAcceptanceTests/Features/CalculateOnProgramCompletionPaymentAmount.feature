Feature: Calculate on-program completion amount for an approved apprenticeship

As a Training provider
I want the completion earnings (Forecast) 
So that they feed into payment calculations and I get paid


*** This feature is dependant on FundingBandMax value for the training code used *** 
*** Please do not change it without the consent of the team's testers ***

@regression
Scenario: Calculate the on program completion payment amount
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	When the apprenticeship commitment is approved
	Then the total completion amount <completion_amount> should be calculated as 20% of the adjusted price
	
Examples:
	| start_date | planned_end_date | agreed_price | training_code | completion_amount |
	| 2022-08-01 | 2023-07-31       | 15000        | 614           | 3000              |
	| 2022-08-01 | 2023-07-31       | 15000        | 177           | 1800              |
