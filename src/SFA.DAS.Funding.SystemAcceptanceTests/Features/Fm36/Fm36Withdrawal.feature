﻿Feature: Fm36Withdrawal

Fm36 Withdrawl tests

@regression
Scenario: Withdrawal of learner - FundStart should be False if withdrawn before qualifying period end (FLP-969 AC1)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	And the fm36 data is retrieved for currentDate
	Then fm36 FundStart value is <expected_fundstart>
	And fm36 ActualDaysIL value is <expected_actual_days_in_learning>

Examples:
	| start_date      | end_date        | agreed_price | training_code | reason                 | last_day_of_delivery | expected_fundstart | expected_actual_days_in_learning |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             | WithdrawDuringLearning | currentAY-09-11      | false              | 42                               |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             | WithdrawDuringLearning | currentAY-09-12      | true               | 43                               |

@regression
Scenario: Withdrawal of learner from start results in no FM36 block (FLP-969 AC3)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	And the fm36 data is retrieved for currentDate
	Then fm36 data does not exist for that apprenticeship

Examples:
	| start_date      | end_date        | agreed_price | training_code | reason            | last_day_of_delivery |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             | WithdrawFromStart | currentAY-08-01      |

@regression
Scenario: Withdrawal of learner from private beta results in no FM36 block (FLP-969 AC4)
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	When a Withdrawal request is recorded with a reason <reason> and last day of delivery <last_day_of_delivery>
	And the fm36 data is retrieved for currentDate
	Then fm36 data does not exist for that apprenticeship

Examples:
	| start_date      | end_date        | agreed_price | training_code | reason           | last_day_of_delivery |
	| currentAY-08-01 | currentAY-07-31 | 15000        | 2             | WithdrawFromBeta | currentAY-011-01     |
