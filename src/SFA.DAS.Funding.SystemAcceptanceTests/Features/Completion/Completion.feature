Feature: Completion

When the SLD inform us of a Learning's Completion
Then we should roll-up future earnings into a single balancing payment
And record the completion payment as earned

@regression
Scenario: Balancing and Completion earnings on Completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11

@regression
Scenario: Balancing and Completion earnings on Completion - Completion removed
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And completion date is removed
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R12
	And Balancing earning is removed
	And Completion earning is removed

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved earlier
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on currentAY-05-20
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R09
	And an earning of 3000 of type Balancing is generated for period currentAY-R10
	And an earning of 3000 of type Completion is generated for period currentAY-R10

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved later
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on currentAY-07-05
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R11
	And an earning of 1000 of type Balancing is generated for period currentAY-R12
	And an earning of 3000 of type Completion is generated for period currentAY-R12

@regression
Scenario: Balancing and Completion earnings on Completion - Change of price post Completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 18000 from date currentAY-08-01 to date currentAY-07-31
	And SLD submit updated learners details
	Then earnings of 1200 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2400 of type Balancing is generated for period currentAY-R11
	And an earning of 3600 of type Completion is generated for period currentAY-R11

@regression
Scenario: Recalculate earnings based on qualifying period when completion date is recorded - qualifying period met
	Given a learning has a start date of <start_date>, a planned end date of <planned_end_date> and an agreed price of <agreed_price>
	When Learning Completion is recorded on <completion_date>
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <planned_end_date>
	And SLD submit updated learners details
	Then the expected number of earnings instalments after completion are <number_of_instalments>
	And an earning of <balancing_amount> of type Balancing is generated for period <balancing_period>
	And an earning of <completion_amount> of type Completion is generated for period <completion_period>

Examples:
	| start_date      | planned_end_date | agreed_price | completion_date | number_of_instalments | balancing_amount | balancing_period | completion_amount | completion_period |
	| currentAY-08-01 | currentAY-08-13  |        15000 | currentAY-08-01 |                     0 |            12000 | currentAY-R01    |              3000 | currentAY-R01     |
	| currentAY-08-01 | currentAY-08-14  |        15000 | currentAY-08-14 |                     0 |            12000 | currentAY-R01    |              3000 | currentAY-R01     |
	| currentAY-08-20 | currentAY-09-02  |        15000 | currentAY-09-02 |                     1 |                0 | currentAY-R02    |              3000 | currentAY-R02     |
	| currentAY-08-01 | currentAY-01-14  |        15000 | currentAY-11-08 |                     3 |             4800 | currentAY-R04    |              3000 | currentAY-R04     |
	| currentAY-08-01 | currentAY-10-31  |        15000 | currentAY-08-14 |                     0 |            12000 | currentAY-R01    |              3000 | currentAY-R01     |
	| currentAY-08-01 | currentAY-07-31  |        15000 | currentAY-01-15 |                     5 |             7000 | currentAY-R06    |              3000 | currentAY-R06     |

