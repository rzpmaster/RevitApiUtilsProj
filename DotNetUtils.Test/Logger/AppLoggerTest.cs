using DotNetUtils.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Test.Logger
{
    [TestFixture]
    public class AppLoggerTest
    {
        [Test]
        public void LogShould()
        {
            AppLogger.Log();
            AppLogger.Log("Hello World!");
        }
    }
}
