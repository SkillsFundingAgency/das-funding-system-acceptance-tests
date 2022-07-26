Feature: Sample

Sample feature showing how to publish an event and then read its data from the service bus queue

Scenario: Sample event is published
	Given a sample event is published by our system
	Then event data can be asserted on


#
#Scenario: Sample output event is published
#	Given a sample input event is published by an external system
#	And our system subscribes to this event
#	When the subscriber function in our system publishes an output event
#	Then the output event data can be verified
