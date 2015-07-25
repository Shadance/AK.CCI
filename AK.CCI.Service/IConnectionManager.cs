using System.Collections.Generic;
using System.Threading;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public interface IConnectionManager
	{
		ManualResetEvent TraderConnectedEvent { get; }
		CandleManager CandleManager { get; }
		QuikTrader Trader { get; }
	}
}