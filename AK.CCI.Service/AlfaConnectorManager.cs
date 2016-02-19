using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using AK.CCI.Service.Settings;
using Ecng.Common;
using log4net;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.AlfaDirect;
using StockSharp.Algo;
using StockSharp.Algo.Testing;
using StockSharp.Localization;
using StockSharp.Logging;
using StockSharp.Messages;
using LogManager = log4net.LogManager;

namespace AK.CCI.Service
{
	public class AlfaConnectorManager : IConnectorManager
	{
		protected static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		protected IConfiguration _configuration;

		protected AlfaTrader _trader;
		protected CandleManager _candleManager;
	    protected Security _security;
        
		public ManualResetEvent TraderConnectedEvent { get; } = new ManualResetEvent(false);

        public AlfaConnectorManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual IConnector Trader
		{
			get
			{
				if (_trader == null)
				{
                    _trader = new AlfaTrader { LogLevel = LogLevels.Debug };

                    _trader.Restored += () => Log.Info("Connection to AlfaDirect restored.");
                    _trader.NewSecurities += securities => Log.Info("Trader.NewSecurities");
                    _trader.NewSecurities += securities =>
                    {
                        if (securities.All(s => s != _security))
                            return;

                        var level1Info = new Level1ChangeMessage
                        {
                            SecurityId = _security.ToSecurityId(),
                        };
                        // fill level1 values
                        _trader.SendInMessage(level1Info);
                        _trader.RegisterTrades(_security);
                        _trader.RegisterMarketDepth(_security);
                    };

                    // подписываемся на событие успешного соединения
                    _trader.Connected += () => TraderConnectedEvent.Set();

                    // подписываемся на событие успешного отключения
                    _trader.Disconnected += () => TraderConnectedEvent.Reset();

                    // подписываемся на событие разрыва соединения
                    _trader.ConnectionError += error => Log.Info("Connection to AlfaDirect lost!");

                    // подписываемся на ошибку обработки данных (транзакций и маркет)
                    _trader.Error += error => Log.Error(error.ToString());

                    // подписываемся на ошибку подписки маркет-данных
                    _trader.MarketDataSubscriptionFailed += (security, type, error) =>
                        Log.Error(error.ToString() + LocalizedStrings.Str2956Params.Put(type, security));

//                    _trader.StateChanged += () => Log.InfoFormat("Trader.StateChanged {0}", _trader.State);

                    // подписываемся на событие о неудачной регистрации заявок
                    _trader.OrdersRegisterFailed += fails =>
				    {
				        foreach (var fail in fails)
				            Log.Error(fail.Error);
				    };
                    // подписываемся на событие о неудачном снятии заявок
                    _trader.OrdersCancelFailed += fails =>
                    {
                        foreach (var fail in fails)
                            Log.Error(fail.Error);
                    };
                    // подписываемся на событие о неудачной регистрации стоп-заявок
                    _trader.StopOrdersRegisterFailed += fails =>
                    {
                        foreach (var fail in fails)
                            Log.Error(fail.Error);
                    };
                    // подписываемся на событие о неудачном снятии стоп-заявок
                    _trader.StopOrdersCancelFailed += fails =>
                    {
                        foreach (var fail in fails)
                            Log.Error(fail.Error);
                    };

				    _trader.Login = _configuration.Login;
                    _trader.Password = _configuration.Password;
                }

                if ((_trader.ConnectionState == ConnectionStates.Disconnected)|| (_trader.ConnectionState == ConnectionStates.Failed))
                    _trader.Connect();

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

    }
}
