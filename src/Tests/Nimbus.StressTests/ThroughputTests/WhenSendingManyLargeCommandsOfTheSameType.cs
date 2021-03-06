﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyLargeCommandsOfTheSameType : ThroughputSpecificationForBus
    {
        protected override int NumMessagesToSend
        {
            get { return 1000; }
        }

        protected override int ExpectedMessagesPerSecond
        {
            get { return 100; }
        }

        public override IEnumerable<Task> SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend; i++)
            {
                var command = new FooCommand
                              {
                                  SomeMessage = new string(Enumerable.Range(0, 32*1024).Select(j => '.').ToArray()),
                              };
                yield return bus.Send(command);
                Console.Write(".");
            }
            Console.WriteLine();
        }
    }
}