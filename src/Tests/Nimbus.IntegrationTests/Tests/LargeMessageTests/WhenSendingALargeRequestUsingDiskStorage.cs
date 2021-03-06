using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;
using Nimbus.LargeMessages.FileSystem.Configuration;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingALargeRequestUsingDiskStorage : SpecificationForAsync<Bus>
    {
        private BigFatResponse _response;
        private BigFatRequest _busRequest;
        private string _largeMessageBodyTempPath;

        protected override async Task<Bus> Given()
        {
            _largeMessageBodyTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Guid.NewGuid().ToString());

            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var logger = TestHarnessLoggerFactory.Create();

            var largeMessageBodyStorage = new FileSystemStorageBuilder().Configure()
                                                                        .WithStorageDirectory(_largeMessageBodyTempPath)
                                                                        .WithLogger(logger)
                                                                        .Build();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithLargeMessageStorage(c => c.WithLargeMessageBodyStore(largeMessageBodyStorage)
                                                                     .WithMaxSmallMessageSize(64*1024)
                                                                     .WithMaxLargeMessageSize(10*1048576))
                                      .WithDebugOptions(dc => dc.RemoveAllExistingNamespaceElementsOnStartup(
                                          "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start();
            return bus;
        }

        protected override async Task When()
        {
            var bigQuestion = new string(Enumerable.Range(0, 524288).Select(i => '.').ToArray());

            _busRequest = new BigFatRequest
                          {
                              SomeBigQuestion = bigQuestion,
                          };
            _response = await Subject.Request(_busRequest);
        }

        [Test]
        public async Task TheResponseShouldReturnUnscathed()
        {
            _response.SomeBigAnswer.Length.ShouldBe(BigFatRequestHandler.MessageSize);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (Directory.Exists(_largeMessageBodyTempPath)) Directory.Delete(_largeMessageBodyTempPath, true);
        }
    }
}