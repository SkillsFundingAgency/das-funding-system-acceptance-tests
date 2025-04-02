## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# das-funding-system-acceptance-tests

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-funding-system-acceptance-tests?branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3217&branchName=main)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/jira/software/c/projects/FLP/boards/753)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3480354918/Flexible+Payments+Models)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

das-funding-system-acceptance-tests is a repository which runs system acceptance tests against funding-earnings and funding-payments.


## How It Works

This is a dotnet project containing specflow tests. Although the subject of the tests are funding-earnings and funding-payments, the data inserted into those domains will come via other apps (such as das-apprenticeships). Most of the triggering is done via azure service bus events, although some are direct http calls.

## 🚀 Installation

### Pre-Requisites

* A clone of this repository
* A code editor that supports .Net8

### Config

Within the SFA.Das.Funding.SystemAcceptanceTests project is a appsettings.ProjectConfig.json file. This should be populated with the configuration values for the environment being tested

### Running the tests

The tests can be triggered in visual studio by running tests as you would with any unit test. You will also need to white list your ip address for the sql server database in azure for the enviroment you are trying to test.