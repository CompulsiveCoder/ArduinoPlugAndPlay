using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Scripts
{
    [TestFixture]
    public class TransformServiceTemplateTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_TransformServiceTemplate ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Preparing transform service template test...");
            Console.WriteLine ("");

            var scriptName = "transform-service-template.sh";
            var serviceTemplateFile = "svc/arduino-plug-and-play.service.template";

            PullFileFromProject (scriptName, true);
            PullFileFromProject (serviceTemplateFile, false);

            var scriptPath = Path.GetFullPath (scriptName);

            var branch = new BranchDetector ().Branch;

            var destination = "mock";

            var cmd = "bash " + scriptPath + " " + branch + " " + destination;// + " " + smtpServer + " " + emailAddress;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            Console.WriteLine ("");
            Console.WriteLine ("Performing tranform service template script test...");
            Console.WriteLine ("");

            starter.Start (cmd);

            Console.WriteLine ("");
            Console.WriteLine ("Script output...");
            Console.WriteLine ("");

            Console.WriteLine (starter.Output);

            Assert.IsFalse (starter.IsError);
        }
    }
}

