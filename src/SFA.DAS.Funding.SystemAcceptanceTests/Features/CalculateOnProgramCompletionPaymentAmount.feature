Feature: Calculate on-program completion amount for an approved apprenticeship

As a Training provider
I want the completion earnings (Forecast) 
So that they feed into payment calculations and I get paid

@regression
Scenario: On program completion payment amount
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price> and funding band max of <funding_band_max>
	When the apprenticeship commitment is approved
	Then the total completion amount <completion_amount> should be calculated as 20% of the adjusted price
	
Examples:
	| start_date | planned_end_date | agreed_price | funding_band_max | completion_amount |
	| 2022-08-01 | 2023-07-31       | 15,000       | 20,000           | 3000              |
	| 2022-08-01 | 2023-07-31       | 15,000       | 12,000           | 2400              |
