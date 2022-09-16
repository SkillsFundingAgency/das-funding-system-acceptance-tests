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
    [NUnit.Framework.DescriptionAttribute("Calculate On program earnings based on funding band maximum")]
    public partial class CalculateOnProgramEarningsBasedOnFundingBandMaximumFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "CalculateOnProgramEarningsBasedOnFundingBandMaximum.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Calculate On program earnings based on funding band maximum", @"As the ESFA
I want the funding band maximum to be applied  
So we don’t overpay for apprenticeship funding

*** This feature is dependant on FundingBandMax value for the training code used *** 
*** Please do not change it without the consent of the team's testers ***


| Training Code | Earliest Start Date		| Latest End Date		  | Proposed Max Funding |
| 6				| 2014-11-12 00:00:00.000	| 2019-03-03 00:00:00.000 | 27000                |
| 6				| 2019-03-04 00:00:00.000	| NULL					  | 2600                 |", ProgrammingLanguage.CSharp, featureTags);
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
        [NUnit.Framework.DescriptionAttribute("On program earnings generation when agreed price is below funding band max")]
        [NUnit.Framework.CategoryAttribute("regression")]
        [NUnit.Framework.TestCaseAttribute("2018-08-01", "2019-07-31", "26100", "6", "1740", null)]
        public void OnProgramEarningsGenerationWhenAgreedPriceIsBelowFundingBandMax(string start_Date, string planned_End_Date, string agreed_Price, string training_Code, string instalment_Amount, string[] exampleTags)
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
            argumentsOfScenario.Add("instalment_amount", instalment_Amount);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("On program earnings generation when agreed price is below funding band max", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 16
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 17
 testRunner.Given(string.Format("an apprenticeship has a start date of {0}, a planned end date of {1}, an agreed p" +
                            "rice of {2}, and a training code {3}", start_Date, planned_End_Date, agreed_Price, training_Code), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 18
 testRunner.When("the apprenticeship commitment is approved", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 19
 testRunner.And("the agreed price is below the funding band maximum for the selected course", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 20
 testRunner.Then(string.Format("Agreed price is used to calculate the on-program earnings which is divided equall" +
                            "y into number of planned months {0}", instalment_Amount), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("On program earnings generation when agreed price is above funding band max")]
        [NUnit.Framework.CategoryAttribute("regression")]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "30000", "6", "1733.3333333333333333333333333", null)]
        public void OnProgramEarningsGenerationWhenAgreedPriceIsAboveFundingBandMax(string start_Date, string planned_End_Date, string agreed_Price, string training_Code, string instalment_Amount, string[] exampleTags)
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
            argumentsOfScenario.Add("instalment_amount", instalment_Amount);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("On program earnings generation when agreed price is above funding band max", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 27
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 28
 testRunner.Given(string.Format("an apprenticeship has a start date of {0}, a planned end date of {1}, an agreed p" +
                            "rice of {2}, and a training code {3}", start_Date, planned_End_Date, agreed_Price, training_Code), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 29
 testRunner.When("the apprenticeship commitment is approved", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 30
 testRunner.And("the agreed price is above the funding band maximum for the selected course", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 31
 testRunner.Then(string.Format("Funding band maximum price is used to calculate the on-program earnings which is " +
                            "divided equally into number of planned months {0}", instalment_Amount), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
