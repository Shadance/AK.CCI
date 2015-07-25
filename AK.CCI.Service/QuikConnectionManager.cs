using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Threading;
using Ecng.Common;
using log4net;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public class QuikConnectionManager : IConnectionManager
	{
		private static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		private readonly IConfiguration _configuration;

		private QuikTrader _trader;
		private CandleManager _candleManager;

		public ManualResetEvent TraderConnectedEvent { get; } = new ManualResetEvent(false);

		public QuikTrader Trader
		{
			get
			{
				if (_trader == null)
				{
					_trader = new QuikTrader
					{
						LuaFixServerAddress = "127.0.0.1:5001".To<EndPoint>(),
						LuaLogin = "quik",
						LuaPassword = "quik".To<SecureString>()
					};

					_trader.Connected += () => TraderConnectedEvent.Set();
					_trader.Restored += () => Log.Info("Trader.Restored");
					_trader.ConnectionError += error => Log.Info("Trader.ConnectionError", error);
					_trader.Error += error => Log.Info("Trader.Error", error);
					_trader.MarketDataSubscriptionFailed += (security, type, error) => Log.Info("Trader.MarketDataSubscriptionFailed", error);
					_trader.NewSecurities += securities => Log.Info("Trader.NewSecurities");

					_trader.Connect();
				}

				return _trader;
			}
		}

		public CandleManager CandleManager
		{
			get { return _candleManager ?? (_candleManager = new CandleManager(_trader)); }
		}

		public QuikConnectionManager(IConfiguration configuration)
		{
			_configuration = configuration;
		}
	}
}
