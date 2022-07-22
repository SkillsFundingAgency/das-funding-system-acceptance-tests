Feature: Calculate earnings for learning payments

Scenario Outline: Earnings Generation
	Given An apprenticeship is created with <AgreedPrice>, <ActualStartDate>, <PlannedEndDate>
	Then Earnings results are published with calculated <AdjustedAgreedPrice>, <LearningAmount>, <NumberOfInstalments>, <FirstDeliveryPeriod>, <FirstCalendarPeriod>
	And correct Uln, EmployerId, ProviderId, TransferSenderEmployerId, StartDate, TrainingCode, EmployerType information

Examples: 
| AgreedPrice | ActualStartDate | PlannedEndDate | AdjustedAgreedPrice | LearningAmount | NumberOfInstalments | FirstDeliveryPeriod | FirstCalendarPeriod |
| 12000       | 2021-12-01      | 2022-12-01     | 9600                | 800            | 12                  | R05-2122            | 12/2021             |
