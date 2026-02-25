Feature: CreateShortCourse

SLD provides new information on Learners on Short Courses to a dedicated endpoint.
Short Courses are stored in Learning.
Approvals are notified via an event.
Earnings are calculated (as part of a future story).

@regression
Scenario: Add a Short Course
	When SLD record a new Short Course with a start date of currentAY-08-01 and an expected end date of currentAY-07-31
	And SLD submit the Short Course details
	Then the Short Course details are recorded in Learning

