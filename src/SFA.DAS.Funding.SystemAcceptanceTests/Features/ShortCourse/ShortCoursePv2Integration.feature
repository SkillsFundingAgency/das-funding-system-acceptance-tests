Feature: ShortCoursePv2Integration

As the payments service
I want to be informed of all approved actual apprenticeship unit earnings
so that payments can be generated

@tag1
Scenario: Inform payments that the 30% milestone has been reached
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	When the short course is approved
	Then send the payable 30% milestone earnings to payments