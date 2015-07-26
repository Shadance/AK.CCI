using System;
using StockSharp.Algo.Indicators;
using StockSharp.Messages;

namespace AK.CCI.Service
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

		protected override IIndicatorValue OnProcess(IIndicatorValue input)
		{
			var result = base.OnProcess(input);

			if (Container.Count > Length + 2)
			{
				var lastValue = this.GetValue(0);
				var prevValue = this.GetValue(1);

				if (prevValue < LBar && lastValue > LBar)
				{
					OnBarCrossed(new BarCrossedEventArgs {Side = Sides.Buy, PrevIndicatorValue = prevValue, LastIndicatorValue = lastValue });
				}
				else
				{
					if (prevValue > HBar && lastValue < HBar)
					{
						OnBarCrossed(new BarCrossedEventArgs { Side = Sides.Sell, PrevIndicatorValue = prevValue, LastIndicatorValue = lastValue });
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