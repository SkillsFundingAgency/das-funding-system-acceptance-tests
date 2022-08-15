Feature: Calculate earnings for an approved apprenticeship

As a Training provider
I want monthly on-program earnings to be calculated 
So they feed into payments calculation I get paid

@regression
Scenario: Earnings Generation for an approved apprenticeship
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, and an agreed price of £<agreed_price>
	When the apprenticeship commitment is approved
	Then the total on-program payment amount must be calculated as 80% of the agreed price £<adjusted_price>
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	And the instalment amount must be calculated by dividing the total on-program amount equally into the number of planned months <instalment_amount>
	And Earnings generated for each month starting from the first delivery period <first_delivery_period> and first calendar period <first_calendar_period>

	Examples:
	| start_date | planned_end_date | agreed_price | adjusted_price | planned_number_of_months | instalment_amount | first_delivery_period | first_calendar_period |
	| 2022-08-01 | 2023-07-31       | 15,000       | 12,000         | 12                       | 1000              | R01-2223              | 08/2022               |