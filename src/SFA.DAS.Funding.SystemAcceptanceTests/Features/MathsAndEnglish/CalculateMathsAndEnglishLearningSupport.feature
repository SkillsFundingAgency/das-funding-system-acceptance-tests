Feature: CalculateMathsAndEnglishLearningSupport

As the DfE
I want to know when the details for learning support has changed for an apprentice
So that earnings and payments can be recalculated based on the latest data

---------- LearnAimRef used ----------
60075880 => Amount: 471.00000

@regression @ignoreInPREPRODandPP
Scenario: Learning support added for Maths and English course
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When a Maths and English learning is recorded from <start_date> to <end_date> with learnAimRef 60075880 and learning support from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then the functionalSkill earnings event is sent to payments
	And maths and english learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>
	And functionalSkill learning support earnings are sent to payments from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | currentAY-10-12 | currentAY-02-15 | currentAY-R03            | currentAY-R06           |

@regression @ignoreInPREPRODandPP
Scenario: Learning support added for Maths and English course - paid until completion
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When an English and Maths learning is recorded from <start_date> to <end_date> with learnAimRef 60075880, completion date as <completion_date>, learning support from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then the functionalSkill earnings event is sent to payments
	And maths and english learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>
	And functionalSkill learning support earnings are sent to payments from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | completion_date | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | currentAY-03-20 | currentAY-10-12 | currentAY-04-15 | currentAY-R03            | currentAY-R07           |

@regression @ignoreInPREPRODandPP
Scenario: No LSF earnings when learner Withdraws from E&M
	Given an apprenticeship has a start date of <start_date>, a planned end date of <end_date>, an agreed price of 12000, and a training code 1
	And a draft apprenticeship learning is created
	And the apprenticeship learning is approved
	When English and Maths learning is recorded from <start_date> to <end_date> with learnAimRef 60075880, withdrawal date <withdrawal_date>, learning support from <ls_start_date> to <ls_end_date>
	And SLD record on-programme cost as total price 12000 from date <start_date> to date <end_date>
	And SLD submit updated learners details
	Then the functionalSkill earnings event is sent to payments
	And maths and english learning support earnings are generated from periods <expected_first_ls_period> to <expected_last_ls_period>
	And functionalSkill learning support earnings are sent to payments from periods <expected_first_ls_period> to <expected_last_ls_period>

Examples:
	| start_date      | end_date        | withdrawal_date | ls_start_date   | ls_end_date     | expected_first_ls_period | expected_last_ls_period |
	| currentAY-09-25 | currentAY-04-15 | currentAY-03-20 | currentAY-10-12 | currentAY-04-15 | currentAY-R03            | currentAY-R07           |

