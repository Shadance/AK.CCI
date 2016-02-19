using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using AK.CCI.Service.Indicators;
using AK.CCI.Service.Settings;
using AK.CCI.Service.Strategies;
using Ecng.Common;
using log4net;
using log4net.Appender;
using Ninject;
using StockSharp.Algo;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Quik;
using StockSharp.Xaml;
using StockSharp.Localization;

using LogManager = log4net.LogManager;

namespace AK.CCI.Service
{
	public class CCIStrategy : AKStrategy
    {
		[Inject]
		public CommodityChannelIndexExtended Indicator { get; set; }

        public CCIStrategy()
        {
        }

		protected override void OnStarted()
		{
            Logger.Info("Configuring CCIStrategy.");
            base.OnStarted();
/*
            if (_area == null)
            {
                _area = new ChartArea();

                _area.Elements.Add(new ChartCandleElement());
                _area.Elements.Add(new ChartIndicatorElement { Color = ColoredConsoleAppender.Colors.Red, StrokeThickness = 1 });
                _area.Elements.Add(new ChartTradeElement());
                new ChartAddAreaCommand(_area).Process(this);
            }
*/
        }

        protected override void OnReseted()
        {
            Indicator.Reset();
            base.OnReseted();
        }
        protected override void ProcessFinishedCandle(Candle candle)
		{
			Indicator.Process(candle);

			Logger.DebugFormat("Indicator.Container.Count {0}", Indicator.Container.Count);
			if (Indicator.IsFormed)
			{
				Logger.DebugFormat("Indicator.GetCurrentValue {0}", Indicator.GetCurrentValue());
			}

			Logger.DebugFormat("Position {0}", Position);
		}

		protected override void ProcessCandle(Candle candle)
		{
			Indicator.Process(candle);

			if (Indicator.IsBarCrossedOnLastValue == null)
			{
				return;
			}

			//Logger.WarnFormat("IndicatorOnBarCrossed {0} (Prev: {1}, Last: {2})", Indicator.IsBarCrossedOnLastValue.Side, Indicator.IsBarCrossedOnLastValue.PrevIndicatorValue, Indicator.IsBarCrossedOnLastValue.LastIndicatorValue);
			if (Orders.Any(o => o.State == OrderStates.Active || o.State == OrderStates.Pending || o.State == OrderStates.None))
			{
				Logger.DebugFormat("Blocks Orders because: State");
				return;
			}

			if (Orders.Any(o => o.Time.Add(CandleTimeFrame) > CurrentTime))
			{
				Logger.DebugFormat("Blocks Orders because: Last Order Time");
				return;
			}

			var whenCrossedZBar = Indicator.WhenCrossedZBar;
            if (whenCrossedZBar > Indicator.Length)
			{
				Logger.DebugFormat("Blocks Orders because: Indicator.WhenCrossedZBar ({0})", whenCrossedZBar);
				return;
			}

			var newOrder = Indicator.IsBarCrossedOnLastValue.Side == Sides.Buy
				? this.BuyAtLimit(candle.ClosePrice)
				: this.SellAtLimit(candle.ClosePrice);

			Logger.WarnFormat("New Order: {0}", newOrder);

            /*            newOrder
                            .WhenNewTrades(SafeGetConnector())
                            .Do(OnNewOrderTrades)
                            .Apply(this);
            */
            if (newOrder != null)
                RegisterOrder(newOrder);
/*
            new ChartDrawCommand(candle.OpenTime, new Dictionary<IChartElement, object>
            {
                { _area.Elements[0], candle },
                { _area.Elements[1], Indicator.GetCurrentValue() },
            }).Process(this);
*/
    }

	}
}