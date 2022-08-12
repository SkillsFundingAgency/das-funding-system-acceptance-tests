﻿Feature: CalculateEarnings

As a Training provider
I want monthly on-program earnings to be calculated 
So we they feed into payments calculation I get paid

@regression
Scenario: Earnings Generation for an approved apprenticeship
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, and an agreed price of £<agreed_price>
	When the apprenticeship commitment is approved
	Then the total on-program payment amount must be calculated as 80% of the agreed price <total_on_program_amount>
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	And the instalment amount must be calculated by dividing the total on-program amount equally into the number of planned months <instalment_amount>
	And an earning must be recorded for each month from the start date to the planned end date

	Examples:
	| start_date          | planned_end_date | agreed_price | total_on_program_amount | planned_number_of_months | instalment_amount |
	| August-CurrentYear  | July-NextYear    | 15,000       | 12,000				  | 12						| 1000				|