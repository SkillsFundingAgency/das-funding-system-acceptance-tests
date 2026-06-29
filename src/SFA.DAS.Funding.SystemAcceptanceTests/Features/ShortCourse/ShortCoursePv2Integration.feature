Feature: ShortCoursePv2Integration

As the payments service
I want to be informed of all approved actual apprenticeship unit earnings
so that payments can be generated

#FLP-1531
@regression
Scenario: Inform payments that the 30% milestone has been reached on approval
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	When the short course is approved
	Then send the payable 30% milestone earnings to payments
	And the payment command sent to approvals has correct values assigned 

#FLP-1531
@regression
Scenario: Inform payments that the completion has been reached on approval
	Given SLD informs us of a new learner with a short course start date currentAY-08-01
	And the training provider recorded that the 30% milestone has been reached pre-approval
	And the training provider also recorded that the learner completed pre-approval
	When the short course is approved
	Then send both 30% milestone and completion earnings to payments