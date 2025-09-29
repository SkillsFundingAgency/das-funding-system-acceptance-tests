Feature: Completion

When the SLD inform us of a Learning's Completion
Then we should roll-up future earnings into a single balancing payment
And record the completion payment as earned

@regression
Scenario: Balancing and Completion earnings on Completion
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	When Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11

@regression
Scenario: Balancing and Completion earnings on Completion - Completion removed
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And completion date is removed
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R12
	And Balancing earning is removed
	And Completion earning is removed

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved earlier
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on currentAY-05-20
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R09
	And an earning of 3000 of type Balancing is generated for period currentAY-R10
	And an earning of 3000 of type Completion is generated for period currentAY-R10

@regression
Scenario: Balancing and Completion earnings on Completion - Completion moved later
	Given a learning has a start date of currentAY-08-01, a planned end date of currentAY-07-31 and an agreed price of 15000
	And Learning Completion is recorded on currentAY-06-15
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	And earnings of 1000 are generated from periods currentAY-R01 to currentAY-R10
	And an earning of 2000 of type Balancing is generated for period currentAY-R11
	And an earning of 3000 of type Completion is generated for period currentAY-R11
	When SLD resubmits ILR
	And Learning Completion is recorded on currentAY-07-05
	And SLD record on-programme cost as total price 15000 from date currentAY-08-01
	And SLD submit updated learners details
	Then earnings of 1000 are generated from periods currentAY-R01 to currentAY-R11
	#And an earning of 1000 of type Balancing is generated for period currentAY-R12
	And an earning of 3000 of type Completion is generated for period currentAY-R12