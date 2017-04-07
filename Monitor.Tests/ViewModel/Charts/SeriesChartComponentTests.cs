using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model.Charting;
using Monitor.ViewModel.Charts;
using NodaTime;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.ViewModel.Charts
{
    [TestFixture]
    public class SeriesChartComponentTests
    {
        [Test]
        public void UpdateExistingOhlcPoints_Day_NoExisting_ModifiesNothing()
        {
            // Act
            var existingPoints = new List<OhlcInstantChartPoint>();

            var updatedPoints = new List<OhlcInstantChartPoint>
            {
                new OhlcInstantChartPoint
                {
                    X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };

            // Arrange
            var component = new SeriesChartComponent(null);
            component.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Resolution.Daily);

            // assert
            Assert.AreEqual(1, updatedPoints.Count);
        }

        [Test]
        public void UpdateExistingOhlcPoints_Day_OlderExisting_ModifiesNothing()
        {
            // Act
            var existingPoints = new List<OhlcInstantChartPoint>()
                            {
                new OhlcInstantChartPoint
                {
                    X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(99)),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };


            var updatedPoints = new List<OhlcInstantChartPoint>
            {
                new OhlcInstantChartPoint
                {
                    X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };

            // Arrange
            var component = new SeriesChartComponent(null);
            component.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Resolution.Daily);

            // assert
            Assert.AreEqual(1, updatedPoints.Count);
        }

        [Test]
        public void UpdateExistingOhlcPoints_Day_SameDayExisting_ModifiesSameDay()
        {
            // Act
            var existingPoint = new OhlcInstantChartPoint
            {
                X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                Open = 1000,
                Low = 500,
                High = 2000,
                Close = 1500
            };

            var existingPoints = new List<OhlcInstantChartPoint>()
                {existingPoint};

            var updatedPoints = new List<OhlcInstantChartPoint>
            {
                new OhlcInstantChartPoint
                {
                    X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                    Open = 1500,
                    Low = 250,
                    High = 2500,
                    Close = 2000
                }
            };

            // Arrange
            var component = new SeriesChartComponent(null);
            component.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Resolution.Daily);

            // assert
            Assert.AreEqual(1000, existingPoint.Open); // Original open should be used
            Assert.AreEqual(250, existingPoint.Low); // Lowest low should be used
            Assert.AreEqual(2500, existingPoint.High); // Highest high should be used
            Assert.AreEqual(2000, existingPoint.Close); // Latest close should be used

            Assert.AreEqual(0, updatedPoints.Count); // Modified entry should be removed from updates
        }

        [Test]
        public void DetectResolution_Daily_ReturnsDaily()
        {
            // Arrange
            var series = new SeriesDefinition
            {
                SeriesType = Monitor.Model.Charting.SeriesType.Line,
                Values = new List<InstantChartPoint>
                {
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    },
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 02, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    }
                }
            };

            // Act
            var resolution = SeriesChartComponent.DetectResolution(series);

            // Assert
            Assert.AreEqual(Resolution.Daily, resolution);
        }

        [Test]
        public void DetectResolution_Hourly_ReturnsHour()
        {
            // Arrange
            var series = new SeriesDefinition
            {
                SeriesType = Monitor.Model.Charting.SeriesType.Line,
                Values = new List<InstantChartPoint>
                {
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    },
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 1, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    }
                }
            };

            // Act
            var resolution = SeriesChartComponent.DetectResolution(series);

            // Assert
            Assert.AreEqual(Resolution.Hour, resolution);
        }

        [Test]
        public void DetectResolution_Minute_ReturnsMinute()
        {
            // Arrange
            var series = new SeriesDefinition
            {
                SeriesType = Monitor.Model.Charting.SeriesType.Line,
                Values = new List<InstantChartPoint>
                {
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    },
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 1, 0, DateTimeKind.Utc)),
                        Y = 100
                    }
                }
            };

            // Act
            var resolution = SeriesChartComponent.DetectResolution(series);

            // Assert
            Assert.AreEqual(Resolution.Minute, resolution);
        }

        [Test]
        public void DetectResolution_Second_ReturnsSecond()
        {
            // Arrange
            var series = new SeriesDefinition
            {
                SeriesType = Monitor.Model.Charting.SeriesType.Line,
                Values = new List<InstantChartPoint>
                {
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    },
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 1, DateTimeKind.Utc)),
                        Y = 100
                    }
                }
            };

            // Act
            var resolution = SeriesChartComponent.DetectResolution(series);

            // Assert
            Assert.AreEqual(Resolution.Second, resolution);
        }

        [Test]
        public void DetectResolution_Tick_ReturnsTicks()
        {
            // Arrange
            var series = new SeriesDefinition
            {
                SeriesType = Monitor.Model.Charting.SeriesType.Line,
                Values = new List<InstantChartPoint>
                {
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc)),
                        Y = 100
                    },
                    new InstantChartPoint
                    {
                        X = Instant.FromDateTimeUtc(new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(0.1)),
                        Y = 100
                    }
                }
            };

            // Act
            var resolution = SeriesChartComponent.DetectResolution(series);

            // Assert
            Assert.AreEqual(Resolution.Tick, resolution);
        }
    }
}