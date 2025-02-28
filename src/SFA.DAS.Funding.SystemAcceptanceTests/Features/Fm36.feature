﻿Feature: Fm36

Retrieve Fm36 data

@regression
Scenario: Retrieve Valid Fm36 data
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then fm36 data exists for that apprenticeship

Examples:
	| start_date      | end_date        | agreed_price | training_code |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             |
