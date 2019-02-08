using System;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using NSubstitute;
using NUnit.Framework;

namespace GitBack.Credential.Manager.Tests
{
    public class InputOutputManagerTests
    {
        private IConsole _console;
        private TextWriter _errorWriter;

        [SetUp]
        public void BeforeEach()
        {
            _console = Substitute.For<IConsole>();
            _errorWriter = new StringWriter();
            _console.Error.Returns(_errorWriter);
            _console.Out.Returns((callInfo) => throw new ApplicationException("InputOutputManager should not call _console.Out."));
        }

        [Test]
        public void RemovesConsoleAppenders_When_removeAppenders_IsTrue()
        {
            const bool removeAppenders = true;
            const string logName = nameof(RemovesConsoleAppenders_When_removeAppenders_IsTrue);
            var logger = SetupLogger(logName);
            var subject = new InputOutputManager(removeAppenders, _console) { WriteWarnOnErrorString = true };

            var actualLogger = subject.GetLogger(logName);

            var parentAppenders = logger.Parent.Appenders.OfType<ConsoleAppender>().ToList();
            Assert.Multiple(() =>
            {
                Assert.That(actualLogger, Is.Not.Null);
                Assert.That(parentAppenders, Is.Empty);
                Assert.That(_errorWriter.ToString(), Is.Not.Empty);
            });
        }



        [Test]
        public void DoesNotRemoveConsoleAppenders_When_removeAppenders_IsFalse()
        {
            const bool removeAppenders = false;
            const string logName = nameof(DoesNotRemoveConsoleAppenders_When_removeAppenders_IsFalse);
            var logger = SetupLogger(logName);

            var subject = new InputOutputManager(removeAppenders, _console);
            var actualLogger = subject.GetLogger(logName);

            var parentAppenders = logger.Parent.Appenders.OfType<ConsoleAppender>().ToList();
            Assert.Multiple(() =>
            {
                Assert.That(actualLogger, Is.Not.Null);
                Assert.That(parentAppenders, Is.Not.Empty);
                Assert.That(_errorWriter.ToString(), Is.Empty);
            });
        }


        private static Logger SetupLogger(string logName)
        {
            if (string.IsNullOrEmpty(logName)) { logName = Guid.NewGuid().ToString(); }

            var log = LogManager.GetLogger(logName);
            var parentLog = LogManager.GetLogger("Parent.Of." + logName);

            var logger = log.Logger as Logger;
            logger.Parent = parentLog.Logger as Logger;

            var consoleAppender = new ConsoleAppender();
            if (!logger.Parent.Appenders.OfType<ConsoleAppender>().Any())
            {
                logger.Parent.AddAppender(consoleAppender);
            }
            Assume.That(logger.Parent.Appenders, Contains.Item(consoleAppender));
            Assume.That(logger.Parent.Appenders.OfType<ConsoleAppender>(), Is.Not.Empty);

            return logger;
        }

    }
}
