Feature: FM36EnglishAndMaths

Retrieve valid Fm36 learning deliveries 


@regression
Scenario: Retrieve Valid Fm36 learning delivery for English and Maths
	Given an apprenticeship has a start date of currentAY-09-23, a planned end date of nextAY-08-23, an agreed price of 15000, and a training code 2
	And the learner is aged 19 at the start of the apprenticeship
	When the apprenticeship commitment is approved
	And SLD record on-programme cost as total price 15000 from date currentAY-09-23 to date nextAY-08-23
	And a Maths and English learning is recorded from <start_date> to <end_date> with learnAimRef <learn_aim_ref>, course <course>, amount <amount>, learning support from <start_date> to <end_date>
	And SLD submit updated learners details
	And the fm36 data is retrieved for nextAY-08-01
	Then English and maths learning deliveries section is populated correctly for learnAimRef <learn_aim_ref> from <start_period> to <end_period>

Examples:
	| start_date   | end_date     | learn_aim_ref | course              | amount | start_period | end_period |
	| nextAY-08-01 | nextAY-01-15 |      60342843 | Entry level English |    931 |            1 |          5 |


