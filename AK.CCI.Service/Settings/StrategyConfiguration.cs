using System;

namespace AK.CCI.Service.Settings
{
	class StrategyConfiguration : IStrategyConfiguration
	{
		public string SecurityCode => "SIU5";

		public TimeSpan CandleTimeFrame => TimeSpan.FromMinutes(1);
		public string PortfolioName => "SOMETHING";
		public decimal Volume => 1;
		public int IndicatorLength => 5;
	}
}