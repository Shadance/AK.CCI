using System;
using System.Configuration;

namespace AK.CCI.Service.Settings
{
	public class StrategyConfiguration : IStrategyConfiguration
	{
		public readonly StrategyConfigurationSection Section;

		public StrategyConfiguration()
		{
			Section = (StrategyConfigurationSection)ConfigurationManager.GetSection("strategyConfiguration") ?? 
				new StrategyConfigurationSection();
		}

		public string SecurityCode => Section.SecurityCode;
		public TimeSpan CandleTimeFrame => Section.CandleTimeFrame;
		public string PortfolioName => Section.PortfolioName;
		public decimal Volume => Section.Volume;
		public int IndicatorLength => Section.IndicatorLength;
		public TimeSpan OrdersCheckInterval => Section.OrdersCheckInterval;
		public TimeSpan OrderExpirationTimeSpan => Section.OrderExpirationTimeSpan;
		public int StopLossLevel => Section.StopLossLevel;
		public int TakeProfitLevel => Section.TakeProfitLevel;
		public int TakeProfitOffset => Section.TakeProfitOffset;
	}
}