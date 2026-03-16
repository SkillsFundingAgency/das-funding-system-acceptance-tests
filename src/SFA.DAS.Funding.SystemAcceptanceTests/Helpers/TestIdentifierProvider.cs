using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class TestIdentifierProvider
{
    private static bool _isInitialised = false;
    private static ConcurrentQueue<string> _ulnValues = new ConcurrentQueue<string>();
    private static ConcurrentQueue<long> _approvalsApprenticehipIdValues = new ConcurrentQueue<long>();
    private static HashSet<string> _dbUlns = new HashSet<string>();
    private static HashSet<long> _dbApprenticeshipIds = new HashSet<long>();
    private static readonly object _lock = new();
    private static readonly Random _random = new();

    public static List<string> Initialise(int numberOfUlns, string learningDbConnectionString)
    {
        numberOfUlns *= 2;
        lock (_lock)
        {
            if (_isInitialised)
                throw new Exception("TestUlnProvider has already been initialised.");

            var sqlServerClient = Sql.SqlServerClientProvider.GetSqlServerClient(learningDbConnectionString);

            var dbUlnsList = sqlServerClient.GetList<string>("SELECT Uln FROM [dbo].[Learner]");
            var dbApprenticeshipsList = sqlServerClient.GetList<long>("SELECT ApprovalsApprenticeshipId FROM [dbo].[ApprenticeshipLearning]");

            _dbUlns = new HashSet<string>(dbUlnsList);
            _dbApprenticeshipIds = new HashSet<long>(dbApprenticeshipsList);

            var ulns = new List<string>();

            for (int i = 0; i < numberOfUlns; i++)
            {
                ulns.Add(AddUniqueUln());
                AddUniqueApprovalsApprenticeshipId();
            }

            _isInitialised = true;

            return ulns;
        }
    }

    internal static string GetNextUln()
    {
        string? uln;

        if (_ulnValues.TryDequeue(out uln))
        {
            return uln;
        }

        throw new Exception("No more ULNs available. Please generate more.");
    }

    internal static long GetNextApprovalsApprenticeshipId()
    {
        long value;

        if (_approvalsApprenticehipIdValues.TryDequeue(out value))
        {
            return value;
        }

        throw new Exception("No more approvals apprenticeship ids available. Please generate more.");
    }

    private static string AddUniqueUln()
    {
        var uln = GenerateRandomUln();
        if (_ulnValues.Any(x => x == uln) || _dbUlns.Contains(uln))
        {
            return AddUniqueUln();
        }
        else
        {
            _ulnValues.Enqueue(uln);
            return uln;
        }
    }

    private static void AddUniqueApprovalsApprenticeshipId()
    {
        var approvalsApprenticeshipId = _random.Next(1000000, int.MaxValue);
        if(_approvalsApprenticehipIdValues.Any(x => x == approvalsApprenticeshipId) || _dbApprenticeshipIds.Contains(approvalsApprenticeshipId))
            AddUniqueApprovalsApprenticeshipId();
        else
        {
            _approvalsApprenticehipIdValues.Enqueue(approvalsApprenticeshipId);
        }
    }

    private static String GenerateRandomUln()
    {
        String randomUln = GenerateRandomNumberBetweenTwoValues(10, 99).ToString()
            + DateTime.Now.ToString("ssffffff");

        for (int i = 1; i < 30; i++)
        {
            if (IsValidCheckSum(randomUln))
            {
                return randomUln;
            }
            randomUln = (long.Parse(randomUln) + 1).ToString();
        }
        throw new Exception("Unable to generate ULN");
    }

    private static int GenerateRandomNumberBetweenTwoValues(int min, int max) => new Random().Next(min, max);

    private static bool IsValidCheckSum(string uln)
    {
        var ulnCheckArray = uln.ToCharArray()
                                .Select(c => long.Parse(c.ToString()))
                                .ToList();

        var multiplier = 10;
        long checkSumValue = 0;
        for (var i = 0; i < 10; i++)
        {
            checkSumValue += ulnCheckArray[i] * multiplier;
            multiplier--;
        }

        return checkSumValue % 11 == 10;
    }

}
