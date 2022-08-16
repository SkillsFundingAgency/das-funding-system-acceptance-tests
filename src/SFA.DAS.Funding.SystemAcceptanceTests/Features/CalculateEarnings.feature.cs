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
    [NUnit.Framework.DescriptionAttribute("Calculate earnings for an approved apprenticeship")]
    public partial class CalculateEarningsForAnApprovedApprenticeshipFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
#line 1 "CalculateEarnings.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Calculate earnings for an approved apprenticeship", "As a Training provider\r\nI want monthly on-program earnings to be calculated \r\nSo " +
                    "they feed into payments calculation I get paid", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Earnings Generation for an approved apprenticeship")]
        [NUnit.Framework.CategoryAttribute("regression")]
        [NUnit.Framework.TestCaseAttribute("2022-08-01", "2023-07-31", "15,000", "12", "1000", "01-2223", "08/2022", null)]
        public virtual void EarningsGenerationForAnApprovedApprenticeship(string start_Date, string planned_End_Date, string agreed_Price, string planned_Number_Of_Months, string instalment_Amount, string first_Delivery_Period, string first_Calendar_Period, string[] exampleTags)
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
            argumentsOfScenario.Add("planned_number_of_months", planned_Number_Of_Months);
            argumentsOfScenario.Add("instalment_amount", instalment_Amount);
            argumentsOfScenario.Add("first_delivery_period", first_Delivery_Period);
            argumentsOfScenario.Add("first_calendar_period", first_Calendar_Period);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Earnings Generation for an approved apprenticeship", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 8
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 9
 testRunner.Given(string.Format("an apprenticeship has a start date of {0}, a planned end date of {1}, and an agre" +
                            "ed price of £{2}", start_Date, planned_End_Date, agreed_Price), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 10
 testRunner.When("the apprenticeship commitment is approved", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 11
 testRunner.Then(string.Format("80% of the agreed price is calculated as total on-program payment which is divide" +
                            "d equally into number of planned months {0}", instalment_Amount), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 12
 testRunner.And(string.Format("the planned number of months must be the number of months from the start date to " +
                            "the planned end date {0}", planned_Number_Of_Months), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "Delivery Period",
                            "Academic Year",
                            "Calendar Period"});
                table1.AddRow(new string[] {
                            "1",
                            "2223",
                            "August"});
                table1.AddRow(new string[] {
                            "2",
                            "2223",
                            "September"});
                table1.AddRow(new string[] {
                            "3",
                            "2223",
                            "October"});
                table1.AddRow(new string[] {
                            "4",
                            "2223",
                            "November"});
                table1.AddRow(new string[] {
                            "5",
                            "2223",
                            "December"});
                table1.AddRow(new string[] {
                            "6",
                            "2223",
                            "January"});
                table1.AddRow(new string[] {
                            "7",
                            "2223",
                            "February"});
                table1.AddRow(new string[] {
                            "8",
                            "2223",
                            "March"});
                table1.AddRow(new string[] {
                            "9",
                            "2223",
                            "April"});
                table1.AddRow(new string[] {
                            "10",
                            "2223",
                            "May"});
                table1.AddRow(new string[] {
                            "11",
                            "2223",
                            "June"});
                table1.AddRow(new string[] {
                            "12",
                            "2223",
                            "July"});
#line 13
 testRunner.And("the delivery period for each instalment must be the delivery period from the coll" +
                        "ection calendar with a matching calendar month/year", ((string)(null)), table1, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion