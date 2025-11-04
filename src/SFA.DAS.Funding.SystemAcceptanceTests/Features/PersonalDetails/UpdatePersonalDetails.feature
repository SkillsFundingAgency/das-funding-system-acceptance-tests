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

