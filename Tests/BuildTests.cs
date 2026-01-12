using NUnit.Framework;
using UnityEngine;

namespace Elfinik.BurstTrace.Tests
{
    public class BuildTests
    {
        [Test]
        public void ToProjectLink_InBuild_ReturnsSafeString()
        {
            var handle = TraceHandle.Capture();

            string link = handle.ToProjectLink();

            Assert.IsNotNull(link);
            Assert.IsNotEmpty(link);


            if (Application.isEditor)
            {
                StringAssert.Contains("href", link);
            }
            else
            {
                Debug.Log("Link in build: " + link);

                Assert.Pass("ToProjectLink executed without errors in Build");
            }
        }
    }
}