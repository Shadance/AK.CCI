using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using Ecng.Common;
using log4net;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Testing;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public class EmulationConnectionManager : IConnectionManager
	{
		private static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		private readonly IConfiguration _configuration;

		private HistoryEmulationConnector _trader;
		private CandleManager _candleManager;

		public ManualResetEvent TraderConnectedEvent { get; } = new ManualResetEvent(false);

		public IConnector Trader
		{
			get
			{
				if (_trader == null)
				{
					// create test security
					var security = new Security
					{
						Id = "SIU5@FORTS",
						Code = "SIU5",
						Name = "RTS-9.09",
						Board = ExchangeBoard.Forts,
					};

					var startTime = new DateTime(2015, 1, 1);
					var stopTime = new DateTime(2015, 12, 31);

					var level1Info = new Level1ChangeMessage
					{
						SecurityId = security.ToSecurityId(),
						ServerTime = startTime
					};
					//.TryAdd(Level1Fields.PriceStep, 10)
					//.TryAdd(Level1Fields.StepPrice, 10)
					//.TryAdd(Level1Fields.MinPrice, 100)
					//.TryAdd(Level1Fields.MaxPrice, 150)
					//.TryAdd(Level1Fields.MarginBuy, 100)
					//.TryAdd(Level1Fields.MarginSell, 100);
					

					var portfolio = new Portfolio
					{
						Name = "SOMETHING",
						BeginValue = 1000000,
					};

					_trader = new HistoryEmulationConnector(
						new[] { security },
						new[] { portfolio })
					{
						// set history range
						StartDate = startTime,
						StopDate = stopTime,

						// set market time freq as time frame
						MarketTimeChangedInterval = TimeSpan.FromMinutes(1)
					};

					_trader.Connected += () => TraderConnectedEvent.Set();
					_trader.Disconnected += () => TraderConnectedEvent.Reset();

					_trader.Restored += () => Log.Info("Trader.Restored");
					_trader.NewSecurities += securities => Log.Info("Trader.NewSecurities");

					_trader.ConnectionError += error => Log.Error("Trader.ConnectionError", error);
					_trader.Error += error =>
					{
						Log.Error("Trader.Error", error);
					};
					_trader.MarketDataSubscriptionFailed += (sec, type, error) => Log.Error("Trader.MarketDataSubscriptionFailed", error);
					_trader.StateChanged += () =>
					{
						Log.InfoFormat("Trader.StateChanged {0}", _trader.State);
					};

					_trader.NewSecurities += securities =>
					{
						if (securities.All(s => s != security))
							return;

						// fill level1 values
						_trader.SendInMessage(level1Info);

						_trader.RegisterTrades(new RandomWalkTradeGenerator(_trader.GetSecurityId(security)));
						//_trader.RegisterMarketDepth(new TrendMarketDepthGenerator(_trader.GetSecurityId(security)) { GenerateDepthOnEachTrade = false });

						_trader.Start();
					};

					_trader.Connect();
				}

				return _trader;
			}
		}

		public CandleManager CandleManager
		{
			get
			{
				if (_candleManager == null)
				{
					_candleManager = new CandleManager(_trader);
					_candleManager.Error += exception => Log.Error("CandleManager.Error", exception);
				}

				return _candleManager;
			}
		}

		public EmulationConnectionManager(IConfiguration configuration)
		{
			_configuration = configuration;
		}
	}
}
