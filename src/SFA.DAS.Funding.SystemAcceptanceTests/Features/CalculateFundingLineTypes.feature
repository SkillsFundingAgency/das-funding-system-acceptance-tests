#Feature: CalculateFundingLineTypes
#
#As a Finance Officer
#I want to know the funding line type for earnings
#So that I can estimate the correct forecasted funding
#
#
#@regression
#Scenario: Calculate funding line types from Apprentice's age
#	Given an apprenticeship has a start date of <start_date>, a planned end date of <planned_end_date>, an agreed price of <agreed_price>, and a training code <training_code>
#	And the apprenticeship learner has a date of birth <date_of_birth>
#	When the apprenticeship commitment is approved
#	Then the leaners age <age_at_course_start> at the start of the course and funding line type <funding_line_type> must be calculated
#
#Examples:
#	| start_date | planned_end_date | agreed_price | training_code | date_of_birth | age_at_course_start | funding_line_type                              |
#	| 2022-08-01 | 2023-07-31       | 15000        | 614           | 2008-08-01    | 14                  | 16-18 Apprenticeship (Employer on App Service) |
#	| 2022-08-01 | 2023-07-31       | 15000        | 614           | 2005-08-01    | 17                  | 16-18 Apprenticeship (Employer on App Service) |
#	| 2022-08-01 | 2023-07-31       | 15000        | 177           | 2004-08-01    | 18                  | 16-18 Apprenticeship (Employer on App Service) |
#	| 2022-08-01 | 2023-07-31       | 15000        | 177           | 2003-08-01    | 19                  | 19+ Apprenticeship (Employer on App Service)   |
