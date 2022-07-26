﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.Funding.SystemAcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("CalculateFundingLineTypes")]
    public partial class CalculateFundingLineTypesFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "CalculateFundingLineTypes.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "CalculateFundingLineTypes", "As a Finance Officer\r\nI want to know the funding line type for earnings\r\nSo that " +
                    "I can estimate the correct forecasted funding", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Calculate funding line types from Apprentice\'s age")]
        [NUnit.Framework.CategoryAttribute("regression")]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "15000", "614", "2008-08-01", "14", "16-18 Apprenticeship (Employer on App Service)", null)]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "15000", "614", "2005-08-01", "17", "16-18 Apprenticeship (Employer on App Service)", null)]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "15000", "177", "2004-08-01", "18", "16-18 Apprenticeship (Employer on App Service)", null)]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "15000", "177", "2003-08-01", "19", "19+ Apprenticeship (Employer on App Service)", null)]
        public void CalculateFundingLineTypesFromApprenticesAge(string start_Date, string planned_End_Date, string agreed_Price, string training_Code, string date_Of_Birth, string age_At_Course_Start, string funding_Line_Type, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "regression"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            string[] tagsOfScenario = @__tags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("start_date", start_Date);
            argumentsOfScenario.Add("planned_end_date", planned_End_Date);
            argumentsOfScenario.Add("agreed_price", agreed_Price);
            argumentsOfScenario.Add("training_code", training_Code);
            argumentsOfScenario.Add("date_of_birth", date_Of_Birth);
            argumentsOfScenario.Add("age_at_course_start", age_At_Course_Start);
            argumentsOfScenario.Add("funding_line_type", funding_Line_Type);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Calculate funding line types from Apprentice\'s age", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 9
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 10
 testRunner.Given(string.Format("an apprenticeship has a start date of {0}, a planned end date of {1}, an agreed p" +
                            "rice of {2}, and a training code {3}", start_Date, planned_End_Date, agreed_Price, training_Code), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 11
 testRunner.And(string.Format("the apprenticeship learner has a date of birth {0}", date_Of_Birth), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 12
 testRunner.When("the apprenticeship commitment is approved", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 13
 testRunner.Then(string.Format("the leaners age {0} at the start of the course and funding line type {1} must be " +
                            "calculated", age_At_Course_Start, funding_Line_Type), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
