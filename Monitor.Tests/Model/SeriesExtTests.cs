using Monitor.Model;
using Monitor.Model.Charting;
using NodaTime;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.Model
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
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)), 10),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)), 20),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(102)), 30),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(103)), 40)
            });

            // Act
            var filteredSeries = series.Since(Instant.MinValue);

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
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)), 10),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)), 20),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(102)), 30),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(103)), 40)
            });

            // Act
            var filteredSeries = series.Since(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(102)));

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
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)), 10),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)), 20),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(102)), 30),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(103)), 40)
            });

            // Act
            var filteredSeries = series.Since(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)));

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
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)), 10),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)), 20),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(102)), 30),
                new InstantChartPoint(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(103)), 40)
            });

            // Act
            var filteredSeries = series.Since(Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(104)));

            // Assert
            Assert.AreEqual(0, filteredSeries.Values.Count);
        }
    }
}
