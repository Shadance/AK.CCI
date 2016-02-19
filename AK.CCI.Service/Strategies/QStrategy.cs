using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AK.CCI.Service.Indicators;
using AK.CCI.Service.Settings;
using AK.CCI.Service.Strategies;
using Ecng.Collections;
using log4net;
using Ninject;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Strategies.Protective;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Quik;
using LogManager = log4net.LogManager;

namespace AK.CCI.Service
{
    public class QStrategy : AKStrategy
    {
        public QStrategy()
        {
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            Logger.Info("Configuring QStrategy.");
        }

        protected override void ProcessCandle(Candle candle)
        {
            if (Orders.Any(o => o.State == OrderStates.Active || o.State == OrderStates.Pending || o.State == OrderStates.None))
            {
                //Logger.DebugFormat("Blocks Orders because: State");
                return;
            }

            var timeout = TimeSpan.FromMilliseconds(CandleTimeFrame.TotalMilliseconds * 6);
            if (Orders.Any(o => o.Time.Add(timeout) > CurrentTime))
            {
                //Logger.DebugFormat("Blocks Orders because: Last Order Time");
                return;
            }

            TimeFrameCandle lastCandle = this.GetCandleManager().GetCandle<TimeFrameCandle>(Series,1);
            if (lastCandle == null)
            {
                //Logger.DebugFormat("Can't get last candle yet.");
                return;
            }

            // hack to avoid analyzing obsolete candles
            if (candle.ClosePrice == lastCandle.ClosePrice
                || candle.CloseTime.Add(CandleTimeFrame) < CurrentTime)
            {
                return;
            }

            Sides direction;
            decimal targetPrice;
            bool isOrderAllowed = false;

            if (candle.ClosePrice > lastCandle.ClosePrice)
            {
                direction = Sides.Buy;
                targetPrice = candle.ClosePrice + TakeProfitLevel;

                if (lastCandle.HighPrice > targetPrice)
                {
                    isOrderAllowed = true;
                }
            }
            else
            {
                direction = Sides.Sell;
                targetPrice = candle.ClosePrice - TakeProfitLevel;

                if (lastCandle.LowPrice < targetPrice)
                {
                    isOrderAllowed = true;
                }
            }

            if (!isOrderAllowed)
            {
                return;
            }

            var newOrder = direction == Sides.Buy
                ? this.BuyAtLimit(candle.ClosePrice)
                : this.SellAtLimit(candle.ClosePrice);

            Logger.WarnFormat("New Order: {0}", newOrder);
            /*
                        newOrder
                            .WhenNewTrades(SafeGetConnector())
                            .Do(OnNewOrderTrades)
                            .Apply(this);
            */
            RegisterOrder(newOrder);
        }

    }
}