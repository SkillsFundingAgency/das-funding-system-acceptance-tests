namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ShortCourseStepDefinitions(ScenarioContext scenarioContext)
    {
        [When(@"SLD inform us of a new Short Course")]
        public void WhenSLDInformUsOfANewShortCourse()
        {
            throw new PendingStepException();
        }

        [Then(@"the Short Course details are recorded in Learning")]
        public void ThenTheShortCourseDetailsAreRecordedInLearning()
        {
            throw new PendingStepException();
        }

        [Then(@"a LearnerData event is published to approvals")]
        public void ThenALearnerDataEventIsPublishedToApprovals()
        {
            throw new PendingStepException();
        }

    }
}
