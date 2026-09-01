Feature: Completion

When the SLD inform us of a Learning's Completion
Then we should roll-up future earnings into a single balancing payment
And record the completion payment as earned

@regression
Scenario: Balancing and Completion earnings on Completion
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	When Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-07-22
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2000 of type Balancing is generated for period nextAY-R11
	And an earning of 3000 of type Completion is generated for period nextAY-R12

@regression
Scenario: Balancing and Completion earnings on Completion - Completion removed
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-06-15
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2000 of type Balancing is generated for period nextAY-R11
	And an earning of 3000 of type Completion is generated for period nextAY-R11
	When SLD resubmits ILR
	And completion date is removed
	And achievement date is removed
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods nextAY-R01 to nextAY-R12
	And Balancing earning is removed
	And Completion earning is removed

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved earlier
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-06-15
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2000 of type Balancing is generated for period nextAY-R11
	And an earning of 3000 of type Completion is generated for period nextAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on nextAY-05-20
	And Learning Achievement date is recorded on nextAY-05-20
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods nextAY-R01 to nextAY-R09
	And an earning of 3000 of type Balancing is generated for period nextAY-R10
	And an earning of 3000 of type Completion is generated for period nextAY-R10

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved later
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-06-15
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2000 of type Balancing is generated for period nextAY-R11
	And an earning of 3000 of type Completion is generated for period nextAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on nextAY-07-05
	And Learning Achievement date is recorded on nextAY-07-05
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods nextAY-R01 to nextAY-R11
	And an earning of 1000 of type Balancing is generated for period nextAY-R12
	And an earning of 3000 of type Completion is generated for period nextAY-R12

@regression
Scenario: Balancing and Completion earnings on Completion - Change of price post Completion
	Given a learning has a start date of nextAY-08-01, a planned end date of nextAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-06-15
	And SLD record on-programme cost as total price 15000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2000 of type Balancing is generated for period nextAY-R11
	And an earning of 3000 of type Completion is generated for period nextAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on nextAY-06-15
	And Learning Achievement date is recorded on nextAY-06-15
	And SLD record on-programme cost as total price 18000 from date nextAY-08-01 to date nextAY-07-31
	And SLD submit updated learners details
	Then earnings of 1200 are generated from periods nextAY-R01 to nextAY-R10
	And an earning of 2400 of type Balancing is generated for period nextAY-R11
	And an earning of 3600 of type Completion is generated for period nextAY-R11

@regression
Scenario: Recalculate earnings based on qualifying period when completion date is recorded - qualifying period met
	Given a learning has a start date of <start_date>, a planned end date of <planned_end_date> and an agreed price of <agreed_price>
	When Learning Completion is recorded on <completion_date>
	And SLD record on-programme cost as total price <agreed_price> from date <start_date> to date <planned_end_date>
	And SLD submit updated learners details
	Then the expected number of earnings instalments after completion are <number_of_instalments>
	And an earning of <balancing_amount> of type Balancing is generated for period <balancing_period>
	And Completion earning is not generated

Examples:
	| start_date      | planned_end_date | agreed_price | completion_date | number_of_instalments | balancing_amount | balancing_period |
	| nextAY-08-20 | nextAY-09-02  |        15000 | nextAY-09-02 |                     1 |                0 | nextAY-R02    |
	| nextAY-08-01 | nextAY-01-14  |        15000 | nextAY-11-08 |                     3 |             4800 | nextAY-R04    |
	| nextAY-08-01 | nextAY-10-31  |        15000 | nextAY-08-14 |                     0 |            12000 | nextAY-R01    |
	| nextAY-08-01 | nextAY-07-31  |        15000 | nextAY-01-15 |                     5 |             7000 | nextAY-R06    |
	| nextAY-08-01 | nextAY-07-31  |        15000 | nextAY-07-30 |                    11 |             1000 | nextAY-R12    |

