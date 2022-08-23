Feature: Calculate On program earnings based on funding band maximum

As the ESFA
I want the funding band maximum to be applied  
So we don’t overpay for apprenticeship funding

@regression
Scenario: On program earnings generation when funding band max is less than agreed price
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, and an agreed price of £<agreed_price>
	When the agreed price is below the funding band maximum <funding_band_max> for the selected course
	And the apprenticeship commitment is approved
	Then Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount> 
	
	Examples:
	| start_date | planned_end_date | agreed_price | funding_band_max | instalment_amount |
	| 2022-08-01 | 2023-07-31       | 15,000       | 20,000           | 1000              |

@regression
Scenario Outline: On-program earnings generation when funding band max is more than agreed price
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, and an agreed price of £<agreed_price>
	When the agreed price is above the funding band maximum <funding_band_max> for the selected course 
	And the apprenticeship commitment is approved
	Then Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>

	Examples:
	| start_date | planned_end_date | agreed_price | funding_band_max | instalment_amount |
	| 2022-08-01 | 2023-07-31       | 15,000       | 12,000           | 800	              |