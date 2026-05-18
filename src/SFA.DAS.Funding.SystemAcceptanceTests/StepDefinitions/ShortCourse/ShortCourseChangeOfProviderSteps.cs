using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseChangeOfProviderSteps
{
    [Given("that a “short course” learner has been created by Provider (.*)")]
    [When("that a “short course” learner has been created by Provider (.*)")]
    public void GivenThatAShortCourseLearnerHasBeenCreatedByProviderA(string trainingProvider)
    {
        
    }

    [Given("the learner has not completed the course with Provider (.*)")]
    public void GivenTheLearnerHasNotCompletedTheCourseWithProviderA(string trainingProvider)
    {

    }

    [When("SLD informs us of the creation of the same learner/course by Provider (.*)")]
    public void WhenSLDInformsUsOfTheCreationOfTheSameLearnerCourseByProvider(string trainingProvider)
    {

    }

    [Then("notify approvals of this learner")]
    public void ThenNotifyApprovalsOfThisLearner()
    {

    }

}

/*



Standard Output: 
Begin Scenario Inform approvals when the same learner/course is created by different training providers
Given that a “short course” learner has been created by Provider A
-> No matching step definition found for the step. Use the following code to create one:
    [Given("that a “short course” learner has been created by Provider A")]
public void GivenThatAShortCourseLearnerHasBeenCreatedByProviderA()
{
_scenarioContext.Pending();
}

And the learner has not completed the course with Provider A
-> No matching step definition found for the step. Use the following code to create one:
    [Given("the learner has not completed the course with Provider A")]
public void GivenTheLearnerHasNotCompletedTheCourseWithProviderA()
{
_scenarioContext.Pending();
}

When SLD informs us of the creation of the same learner/course by Provider B (POST)
-> No matching step definition found for the step. Use the following code to create one:
    [When("SLD informs us of the creation of the same learner/course by Provider B \(POST\)")]
public void WhenSLDInformsUsOfTheCreationOfTheSameLearnerCourseByProviderBPOST()
{
_scenarioContext.Pending();
}

Then notify approvals of this learner
-> No matching step definition found for the step. Use the following code to create one:
    [Then("notify approvals of this learner")]
public void ThenNotifyApprovalsOfThisLearner()
{
_scenarioContext.Pending();
}

No matching step definition found for one or more steps.
using System;
using Reqnroll;

namespace MyNamespace
{
[Binding]
public class StepDefinitions
{
    private readonly ScenarioContext _scenarioContext;

    public StepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    [Given("that a “short course” learner has been created by Provider A")]
public void GivenThatAShortCourseLearnerHasBeenCreatedByProviderA()
{
_scenarioContext.Pending();
}

    [Given("the learner has not completed the course with Provider A")]
public void GivenTheLearnerHasNotCompletedTheCourseWithProviderA()
{
_scenarioContext.Pending();
}

    [When("SLD informs us of the creation of the same learner/course by Provider B \(POST\)")]
public void WhenSLDInformsUsOfTheCreationOfTheSameLearnerCourseByProviderBPOST()
{
_scenarioContext.Pending();
}

    [Then("notify approvals of this learner")]
public void ThenNotifyApprovalsOfThisLearner()
{
_scenarioContext.Pending();
}
}
}


*/