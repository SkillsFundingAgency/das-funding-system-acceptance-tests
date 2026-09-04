Feature: Calculate On program earnings based on funding band maximum

As the ESFA
I want the funding band maximum to be applied  
So we don’t overpay for apprenticeship funding

| Training Code | Max Funding |
| 614			| 27000       |


@regression
Scenario: On program earnings generation when agreed price is above funding band max
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	When the apprenticeship commitment is approved
	And the agreed price is above the funding band maximum for the selected course
	Then Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>
	
Examples:
	| start_date | planned_end_date | agreed_price | training_code | instalment_amount |
	| 2026-08-01 | 2028-07-31       |        50000 |           614 |               900 |
