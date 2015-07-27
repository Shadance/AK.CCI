using System;

namespace AK.CCI.Service.Settings
{
	public interface IStrategyConfiguration
	{
		string SecurityCode { get; }
		TimeSpan CandleTimeFrame { get; }
		string PortfolioName { get; }
		decimal Volume { get; }
		int IndicatorLength { get; }
	}
}