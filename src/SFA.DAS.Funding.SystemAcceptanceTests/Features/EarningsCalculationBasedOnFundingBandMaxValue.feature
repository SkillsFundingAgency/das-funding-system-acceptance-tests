Feature: EarningsCalculationBasedOnFundingBandMaxValue

As the ESFA
I want the funding band maximum to be applied  
So we don’t overpay for apprenticeship funding

@regression
Scenario Outline: Earnings Generation including funding band value for an approved apprenticeship
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of £<agreed_price> and a funding band max as £<funding_band_max>
	When the apprenticeship commitment is approved
	Then 80% of the lowest value between agreed price and funding band price is divided equally into number of planned months <instalment_amount> 


	Examples:
	| start_date | planned_end_date | agreed_price | funding_band_max	| instalment_amount |
	| 2022-08-01 | 2023-07-31       | 20,000       |  15,000            | 1000				|
	| 2022-08-01 | 2023-07-31       | 15,000       |  12,000            | 800				|
	| 2022-08-01 | 2023-07-31       | 15,000       |  15,000            | 1000              |
	| 2022-08-01 | 2023-07-31       | 15,000       |  0					| 0		            |
	| 2022-08-01 | 2023-07-31       | 0		       |  15,000			| 0		            |