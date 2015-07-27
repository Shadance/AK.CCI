using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security;
using AK.CCI.Service.Settings;
using Ecng.Common;
using log4net;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

namespace AK.CCI.Service
{
	public class StrategyManager : IStrategyManager
	{
		private static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		private readonly IConfiguration _configuration;

		private readonly IStrategy[] _strategies;

		public StrategyManager(IConfiguration configuration,
			IStrategy[] strategies)
		{
			_configuration = configuration;
			_strategies = strategies;
		}

		public void Start()
		{
			foreach (IStrategy s in _strategies)
			{
				s.Start();
			}
		}

		public void Stop()
		{
			foreach (IStrategy s in _strategies)
			{
				s.Stop();
			}
		}
	}
}
