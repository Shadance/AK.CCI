using System;
using System.Security.Cryptography.X509Certificates;

namespace AK.CCI.Service
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