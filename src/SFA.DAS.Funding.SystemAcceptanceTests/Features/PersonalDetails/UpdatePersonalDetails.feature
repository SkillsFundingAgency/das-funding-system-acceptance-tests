Feature: UpdatePersonalDetails

At the Dfe
I want to know when the email address, family name and/or given name of an apprentice has changed
So that the data we store is always up to date

@regression
Scenario: Apprentice personal details are updated
	Given an apprenticeship has a start date of 2024-08-01, a planned end date of 2025-07-31, an agreed price of 15000, and a training code 2
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date 2024-08-01 to date 2025-07-31
	And Learner's personal details are updated with first name <first_name> last name <last_name> and email <email>
	And SLD submit updated learners details
	Then Learner's personal details are updated in learning db with first name <first_name> last name <last_name> and email <email>
	And a personal details changed event is published to approvals with first name <first_name> last name <last_name> and email <email>

Examples:
	| first_name                  | last_name | email             |
	| Sys Acceptance Tests - John | Doe       | john.doe@test.com |
	| Sys Acceptance Tests - Jane | Doe       | null              |

@regression
Scenario: SLD inform us of a change to the aprentices date of birth
	Given an apprenticeship has a start date of <start_date>, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is <age>
	And the apprenticeship commitment is approved
	When SLD record on-programme cost as total price 15000 from date <start_date> to date 2027-07-31
	And Learner's date of birth is updated to <dob>
	And SLD submit updated learners details
	Then Learner's date of birth is updated in learning db to <dob>
	And the first incentive due date for provider & employer is <first_incentive_due_date>
	And the second incentive due date for provider & employer is <second_incentive_due_date>

Examples:
	| start_date | age | dob        | first_incentive_due_date | second_incentive_due_date |
	| 2025-08-01 |  17 | 2008-12-31 | 2025-10-29               | 2026-07-31                |
	| 2025-08-01 |  17 | 2007-08-02 | 2025-10-29               | 2026-07-31                |
	| 2025-08-05 |  18 | 2007-08-01 | 2025-11-02               | 2026-08-04                |

# In the below scenario, the learner is initially 18 yo and hence employer and provider's incentives are generated.
# The dob is then revised, making the learner 19 yo on start of the apprenticeship (Not marked as care leaver)
@regression
Scenario: Apprentices age is updated to 19 years old without care leavers - incentives removed  
	Given an apprenticeship has a start date of 2025-08-02, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 18
	And the apprenticeship commitment is approved
	And the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
	When SLD record on-programme cost as total price 15000 from date 2025-08-02 to date 2027-07-31
	And Learner's date of birth is updated to 2006-08-01
	And SLD submit updated learners details
	Then Learner's date of birth is updated in learning db to 2006-08-01
	And the first incentive earning is not generated for provider & employer
	And the second incentive earning is not generated for provider & employer


# In the below scenario, the learner is initially 24 yo and marked as care leaver. Employer and provider's incentives are generated.
# The dob is then revised, making the learner 25 yo on start of the apprenticeship 
@regression
Scenario: Apprentices age is updated to 25 years old - incentives removed  
	Given an apprenticeship has a start date of 2025-08-02, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 24
	And the apprenticeship commitment is approved
	And the apprentice is marked as a care leaver
	And the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
	When SLD record on-programme cost as total price 15000 from date 2025-08-02 to date 2027-07-31
	And Learner's date of birth is updated to 2000-08-01
	And SLD submit updated learners details
	Then Learner's date of birth is updated in learning db to 2000-08-01
	And the first incentive earning is not generated for provider & employer
	And the second incentive earning is not generated for provider & employer


# In the below scenario, the learner is initially 19 yo and NOT marked as care leaver. Employer and provider's incentives are NOT generated.
# The dob is then revised, making the learner 18 yo on start of the apprenticeship - Incentives generated
@regression
Scenario: Apprentices age is updated to 18 years old - incentives added  
	Given an apprenticeship has a start date of 2025-08-02, a planned end date of 2027-07-31, an agreed price of 15000, and a training code 2
	And the age at the start of the apprenticeship is 19
	And the apprenticeship commitment is approved
	And the first incentive earning is not generated for provider & employer
	And the second incentive earning is not generated for provider & employer
	When SLD record on-programme cost as total price 15000 from date 2025-08-02 to date 2027-07-31
	And Learner's date of birth is updated to 2007-08-01
	And SLD submit updated learners details
	Then Learner's date of birth is updated in learning db to 2007-08-01
	And the first incentive earning is generated for provider & employer
	And the second incentive earning is generated for provider & employer
