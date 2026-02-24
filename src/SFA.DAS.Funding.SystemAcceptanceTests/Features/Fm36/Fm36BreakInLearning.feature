#@nonparallelizable
#Feature: Fm36BreakInLearning
#
#These tests ensure that Breaks in Learning are reflected in the FM36 correctly
#
#@regression
#Scenario: Return from a Break in Learning is not included in results if after end of current AY
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	And the apprenticeship commitment is approved
#	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
#	And SLD inform us of a break in learning with pause date <pause_date>
#	And SLD inform us of a return from break in learning with a new learning start date <return_date>
#	And SLD submit updated learners details
#	And the fm36 data is retrieved for <end_date>
#	Then fm36 contains <expected_learning_deliveries> Learning Deliveries
#
#Examples:
#	| start_date       | end_date        | agreed_price | training_code | pause_date       | return_date     | expected_learning_deliveries |
#	| previousAY-08-01 | currentAY-04-23 | 15000        | 2             | previousAY-05-01 | currentAY-08-01 | 1                            |
#	| previousAY-08-01 | currentAY-04-23 | 15000        | 2             | currentAY-11-01  | currentAY-03-01 | 2                            |