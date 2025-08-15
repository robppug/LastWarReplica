using NUnit.Framework;
using UnityEngine;

public class SmokeTests
{
    [Test] public void ProjectLoads() { Assert.IsTrue(Application.isEditor); }
}
