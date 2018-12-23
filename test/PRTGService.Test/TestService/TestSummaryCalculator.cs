using System;
using Xunit;
using PRTGService.Service;
using System.Xml.Linq;
using System.Linq;

namespace TestService
{
    public class TestSummaryCalculator
    {
        [Fact]
        public void testSummary()
        {
            // set up by loading in xml from file
            XElement dataFromFile = 
                XElement.Load(@"/Users/stefanevans/Development/reporter/services/prtg/sam-app/test/PRTGService.Test/TestService/PRTGTestInputXmlFile.xml");
            var itemsList = from item in dataFromFile.Elements() select item;

            StatsSummaryHolder stats = new StatsSummaryHolder();

            // test
            var summaryCalculator = new SummaryCalculator();
            stats = summaryCalculator.GetDataXml(stats, itemsList);

            var cpu = summaryCalculator.GetAverageFromSummary(stats, "CPU Utilization");
            Assert.Equal("0.56", cpu );

            var status = summaryCalculator.GetAverageFromSummary(stats, "Status (Ok)");
            Assert.Equal("100", status);

            var balance = summaryCalculator.GetAverageFromSummary(stats, "CPU Credit Balance");
            Assert.Equal("576", balance);

            var networkIn = summaryCalculator.GetAverageFromSummary(stats, "Network In");
            Assert.Equal("3.44", networkIn);

        }
    }
}
