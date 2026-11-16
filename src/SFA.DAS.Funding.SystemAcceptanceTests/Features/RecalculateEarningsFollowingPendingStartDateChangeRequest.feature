Feature: RecalculateEarningsFollowingPendingStartDateChangeRequest

As a Provider
I want earnings to reflect start date changes pending employer approval
So that payable earnings and payments are restricted until approval is complete

@regression @ignoreInPREPRODandPP
Scenario: Start date change pending employer approval marks earnings as not payable and stops future payments
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When SLD record on-programme cost as total price <agreed_price> from date <new_start_date> to date <end_date>
	And SLD submit updated learners details
	Then a withhold payments for learning event is published
	And the apprenticeship earnings event is sent to payments
	And the apprenticeship earnings to payments event has no instalments

Examples:
	| start_date      | end_date               | agreed_price | training_code | new_start_date  |
	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | currentAY-12-23 |
