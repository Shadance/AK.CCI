using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Ninject;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;

namespace AK.CCI.Service
{
	public class CCIStrategy : Strategy, IStrategy
	{
		private static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		private readonly IConnectionManager _connectionManager;
		private readonly IStrategyConfiguration _strategyConfiguration;

		private Security _security;
		private Portfolio _portfolio;

		protected ManualResetEvent PortfolioFoundEvent = new ManualResetEvent(false);
		protected ManualResetEvent SecurityFoundEvent = new ManualResetEvent(false);

		[Inject]
		public CommodityChannelIndex Indicator { get; private set; }

		public CCIStrategy(IConnectionManager connectionManager, IStrategyConfiguration strategyConfiguration)
		{
			_connectionManager = connectionManager;
			_strategyConfiguration = strategyConfiguration;

			var trader = _connectionManager.Trader;

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
			base.OnStarted();

			Log.Info("Waiting for TraderConnected event.");
			_connectionManager.TraderConnectedEvent.WaitOne();

			Connector = _connectionManager.Trader;

			Log.Info("Waiting for PortfolioFoundEvent event.");
			PortfolioFoundEvent.WaitOne();

			Portfolio = _portfolio;

			Log.Info("Waiting for SecurityFound event.");
			SecurityFoundEvent.WaitOne();

			Security = _security;
			Volume = _strategyConfiguration.Volume;

			Indicator.Length = _strategyConfiguration.IndicatorLength;

			var series = new CandleSeries(typeof (TimeFrameCandle), _security, _strategyConfiguration.CandleTimeFrame);

			series
				.WhenCandlesFinished()
				.Do(ProcessCandle)
				.Apply(this);

			_connectionManager.CandleManager.Start(series);
		}

		private void ProcessCandle(Candle candle)
		{
			Log.Info("New candle registred.");
			if (ProcessState == ProcessStates.Stopping)
			{
				CancelActiveOrders();
				return;
			}

			Indicator.Process(candle);
		}
	}
}
