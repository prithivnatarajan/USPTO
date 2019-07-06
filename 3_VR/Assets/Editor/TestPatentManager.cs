using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using Object = UnityEngine.Object;
using Random = System.Random;

public class TestPatentManager
{
//    private Object _patentPrefab = Resources.Load("PatentPrefab");
    private const int MinYear = 1990;
    private const int MaxYear = 2018;

    private static List<Patent> GetTestPatents()
    {
        var testPatents = new List<Patent>
        {
            new Patent("04-09-1994", 0.5f, 0.5f, 2f, "test_patent_id_1", 0,
                new List<string>(), 
                new Dictionary<string, string>(),
                new SortedList<DateTime, int>()
                {
                    {DateTime.Parse("04-10-1995"), 1},
                    {DateTime.Parse("05-13-1996"), 2},
                    {DateTime.Parse("09-18-1997"), 3},
                    {DateTime.Parse("02-01-1998"), 5},
                    {DateTime.Parse("04-07-1999"), 7},
                    {DateTime.Parse("08-27-2000"), 9},
                    {DateTime.Parse("07-16-2001"), 10},
                    {DateTime.Parse("08-10-2002"), 12},
                    {DateTime.Parse("08-11-2002"), 13},
                    {DateTime.Parse("09-01-2002"), 14},
                    {DateTime.Parse("09-02-2002"), 16},
                }),
            new Patent("10-01-1992", 0, 0, 0, "test_patent_id_2", 1,
                new List<string>(),
                new Dictionary<string, string>(),
                GetRandomCitations(100))
        };


        testPatents.ForEach(p => p.Size = 1f);
        return testPatents;
    }

    private static SortedList<DateTime, int> GetRandomCitations(int numCitations)
    {
        var randomCitations = new SortedList<DateTime, int>();
        var rng = new Random();
        var currentDateTime = new DateTime(MinYear, 1, 1);
        var curNumCitations = 0;
        for (var i = 0; i < numCitations; i++)
        {
            curNumCitations += rng.Next(1, 20);
            currentDateTime = currentDateTime.AddDays(rng.Next(1, 28));
            randomCitations.Add(currentDateTime, curNumCitations);
        }

        return randomCitations;
    }

    private static void TestPatentCitationSize(Patent patent, int dayIncrement, int monthIncrement, int yearIncrement)
    {
        var prevMonth = new DateTime();
        //patent.SetCurrentTime(prevMonth);

        for (var currentDateTime = new DateTime(MinYear, 1, 1);
            currentDateTime < new DateTime(MaxYear, 1, 1);
            currentDateTime = currentDateTime.AddDays(dayIncrement).AddMonths(monthIncrement).AddYears(yearIncrement))
        {
            var prevSize = patent.Size;
            // patent.SetCurrentTime(currentDateTime);
            var newSize = patent.Size;
            if (patent.allCitations.Keys
                .Any(d => d <= currentDateTime && d > prevMonth))
            {
                Assert.Greater(newSize, prevSize, string.Format("currentDateTime is {0}", currentDateTime));
            }
            else
            {
                Assert.AreEqual(newSize, prevSize, string.Format("currentDateTime is {0}", currentDateTime));
            }

            prevSize = newSize;
            prevMonth = currentDateTime;
        }
    }


    [Test]
    public void NewEditModeTestSimplePasses()
    {
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator NewEditModeTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
        var testPatents = GetTestPatents();
        Assert.AreEqual(testPatents[0].dateGranted, new DateTime(1994, 4, 9));
        Assert.AreEqual(testPatents[1].dateGranted, new DateTime(1992, 10, 1));
    }

    [UnityTest]
    public IEnumerator TestPatentsByMonth()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
        var testPatents = GetTestPatents();
        TestPatentCitationSize(testPatents[0], 0, 1, 0);
    }

    [UnityTest]
    public IEnumerator TestPatentsByDay()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
        foreach (var patent in GetTestPatents())
        {
            TestPatentCitationSize(patent, 1, 0, 0);
        }
    }

    [UnityTest]
    public IEnumerator TestPatentCitationsRandomly()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
        var rng = new Random();
        for (var i = 0; i < 10; i++)
        {
            foreach (var patent in GetTestPatents())
            {
                var dayInc = rng.Next(27);
                var monthInc = rng.Next(11);
                var yearInc = rng.Next(5);
                if (dayInc + monthInc + yearInc == 0)
                {
                    continue;
                }

                TestPatentCitationSize(patent, dayInc, monthInc, yearInc);
            }
        }
    }
}