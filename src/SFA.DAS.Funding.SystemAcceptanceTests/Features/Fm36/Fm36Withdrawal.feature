Feature: Fm36Withdrawal

Fm36 Withdrawl tests

@regression
Scenario: Withdrawal of learner - FundStart should be False if withdrawn before qualifying period end (FLP-969 AC1)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <last_day_of_delivery>
	And SLD submit updated learners details
	And the fm36 data is retrieved for previousAY-07-31
	Then fm36 FundStart value is <expected_fundstart>
	And fm36 ActualDaysIL value is <expected_actual_days_in_learning>
	And fm36 ActualEndDate value is <last_day_of_delivery>
	And fm36 ThresholdDays value is <threshold_days>

Examples:
	| start_date       | end_date         | agreed_price | training_code | last_day_of_delivery | expected_fundstart | expected_actual_days_in_learning | threshold_days |
	| previousAY-08-01 | previousAY-07-31 | 15000        | 2             | previousAY-09-10     | false              | 41                               | 42             |
	| previousAY-08-01 | previousAY-07-31 | 15000        | 2             | previousAY-09-11     | true               | 42                               | 42             |
	| previousAY-08-31 | previousAY-09-13 | 15000        | 2             | previousAY-09-13     | true               | 14                               | 14             |
	| previousAY-08-31 | previousAY-09-13 | 15000        | 2             | previousAY-09-12     | false              | 13                               | 14             |
	| previousAY-08-01 | previousAY-01-14 | 15000        | 2             | previousAY-12-15     | true               | 137                              | 14             |
	#| previousAY-08-31 | previousAY-09-12 | 15000        | 2             | previousAY-08-31     | true               | 1                                | 1              | 
	
	# commented out test until we know how to deal with short courses (<14 days) with QP of 1 day. Learner is withdrawn back to start vs DELETE. so whether they appear in the FM36 or not. 
	# Additionally, at the moment, Learning doesnt know anout QP. Only earning does!


@regression
Scenario: Withdrawal of learner from start results in no FM36 block (FLP-969 AC3)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <end_date>
	And Learning withdrawal date is recorded on <last_day_of_delivery>
	And SLD submit updated learners details
	And the fm36 data is retrieved for currentDate
	Then fm36 data does not exist for that apprenticeship
Examples:
	| start_date      | end_date        | agreed_price | training_code | last_day_of_delivery |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             | currentAY-08-01      |

