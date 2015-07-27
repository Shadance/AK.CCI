using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Threading;
using AK.CCI.Service.Settings;
using Ecng.Common;
using log4net;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Candles.Compression;
using StockSharp.Algo.Testing;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public class QuikConnectorManager : IConnectorManager
	{
		protected static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		protected IConfiguration _configuration;

		protected IConnector _trader;
		protected CandleManager _candleManager;

		public ManualResetEvent TraderConnectedEvent { get; } = new ManualResetEvent(false);

		public virtual IConnector Trader
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

					ConfigureConnector();

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

		public QuikConnectorManager(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected void ConfigureConnector()
		{
			_trader.Connected += () => TraderConnectedEvent.Set();

			_trader.ConnectionError += error => Log.Error("Trader.ConnectionError", error);
			_trader.Error += error => Log.Error("Trader.Error", error);
			_trader.MarketDataSubscriptionFailed +=
				(security, type, error) => Log.Error("Trader.MarketDataSubscriptionFailed", error);
		}
	}

	public class QuikEmulationConnectorManager : QuikConnectorManager
	{
		public QuikEmulationConnectorManager(IConfiguration configuration) : base(configuration)
		{
		}

		public override IConnector Trader
		{
			get
			{
				if (_trader == null)
				{
					_trader = new RealTimeEmulationTrader<QuikTrader>(new QuikTrader()
					{
						LuaFixServerAddress = "127.0.0.1:5001".To<EndPoint>(),
						LuaLogin = "quik",
						LuaPassword = "quik".To<SecureString>()
					});

					ConfigureConnector();

					_trader.Connect();
				}

				return _trader;
			}
		}
	}
}
