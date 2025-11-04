Feature: Add Learner Data

In order to keep DAS in sync with new learners reported in the ILR
As SLD
I want to pass new learners data to Approvals
So that apprenticeships can be created

@regression
Scenario: Add a Learner
	When SLD inform us of a new Learner
	Then the learner's details are added to Learner Data db


@regression
Scenario: Add a learner with empty costs array
	When SLD inform us of a learner with empty costs array
	Then treat Training price as 0, EPAO price as 0 and fromDate as Start Date

@regression
Scenario: Add a learner with variations of costs array
	When SLD inform us of a learner with training price <sld_training_price>, epao as <sld_epao_price> and fromDate <sld_from_date>
	Then treat Training price as <training_price>, EPAO price as <epao_price> and fromDate as Start Date

Examples:
	| sld_training_price | sld_epao_price | sld_from_date | training_price | epao_price |
	| null               | null           | null          |              0 |          0 |
	|              12000 | null           | null          |          12000 |          0 |
	|              12000 |           3000 | null          |          12000 |       3000 |
