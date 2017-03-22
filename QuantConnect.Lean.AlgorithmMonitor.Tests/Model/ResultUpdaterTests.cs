using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Utils;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.Model
{
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
                                        Values = new List<TimeStampChartPoint>
                                        {
                                            new TimeStampChartPoint
                                            {
                                                X = TimeStamp.FromDays(100),
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
                                        Values = new List<TimeStampChartPoint>
                                        {
                                            new TimeStampChartPoint
                                            {
                                                X = TimeStamp.FromDays(100),
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
                                        Values = new List<TimeStampChartPoint>
                                        {
                                            new TimeStampChartPoint
                                            {
                                                X = TimeStamp.FromDays(100),
                                                Y = 100
                                            },
                                            new TimeStampChartPoint
                                            {
                                                X = TimeStamp.FromDays(101),
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
