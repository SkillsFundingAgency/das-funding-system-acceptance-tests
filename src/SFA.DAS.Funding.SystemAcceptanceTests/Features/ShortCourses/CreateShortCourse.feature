Feature: CreateShortCourse

SLD provides new information on Learners on Short Courses to a dedicated endpoint.
Short Courses are stored in Learning.
Approvals are notified via an event.
Earnings are calculated (as part of a future story).

@regression
Scenario: Add a Short Course
	When SLD inform us of a new Short Course
	Then the Short Course details are recorded in Learning
	And a LearnerData event is published to approvals

