Feature: EarningsToPayments

Tests FLP-2003

@ignore
Scenario: Send apprenticeship earnings to payments on approval 
	Given a draft apprenticeship learning is created
	When the apprenticeship is approved by the employer
	Then send the apprenticeship approved <earning type> earnings to payments

	| earning type |
	| :----------: |
	|  learning    |
	|  completion  |
	|  balancing   |

@ignore
Scenario: Send the apprenticeship learning type to payments
	Given a draft apprenticeship learning is created
	When the apprenticeship is approved by the employer
	Then the apprenticeship "learning type" is sent to Payments

@ignore
Scenario: Inform payments when an apprenticeship is being funded by a levy transfer
	Given a draft apprenticeship learning is created
	And that apprentice record has been approved by the employer
	And the learner is being funded by a levy transfer
	When payments are informed of on-programme earnings
	Then inform payments that the learner is being funded by a levy transfer