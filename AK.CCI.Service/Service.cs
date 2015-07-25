using log4net;

namespace AK.CCI.Service
{
	public class Service
	{
		private static readonly ILog Log = LogManager.GetLogger("AK.CCI.Service");
		private IConfiguration _configuration;
		private readonly IStrategyManager _strategyManager;

		public Service(IConfiguration configuration, IStrategyManager strategyManager)
		{
			_configuration = configuration;
			_strategyManager = strategyManager;
		}

		public void Start()
		{
			Log.Info("AK.CCI.Service has been started.");

			_strategyManager.Start();
		}

		public void Stop()
		{
			_strategyManager.Stop();

			Log.Info("AK.CCI.Service has been stopped.");
		}
	}
}