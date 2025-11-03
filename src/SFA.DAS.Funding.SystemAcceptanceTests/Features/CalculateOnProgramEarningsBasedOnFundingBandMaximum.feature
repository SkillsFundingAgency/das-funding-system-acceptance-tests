#Feature: Calculate On program earnings based on funding band maximum
#
#As the ESFA
#I want the funding band maximum to be applied  
#So we don’t overpay for apprenticeship funding
#
#*** This feature is dependant on FundingBandMax value for the training code used *** 
#*** Please do not change it without the consent of the team's testers ***
#
#| Training Code | Earliest Start Date		| Latest End Date		  | Proposed Max Funding |
#| 6				| 2014-11-12 00:00:00.000	| 2019-03-03 00:00:00.000 | 27000                |
#| 6				| 2019-03-04 00:00:00.000	| NULL					  | 26000                |
#
#@regression
#Scenario: On program earnings generation when agreed price is below funding band max
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	When the apprenticeship commitment is approved
#	And the agreed price is below the funding band maximum for the selected course
#	Then Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>
#	
#Examples:
#	| start_date | planned_end_date | agreed_price | training_code | instalment_amount |
#	| 2018-08-01 | 2019-07-31       | 26100        | 6             | 1740              |
#
#@regression
#Scenario Outline: On program earnings generation when agreed price is above funding band max
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	When the apprenticeship commitment is approved
#	And the agreed price is above the funding band maximum for the selected course
#	Then Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months <instalment_amount>
#
#Examples:
#	| start_date | planned_end_date | agreed_price | training_code | instalment_amount |
#	| 2022-08-01 | 2023-07-31       | 30000        | 6             | 1733.33333        |
#	| 2018-08-01 | 2019-07-31       | 30000        | 6             | 1800              |