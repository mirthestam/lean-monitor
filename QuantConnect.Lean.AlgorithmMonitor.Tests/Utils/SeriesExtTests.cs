using Monitor.Model.Charting;
using Monitor.Utils;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.Utils
{
    [TestFixture]
    public class SeriesExtTests
    {
        [Test]
        public void Since_AllNewData_ReturnsEverything()
        {
            // Arrange
            var series = new SeriesDefinition();
            series.Values.AddRange(new[]
            {
                new TimeStampChartPoint(TimeStamp.FromDays(100), 10),
                new TimeStampChartPoint(TimeStamp.FromDays(101), 20),
                new TimeStampChartPoint(TimeStamp.FromDays(102), 30),
                new TimeStampChartPoint(TimeStamp.FromDays(103), 40)
            });

            // Act
            var filteredSeries = series.Since(TimeStamp.MinValue);

            // Assert
            Assert.AreEqual(4, filteredSeries.Values.Count);
        }

        [Test]
        public void Since_Middle_ReturnsNewer()
        {
            // Arrange
            var series = new SeriesDefinition();
            series.Values.AddRange(new[]
            {
                new TimeStampChartPoint(TimeStamp.FromDays(100), 10),
                new TimeStampChartPoint(TimeStamp.FromDays(101), 20),
                new TimeStampChartPoint(TimeStamp.FromDays(102), 30),
                new TimeStampChartPoint(TimeStamp.FromDays(103), 40)
            });

            // Act
            var filteredSeries = series.Since(TimeStamp.FromDays(102));

            // Assert
            Assert.AreEqual(1, filteredSeries.Values.Count);
        }

        [Test]
        public void Since_Middle2_ReturnsNewer()
        {
            // Arrange
            var series = new SeriesDefinition();
            series.Values.AddRange(new[]
            {
                new TimeStampChartPoint(TimeStamp.FromSeconds(100), 10),
                new TimeStampChartPoint(TimeStamp.FromSeconds(101), 20),
                new TimeStampChartPoint(TimeStamp.FromSeconds(102), 30),
                new TimeStampChartPoint(TimeStamp.FromSeconds(103), 40)
            });

            // Act
            var filteredSeries = series.Since(TimeStamp.FromSeconds(101));

            // Assert
            Assert.AreEqual(2, filteredSeries.Values.Count);
        }

        [Test]
        public void Since_Later_ReturnsNone()
        {
            // Arrange
            var series = new SeriesDefinition();
            series.Values.AddRange(new[]
            {
                new TimeStampChartPoint(TimeStamp.FromSeconds(100), 10),
                new TimeStampChartPoint(TimeStamp.FromSeconds(101), 20),
                new TimeStampChartPoint(TimeStamp.FromSeconds(102), 30),
                new TimeStampChartPoint(TimeStamp.FromSeconds(103), 40)
            });

            // Act
            var filteredSeries = series.Since(TimeStamp.FromSeconds(104));

            // Assert
            Assert.AreEqual(0, filteredSeries.Values.Count);
        }
    }
}
