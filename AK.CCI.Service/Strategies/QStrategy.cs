using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AK.CCI.Service.Indicators;
using AK.CCI.Service.Settings;
using Ecng.Collections;
using log4net;
using Ninject;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Strategies.Protective;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Quik;
using LogManager = log4net.LogManager;

namespace AK.CCI.Service
{
	public class QStrategy : Strategy, IStrategy
	{
		private static readonly ILog Logger = LogManager.GetLogger("AK.CCI.Service");

		private readonly IConnectorManager _connectorManager;
		private readonly IStrategyConfiguration _strategyConfiguration;

		private Security _security;
		private Portfolio _portfolio;

		private CandleSeries _series;

		protected ManualResetEvent PortfolioFoundEvent = new ManualResetEvent(false);
		protected ManualResetEvent SecurityFoundEvent = new ManualResetEvent(false);

		private Timer _ordersCheckTimer;

		public QStrategy(IConnectorManager connectorManager, IStrategyConfiguration strategyConfiguration)
		{
			_connectorManager = connectorManager;
			_strategyConfiguration = strategyConfiguration;

			var trader = _connectorManager.Trader;

			if (_portfolio == null)
			{
				if (trader.Portfolios != null && trader.Portfolios.Any())
				{
					LookupPortfolio(trader.Portfolios);
				}
			}

			if (_portfolio == null)
			{
				trader.NewPortfolios += portfolios =>
				{
					if (_portfolio == null)
					{
						LookupPortfolio(portfolios);
					}
				};
			}

			if (_security == null)
			{
				if (trader.Securities != null && trader.Securities.Any())
				{
					LookupSecurity(trader.Securities);
				}
			}

			if (_security == null)
			{
				trader.NewSecurities += securities =>
				{
					if (_security == null)
					{
						LookupSecurity(securities);
					}
				};
			}

			Log += OnLog;
		}

		private void OnLog(LogMessage message)
		{
			if (message.Level > LogLevels.Debug)
			{
				Logger.InfoFormat("[{0}] {1}", message.Source, message.Message);
			}
		}

		private void LookupPortfolio(IEnumerable<Portfolio> portfolios)
		{
			_portfolio = portfolios.FirstOrDefault(port => port.Name == _strategyConfiguration.PortfolioName);
			if (_portfolio != null)
			{
				PortfolioFoundEvent.Set();
			}
		}

		private void LookupSecurity(IEnumerable<Security> securities)
		{
			_security = securities.FirstOrDefault(sec => sec.Code == _strategyConfiguration.SecurityCode);
			if (_security != null)
			{
				SecurityFoundEvent.Set();
			}
		}

		protected override void OnStarted()
		{
			Logger.Info("Waiting for TraderConnected event.");
			_connectorManager.TraderConnectedEvent.WaitOne();

			Connector = _connectorManager.Trader;

			Logger.Info("Waiting for PortfolioFoundEvent event.");
			PortfolioFoundEvent.WaitOne();

			Portfolio = _portfolio;

			Logger.Info("Waiting for SecurityFound event.");
			SecurityFoundEvent.WaitOne();

			Logger.Info("Configuring QStrategy.");

			Security = _security;
			Volume = _strategyConfiguration.Volume;

			_ordersCheckTimer = new Timer(CheckOrdersElapsed, null, 0, _strategyConfiguration.OrdersCheckInterval.Milliseconds);

			_series = new CandleSeries(typeof(TimeFrameCandle), _security, _strategyConfiguration.CandleTimeFrame)
			{
				From = CurrentTime - TimeSpan.FromTicks(_strategyConfiguration.CandleTimeFrame.Ticks * _strategyConfiguration.IndicatorLength)
			};

			_connectorManager.CandleManager.Start(_series);

			Security
				.WhenChanged(Connector)
				.Do(ProcessSecurityChange)
				.Apply(this);

			_series
				.WhenCandlesChanged()
				.Do(ProcessCandle)
				.Apply(this);

			base.OnStarted();
		}

		protected override void OnError(Exception error)
		{
			Logger.Error(error);
			base.OnError(error);
		}

