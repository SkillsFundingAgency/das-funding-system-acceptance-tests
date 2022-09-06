Feature: Calculate On program earnings based on funding band maximum

As the ESFA
I want the funding band maximum to be applied  
So we don’t overpay for apprenticeship funding

*** This feature is dependant on FundingBandMax value for the training code used *** 
*** Please do not change it without the consent of the team's testers ***

@regression
Scenario: On program earnings generation when funding band max is more than agreed price
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	When the apprenticeship commitment is approved
	And the agreed price is below the funding band maximum <funding_band_max> for the selected course
	Then Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>
	
Examples:
	| start_date | planned_end_date | agreed_price | training_code | funding_band_max | instalment_amount |
	| 2022-08-01 | 2023-07-31       | 15000        | 614           | 27000            | 1000              |

@regression
Scenario Outline: On-program earnings generation when funding band max is less than agreed price
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	When the apprenticeship commitment is approved
	And the agreed price is above the funding band maximum <funding_band_max> for the selected course
	Then Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>

Examples:
	| start_date | planned_end_date | agreed_price | training_code | funding_band_max | instalment_amount |
	| 2022-08-01 | 2023-07-31       | 15000        | 177           | 9000             | 600               |