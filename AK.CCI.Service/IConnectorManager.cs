using System.Collections.Generic;
using System.Threading;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public interface IConnectorManager
	{
		ManualResetEvent TraderConnectedEvent { get; }
		CandleManager CandleManager { get; }
		IConnector Trader { get; }
	}
}