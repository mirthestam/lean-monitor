using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitor.Model.Charting;
using Monitor.Utils;
using Monitor.ViewModel.Charts;
using NUnit.Framework;

namespace QuantConnect.Lean.AlgorithmMonitor.Tests.ViewModel.Charts
{

    public class TestChartViewModel : ChartViewModelBase
    {
        public override bool CanClose { get; } = true;
        protected override void ZoomToFit()
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class ChartViewModelBaseTests
    {
        [Test]
        public void UpdateExistingOhlcPoints_Day_NoExisting_ModifiesNothing()
        {
            // Act
            var existingPoints = new List<TimeStampOhlcChartPoint>();

            var updatedPoints = new List<TimeStampOhlcChartPoint>
            {
                new TimeStampOhlcChartPoint
                {
                    X = TimeStamp.FromDays(100),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };

            // Arrange
            var model = new TestChartViewModel();
            model.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Monitor.Model.Resolution.Day);

            // assert
            Assert.AreEqual(1, updatedPoints.Count);
        }

        [Test]
        public void UpdateExistingOhlcPoints_Day_OlderExisting_ModifiesNothing()
        {
            // Act
            var existingPoints = new List<TimeStampOhlcChartPoint>()
                            {
                new TimeStampOhlcChartPoint
                {
                    X = TimeStamp.FromDays(99),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };


            var updatedPoints = new List<TimeStampOhlcChartPoint>
            {
                new TimeStampOhlcChartPoint
                {
                    X = TimeStamp.FromDays(100),
                    Open = 1000,
                    Low = 500,
                    High = 2000,
                    Close = 1500
                }
            };

            // Arrange
            var model = new TestChartViewModel();
            model.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Monitor.Model.Resolution.Day);

            // assert
            Assert.AreEqual(1, updatedPoints.Count);
        }

        [Test]
        public void UpdateExistingOhlcPoints_Day_SameDayExisting_ModifiesSameDay()
        {
            // Act
            var existingPoint = new TimeStampOhlcChartPoint
            {
                X = TimeStamp.FromDays(100),
                Open = 1000,
                Low = 500,
                High = 2000,
                Close = 1500
            };

            var existingPoints = new List<TimeStampOhlcChartPoint>()
                {existingPoint};        

            var updatedPoints = new List<TimeStampOhlcChartPoint>
            {
                new TimeStampOhlcChartPoint
                {
                    X = TimeStamp.FromDays(100),
                    Open = 1500,
                    Low = 250,
                    High = 2500,
                    Close = 2000
                }
            };

            // Arrange
            var model = new TestChartViewModel();
            model.UpdateExistingOhlcPoints(existingPoints, updatedPoints, Monitor.Model.Resolution.Day);

            // assert
            Assert.AreEqual(1000, existingPoint.Open); // Original open should be used
            Assert.AreEqual(250, existingPoint.Low); // Lowest low should be used
            Assert.AreEqual(2500, existingPoint.High); // Highest high should be used
            Assert.AreEqual(2000, existingPoint.Close); // Latest close should be used

            Assert.AreEqual(0, updatedPoints.Count); // Modified entry should be removed from updates
        }
    }
}