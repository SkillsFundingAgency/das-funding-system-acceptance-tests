Feature: Completion

When the SLD inform us of a Learning's Completion
Then we should calculate the balancing earnings

@regression
Scenario: Balancing earnings on Completion
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD inform us that the Learning Completed on <completion_date>
	#Then the Completion date for the Learning is set -> in which service are we checking this?
	#And the Earnings are recalculated
	#And the Balancing Earning is 

Examples:
	| start_date | end_date   | agreed_price | training_code | completion_date | 
	| 2024-11-01 | 2025-11-23 | 15000        | 2             | 2025-06-23      |


	# planned_number_of_months | balancing_amount | balancing_delivery_period | completion_amount |
	#11                       | 3333.33334       | 11                        | 2000              |
