@nonparallelizable
Feature: GetProviderRefData

In order to keep DAS in sync with provider details reported in the ILR
As SLD
I want to know provider reference data such as employers and courses already on the apprenticeship service
So that DAS is kept up-to-date with latest data in the ILR

@regression
Scenario: Get Provider Reference Data
	When SLD want to know the provider reference data for a provider
	Then all provider reference data are returned in the response
