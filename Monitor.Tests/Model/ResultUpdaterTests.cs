using System.Collections.Generic;
using Monitor.Model;
using Monitor.Model.Charting;
using NodaTime;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.Model
{
    using Result = Monitor.Model.Result;

    [TestFixture]
    public class ResultUpdaterTests
    {
        /// <summary>
        /// This test ensures, new charts, series and their values get added when they are new
        /// </summary>
        [Test]
        public void MergeCharts_EmptyCharts_GetsMerged()
        {
            // Arrange
            var target = new Result();

            var source = new Result
            {
                Charts = new Dictionary<string, ChartDefinition>
                {
                    {
                        "chart", new ChartDefinition
                        {
                            Series = new Dictionary<string, SeriesDefinition>
                            {
                                {
                                    "series", new SeriesDefinition
                                    {
                                        Values = new List<InstantChartPoint>
                                        {
                                            new InstantChartPoint
                                            {
                                                X = Instant.FromUnixTimeSeconds(60 * 60 * 24 * 100), // 100 days
                                                Y = 100
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            ResultUpdater.Merge(target, source);

            // Assert
            Assert.AreEqual(1, target.Charts["chart"].Series["series"].Values.Count);
        }

        /// <summary>
        /// This test makes sure, only new updates (appended data) will be read from a series
        /// </summary>
        [Test]
        public void MergeCharts_SameSeriesUpdatedData_GetsMerged()
        {
            // Arrange
            var target = new Result
            {
                Charts = new Dictionary<string, ChartDefinition>
                {
                    {
                        "chart", new ChartDefinition
                        {
                            Series = new Dictionary<string, SeriesDefinition>
                            {
                                {
                                    "series", new SeriesDefinition
                                    {
                                        Values = new List<InstantChartPoint>
                                        {
                                            new InstantChartPoint
                                            {
                                                X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                                                Y = 100
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var source = new Result
            {
                Charts = new Dictionary<string, ChartDefinition>
                {
                    {
                        "chart", new ChartDefinition
                        {
                            Series = new Dictionary<string, SeriesDefinition>
                            {
                                {
                                    "series", new SeriesDefinition
                                    {
                                        Values = new List<InstantChartPoint>
                                        {
                                            new InstantChartPoint
                                            {
                                                X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(100)),
                                                Y = 100
                                            },
                                            new InstantChartPoint
                                            {
                                                X = Instant.Add(Instant.FromUnixTimeTicks(0), Duration.FromDays(101)),
                                                Y = 100
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            // Act
            ResultUpdater.Merge(target, source);

            // Assert
            Assert.AreEqual(2, target.Charts["chart"].Series["series"].Values.Count);
        }
    }
}
