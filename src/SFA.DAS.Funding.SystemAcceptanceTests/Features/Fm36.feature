Feature: Fm36

Retrieve Fm36 data

@regression
Scenario: Retrieve Valid Fm36 data
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then fm36 data exists for that apprenticeship

Examples:
	| start_date      | end_date     | agreed_price | training_code | age |
	| currentAY-08-01 | nextAY-07-31 | 15000        | 2             | 19  |
	| currentAY-08-01 | nextAY-07-31 | 15000        | 2             | 17  |

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

@regression
Scenario: Price change approved; new price episode in FM36 block
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the apprenticeship commitment is approved
	And the total price is above or below or at the funding band maximum
	And a price change request was sent on <pc_from_date>
	And the price change request has an approval date of <pc_approved_date> with a new total <new_total_price>
	When the price change is approved
	And Payments Generated Events are published
	And the fm36 data is retrieved for currentDate
	Then fm36 block contains a new price episode starting <pc_from_date> with episode 1 tnp of <agreed_price> and episode 2 tnp of <new_total_price>
	
Examples:
	| start_date      | end_date     | agreed_price | training_code | pc_from_date    | new_total_price | pc_approved_date |
	| currentAY-08-20 | nextAY-04-23 | 15000        | 2             | currentAY-09-29 | 18000           | currentAY-09-29  |


@regression
Scenario: Start date change approved
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the learner is aged <age> at the start of the apprenticeship
	And the apprenticeship commitment is approved
	And the fm36 data is retrieved for currentDate
	And a start date change request was sent with an approval date of <sdc_approved_date> with a new start date of <new_start_date> and end date of <new_end_date>
	When the start date change is approved
	And Payments Generated Events are published
	And the fm36 data is retrieved for currentDate
	Then the fm36 PriceEpisodeInstalmentValue is <new_expected_earnings>
	And Incentive periods and dates are updated in the fm36 response

Examples:
	| start_date      | end_date               | agreed_price | training_code | new_start_date  | new_end_date           | sdc_approved_date | previous_earnings | new_expected_earnings | age |
	| currentAY-08-23 | currentAYPlusTwo-08-23 | 15000        | 2             | currentAY-12-23 | currentAYPlusTwo-08-23 | currentAY-06-10   | 500               | 600                   | 17  |


@regression
Scenario: Retrieve Valid Fm36 data for learners aged 15
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the learner is aged <age> at the start of the apprenticeship
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then incentives earnings are generated for learners aged 15

Examples:
	| start_date      | end_date     | agreed_price | training_code | age |
	| currentAY-08-01 | nextAY-11-15 | 15000        | 2             | 15  |


@regression
Scenario: Do not retrieve Fm36 data for Inactive learners
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the learner is aged <age> at the start of the apprenticeship
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then fm36 data does not exist for that apprenticeship

Examples:
	| start_date        | end_date               | agreed_price | training_code | age |
	| previousAy-08-01  | previousAY-07-31       | 15000        | 2             | 16  |
	| nextAy-08-01      | CurrentAyPlusTwo-11-15 | 15000        | 2             | 18  |
	| nextMonthFirstDay | CurrentAyPlusTwo-11-15 | 15000        | 2             | 19  |


@regression
Scenario: Retrieve Fm36 data for Active learners
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the learner is aged <age> at the start of the apprenticeship
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved for currentDate
	Then learner is returned in the fm36 response

Examples:
	| start_date            | end_date               | agreed_price | training_code | age |
	| lastDayOfCurrentMonth | CurrentAyPlusTwo-11-15 | 15000        | 91            | 19  |

@regression
Scenario: Retrieve Valid Fm36 19-24 incentives data
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	And the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	When the fm36 data is retrieved for currentDate
	Then Incentive periods and dates are updated in the fm36 response

Examples:
	| start_date      | end_date     | agreed_price | training_code | age |
	| currentAY-08-01 | nextAY-07-31 | 15000        | 2             | 19  |
	| currentAY-08-01 | nextAY-07-31 | 15000        | 2             | 24  |

@regression
Scenario: Retrieve Valid Fm36 learning support data
	Given an apprenticeship has a start date of currentAY-08-01, a planned end date of currentAY-07-31, an agreed price of 15000, and a training code 614
	And the age at the start of the apprenticeship is 19
	When the apprenticeship commitment is approved
	And learning support is recorded from <learning_support_start> to <learning_support_end>
	When the fm36 data is retrieved for currentDate
	Then learning support amounts and periods from <expected_first_payment_period> to <expected_last_payment_period> are updated in the fm36 response

Examples:
	| learning_support_start | learning_support_end | expected_first_payment_period | expected_last_payment_period |
	| currentAY-08-01        | currentAY-12-15      | currentAY-R01                 | currentAY-R04                |
	| currentAY-09-01        | currentAY-12-15      | currentAY-R02                 | currentAY-R04                |