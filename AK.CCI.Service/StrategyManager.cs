using AK.CCI.Service.Settings;
using AK.CCI.Service.Strategies;
using log4net;
using StockSharp.BusinessEntities;

namespace AK.CCI.Service
{
	public class StrategyManager : IStrategyManager
	{
		private readonly IConfiguration _configuration;
		private readonly AKStrategy[] _strategies;
	    private Portfolio   _portfolio;
	    private Security _security;
        
		public StrategyManager(IConfiguration configuration,
            AKStrategy[] strategies)
		{
			_configuration = configuration;
			_strategies = strategies;
            _portfolio  = new Portfolio();
		    _security = new Security();

		}

		public void Start()
		{            
            foreach (AKStrategy s in _strategies)
			{
                s.Portfolio = _portfolio;
			    s.Security = _security;
                s.Start();
			}
		}

		public void Stop()
		{
			foreach (AKStrategy s in _strategies)
			{
				s.Stop();
			}
		}
	}
}
