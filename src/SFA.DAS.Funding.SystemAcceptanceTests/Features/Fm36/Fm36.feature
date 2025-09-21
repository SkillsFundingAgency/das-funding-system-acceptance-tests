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
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	When the fm36 data is retrieved for currentDate
	Then learning support amounts and periods from <expected_first_payment_period> to <expected_last_payment_period> are updated in the fm36 response

Examples:
	| learning_support_start | learning_support_end | expected_first_payment_period | expected_last_payment_period |
	| currentAY-08-01        | currentAY-12-15      | currentAY-R01                 | currentAY-R04                |
	| currentAY-09-01        | currentAY-12-15      | currentAY-R02                 | currentAY-R04                |


#Remove the below test when FM36 Endpoint is removed from Earnings outer 
#and repurpose the exisiting tests to point to LearnerData outer 

@regression
Scenario: Retrieve Valid Fm36 data through LearnerData outer api
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of <agreed_price>, and a training code <training_code>
	And the age at the start of the apprenticeship is <age>
	And the apprenticeship commitment is approved
	When the fm36 data is retrieved through LearnerData outer api for currentDate
	Then fm36 data exists for that apprenticeship

Examples:
	| start_date      | end_date     | agreed_price | training_code | age |
	| currentAY-08-01 | nextAY-07-31 | 15000        | 2             | 19  |