		protected virtual void CheckOrdersElapsed(object stateInfo)
		{
			var now = this.CurrentTime;
			foreach (var order in Orders.Where(o => o.IsMatchedEmpty()))
			{
				if (order.Time.Add(_strategyConfiguration.OrderExpirationTimeSpan) < now)
				{
					CancelOrder(order);
				}
			}
		}

		protected virtual void ProcessSecurityChange()
		{
			if (Security.LastTrade.Time < this.StartedTime)
			{
				if (Security.LastTrade.Time.Minute == 0 && Security.LastTrade.Time.Second == 0)
				{
					Logger.DebugFormat("Loading Trades ... {0}", Security.LastTrade.Time);
				}
			}
		}

		protected virtual void ProcessCandle(Candle candle)
		{
			if (Orders.Any(o => o.State == OrderStates.Active || o.State == OrderStates.Pending || o.State == OrderStates.None))
			{
				//Logger.DebugFormat("Blocks Orders because: State");
				return;
			}

			var timeout = TimeSpan.FromMilliseconds(_strategyConfiguration.CandleTimeFrame.TotalMilliseconds*6);
			if (Orders.Any(o => o.Time.Add(timeout) > CurrentTime))
			{
				//Logger.DebugFormat("Blocks Orders because: Last Order Time");
				return;
			}

			TimeFrameCandle lastCandle = _series.GetCandle<TimeFrameCandle>(1);
			if (lastCandle == null)
			{
				//Logger.DebugFormat("Can't get last candle yet.");
				return;
			}

			// hack to avoid analyzing obsolete candles
			if (candle.ClosePrice == lastCandle.ClosePrice
				|| candle.CloseTime.Add(_strategyConfiguration.CandleTimeFrame) < CurrentTime)
			{
				return;
			}

			Sides direction;
			decimal targetPrice;
			bool isOrderAllowed = false;

			if (candle.ClosePrice > lastCandle.ClosePrice)
			{
				direction = Sides.Buy;
				targetPrice = candle.ClosePrice + _strategyConfiguration.TakeProfitLevel;

				if (lastCandle.HighPrice > targetPrice)
				{
					isOrderAllowed = true;
				}
			}
            else
            {
	            direction = Sides.Sell;
				targetPrice = candle.ClosePrice - _strategyConfiguration.TakeProfitLevel;

				if (lastCandle.LowPrice < targetPrice)
				{
					isOrderAllowed = true;
				}
			}

			if (!isOrderAllowed)
			{
				return;
			}
			
			var newOrder = direction == Sides.Buy
				? this.BuyAtLimit(candle.ClosePrice)
				: this.SellAtLimit(candle.ClosePrice);

			Logger.WarnFormat("New Order: {0}", newOrder);

			newOrder
				.WhenNewTrades()
				.Do(OnNewOrderTrades)
				.Apply(this);

			RegisterOrder(newOrder);
		}

		protected virtual void OnNewOrderTrades(IEnumerable<MyTrade> trades)
		{
			foreach (var t in trades)
			{
				if (t.Order.Type == OrderTypes.Conditional)
				{
					continue;
				}

				var stopLossLevel = _strategyConfiguration.StopLossLevel;
				var takeProfitLevel = _strategyConfiguration.TakeProfitLevel;
				var takeProfitOffset = _strategyConfiguration.TakeProfitOffset;

				var direction = (t.Order.Direction == Sides.Buy) ? Sides.Sell : Sides.Buy;

				var order = new Order
				{
					Type = OrderTypes.Conditional,
					Volume = t.Order.Volume,
					//Price = stopLossLevel,
					Direction = direction,
					Security = t.Order.Security,
					Condition = new QuikOrderCondition
					{
						Type = QuikOrderConditionTypes.TakeProfitStopLimit,
						StopPrice = (direction == Sides.Sell) ? t.Order.Price + takeProfitLevel : t.Order.Price - takeProfitLevel,
						StopLimitPrice = (direction == Sides.Sell) ? t.Order.Price - stopLossLevel : t.Order.Price + stopLossLevel,
						Offset = takeProfitOffset,
						IsMarketStopLimit = true,
						IsMarketTakeProfit = true
					}
				};

				RegisterOrder(order);
			}
		}
	}
}