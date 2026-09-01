Feature: Calculate on-program earnings for an approved apprenticeship

As a Training provider
I want monthly on-program earnings to be calculated 
So they feed into payments calculation I get paid

@regression
Scenario: On-program earnings generation for an approved apprenticeship
	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	When the apprenticeship commitment is approved
	Then 80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months <instalment_amount>
	And the planned number of months must be the number of months from the start date to the planned end date <planned_number_of_months>
	And the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month and year
		| Delivery Period | Academic Year | Calendar Period |
		| 1               | 2627          | August          |
		| 2               | 2627          | September       |
		| 3               | 2627          | October         |
		| 4               | 2627          | November        |
		| 5               | 2627          | December        |
		| 6               | 2627          | January         |
		| 7               | 2627          | February        |
		| 8               | 2627          | March           |
		| 9               | 2627          | April           |
		| 10              | 2627          | May             |
		| 11              | 2627          | June            |
		| 12              | 2627          | July            |
		| 1               | 2728          | August          |
		| 2               | 2728          | September       |
		| 3               | 2728          | October         |
		| 4               | 2728          | November        |
		| 5               | 2728          | December        |
		| 6               | 2728          | January         |


Examples:
	| start_date | planned_end_date | agreed_price | training_code | planned_number_of_months | instalment_amount | first_delivery_period | first_calendar_period |
	| 2026-08-01 | 2028-02-15       | 15,000       | 614           | 18                       | 666.66667         | 01-2627               | 08/2026               |