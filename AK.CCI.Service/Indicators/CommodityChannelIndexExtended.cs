using System;
using System.Threading;
using StockSharp.Algo.Indicators;
using StockSharp.Messages;

namespace AK.CCI.Service.Indicators
{
	public class BarCrossedEventArgs : EventArgs
	{
		public Sides Side { get; set; }
		public decimal PrevIndicatorValue { get; set; }
		public decimal LastIndicatorValue { get; set; }
	}

	public class CommodityChannelIndexExtended : CommodityChannelIndex
	{
		private const decimal HBar = 100;
		private const decimal LBar = -100;

		public event EventHandler<BarCrossedEventArgs> BarCrossed;
		public BarCrossedEventArgs IsBarCrossedOnLastValue;

		protected override IIndicatorValue OnProcess(IIndicatorValue input)
		{
			var result = base.OnProcess(input);

			IsBarCrossedOnLastValue = null;
			if (Container.Count > Length)
			{
				var lastValue = result.GetValue<decimal>();
				var prevValue = this.GetCurrentValue();

				if (prevValue < LBar && lastValue > LBar)
				{
					IsBarCrossedOnLastValue = new BarCrossedEventArgs { Side = Sides.Buy, PrevIndicatorValue = prevValue, LastIndicatorValue = lastValue };
					OnBarCrossed(IsBarCrossedOnLastValue);
				}
				else
				{
					if (prevValue > HBar && lastValue < HBar)
					{
						IsBarCrossedOnLastValue = new BarCrossedEventArgs { Side = Sides.Sell, PrevIndicatorValue = prevValue, LastIndicatorValue = lastValue };
						OnBarCrossed(IsBarCrossedOnLastValue);
					}
				}
			}

			return result;
		}

		protected virtual void OnBarCrossed(BarCrossedEventArgs e)
		{
			BarCrossed?.Invoke(this, e);
		}
	}
}