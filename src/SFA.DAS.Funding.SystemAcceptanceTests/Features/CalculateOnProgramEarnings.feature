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
	And the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year
	  | Delivery Period | Academic Year | Calendar Period |
	  | 1               | 2223          | August          |
	  | 2               | 2223          | September       |
	  | 3               | 2223          | October         |
	  | 4               | 2223          | November        |
	  | 5               | 2223          | December        |
	  | 6               | 2223          | January         |
	  | 7               | 2223          | February        |
	  | 8               | 2223          | March           |
	  | 9               | 2223          | April           |
	  | 10              | 2223          | May             |
	  | 11              | 2223          | June            |
	  | 12              | 2223          | July            |


	Examples:
	| start_date | planned_end_date | agreed_price | training_code | planned_number_of_months | instalment_amount | first_delivery_period | first_calendar_period |
	| 2022-08-01 | 2023-07-31       | 15,000       | 614           | 12                       | 1000              | 01-2223               | 08/2022               |