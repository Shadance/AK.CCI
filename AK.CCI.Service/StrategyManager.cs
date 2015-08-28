using AK.CCI.Service.Settings;
using log4net;

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
