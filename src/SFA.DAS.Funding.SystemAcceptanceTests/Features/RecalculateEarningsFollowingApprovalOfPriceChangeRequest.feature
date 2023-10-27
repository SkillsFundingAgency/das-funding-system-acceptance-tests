Feature: RecalculateEarningsFollowingApprovalOfPriceChangeRequest

As a Provider
I want my earnings to reflect approved changes to the total price
So that I am paid the correct amount

@regression
Scenario: Price change approved in the year it was requested, below or at funding band max; recalc earnings
	Given earnings have been calculated for an apprenticeship in the pilot
	And the total price is below or at the funding band maximum
	And a price change request was sent before the end of R14 of the current academic year
	And the price change request is for a new total price up to or at the funding band maximum
	When the change is approved by the other party before the end of year X
	Then the earnings are recalculated based on the new price
	And the history of old and new earnings is maintained



