Feature: Fm36ChangeOfCircumstance

Fm36 Change Of Circumstance

@regression
Scenario: Price change approved; new price episode in FM36 block
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	When SLD record on-programme cost as total price <agreed_price> from date <start_date>
	And SLD record on-programme cost as total price <new_total_price> from date <pc_from_date>
	And SLD submit updated learners details
	And the fm36 data is retrieved for previousAY-07-31
	Then fm36 block contains a new price episode starting <pc_from_date> with episode 1 tnp of <agreed_price> and episode 2 tnp of <new_total_price>
	
Examples:
	| start_date       | end_date        | agreed_price | training_code | pc_from_date     | new_total_price | pc_approved_date |
	| previousAY-08-20 | currentAY-04-23 | 15000        | 2             | previousAY-09-29 | 18000           | previousAY-09-29 |

@regression
Scenario: Start date change approved
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the learner is aged <age> at the start of the apprenticeship
	And the apprenticeship commitment is approved
	And the fm36 data is retrieved for currentDate
	When SLD record on-programme cost as total price <agreed_price> from date <new_start_date>
	And SLD submit updated learners details
	And the fm36 data is retrieved for previousAY-07-31
	Then the fm36 PriceEpisodeInstalmentValue is <new_expected_earnings>
	And Incentive periods and dates are updated in the fm36 response

Examples:
	| start_date       | end_date        | agreed_price | training_code | new_start_date   | previous_earnings | new_expected_earnings | age |
	| previousAY-08-23 | currentAY-08-23 | 15000        | 2             | previousAY-12-23 | 1000              | 1500                  | 17  |
