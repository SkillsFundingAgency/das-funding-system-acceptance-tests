Feature: EarningsToPayments

Tests FLP-2003

Scenario: Send apprenticeship earnings to payments on approval 
	Given there is an apprenticeship
	And a draft apprenticeship learning is created with a completion and achievement date of currentAY-07-31
	When the apprenticeship learning is approved
	Then send the apprenticeship approved earnings to payments
	| deliveryPeriod | amount    | type       |
	| 3              | 500.00000 | learning   |
	| 10             | 500.00000 | learning   |
	| 11             | 500.00000 | learning   |
	| 12             | 3000.00000| completion |
	| 5              | 500.00000 | learning   |
	| 6              | 500.00000 | learning   |
	| 9              | 500.00000 | learning   |
	| 4              | 500.00000 | learning   |
	| 7              | 500.00000 | learning   |
	| 2              | 500.00000 | learning   |
	| 12             | 6500.00000| balancing  |
	| 8              | 500.00000 | learning   |
	| 1              | 500.00000 | learning   |

@ignore
Scenario: Send the apprenticeship learning type to payments
	Given a draft apprenticeship learning is created
	When the apprenticeship learning is approved
	Then the apprenticeship "learning type" is sent to Payments

@ignore
Scenario: Inform payments when an apprenticeship is being funded by a levy transfer
	Given a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	And the learner is being funded by a levy transfer
	When payments are informed of on-programme earnings
	Then inform payments that the learner is being funded by a levy transfer