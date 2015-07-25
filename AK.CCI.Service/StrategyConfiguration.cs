using System;

namespace AK.CCI.Service
{
	class StrategyConfiguration : IStrategyConfiguration
	{
		public string SecurityCode => "siu5";

		public TimeSpan CandleTimeFrame => TimeSpan.FromMinutes(1);
		public string PortfolioName => "SOMETHING";
		public decimal Volume => 1;
		public int IndicatorLength => 14;
	}
}