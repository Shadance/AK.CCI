using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK.CCI.Service.Settings
{
	public class StrategyConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("portfolioName", DefaultValue = "SPBFUT00dao")]
		public string PortfolioName
		{
			get { return (string)this["portfolioName"]; }
			set { this["portfolioName"] = value; }
		}

		[ConfigurationProperty("securityCode", DefaultValue = "SiU5")]
		public string SecurityCode
		{
			get { return (string)this["securityCode"]; }
			set { this["securityCode"] = value; }
		}

		[ConfigurationProperty("candleTimeFrame", DefaultValue = "00:01:00")]
		public TimeSpan CandleTimeFrame
		{
			get { return (TimeSpan)this["candleTimeFrame"]; }
			set { this["candleTimeFrame"] = value; }
		}

		[ConfigurationProperty("orderExpirationTimeSpan", DefaultValue = "00:02:00")]
		public TimeSpan OrderExpirationTimeSpan
		{
			get { return (TimeSpan)this["orderExpirationTimeSpan"]; }
			set { this["orderExpirationTimeSpan"] = value; }
		}

		[ConfigurationProperty("volume", DefaultValue = "1")]
		public decimal Volume
		{
			get { return (decimal)this["volume"]; }
			set { this["volume"] = value; }
		}

		[ConfigurationProperty("indicatorLength", DefaultValue = "5")]
		public int IndicatorLength
		{
			get { return (int)this["indicatorLength"]; }
			set { this["indicatorLength"] = value; }
		}

		[ConfigurationProperty("ordersCheckInterval", DefaultValue = "00:00:10")]
		public TimeSpan OrdersCheckInterval
		{
			get { return (TimeSpan)this["ordersCheckInterval"]; }
			set { this["ordersCheckInterval"] = value; }
		}

		[ConfigurationProperty("stopLossLevel", DefaultValue = "20")]
		public int StopLossLevel
		{
			get { return (int)this["stopLossLevel"]; }
			set { this["stopLossLevel"] = value; }
		}

		[ConfigurationProperty("takeProfitLevel", DefaultValue = "40")]
		public int TakeProfitLevel
		{
			get { return (int)this["takeProfitLevel"]; }
			set { this["takeProfitLevel"] = value; }
		}

		[ConfigurationProperty("takeProfitOffset", DefaultValue = "40")]
		public int TakeProfitOffset
		{
			get { return (int)this["takeProfitOffset"]; }
			set { this["takeProfitOffset"] = value; }
		}
	}
}
