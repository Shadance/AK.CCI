using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AK.CCI.Service.Settings;
using Ecng.Common;
using log4net;
using log4net.Appender;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Risk;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Quik;
using StockSharp.Xaml;
using LogManager = log4net.LogManager;

namespace AK.CCI.Service.Strategies
{
    public class AKStrategy : Strategy, IStrategyConfiguration
    {
        protected static readonly ILog Logger = LogManager.GetLogger("AK.CCI.Service");
      
        //        private ChartArea _area;
        //        protected Portfolio _portfolio;
        protected CandleSeries Series;

        protected ManualResetEvent PortfolioFoundEvent  = new ManualResetEvent(false);
        protected ManualResetEvent SecurityFoundEvent   = new ManualResetEvent(false);

        protected Timer OrdersCheckTimer;

        protected readonly StrategyParam<TimeSpan> _CandleTimeFrame;
        [DisplayName("CandleTimeFrame")]
        public TimeSpan CandleTimeFrame
        {
            get { return _CandleTimeFrame.Value; }
            set { _CandleTimeFrame.Value = value; }
        }

        protected readonly StrategyParam<string> _PortfolioName;
        [DisplayName("PortfolioName")]
        public string PortfolioName
        {
            get { return _PortfolioName.Value; }
            set { _PortfolioName.Value = value; }
        }

        protected readonly StrategyParam<int> _IndicatorLength;
        [DisplayName("Indicator Length")]
        public int IndicatorLength
        {
            get { return _IndicatorLength.Value; }
            set { _IndicatorLength.Value = value; }
        }

        protected readonly StrategyParam<string> _SecurityCode;
        [DisplayName("Security Code")]
        public string SecurityCode
        {
            get { return _SecurityCode.Value; }
            set { _SecurityCode.Value = value; }
        }

        protected readonly StrategyParam<TimeSpan> _OrdersCheckInterval;
        [DisplayName("OrdersCheckInterval")]
        public TimeSpan OrdersCheckInterval
        {
            get { return _OrdersCheckInterval.Value; }
            set { _OrdersCheckInterval.Value = value; }
        }

        protected readonly StrategyParam<TimeSpan> _OrderExpirationTimeSpan;
        [DisplayName("OrderExpirationTimeSpan")]
        public TimeSpan OrderExpirationTimeSpan
        {
            get { return _OrderExpirationTimeSpan.Value; }
            set { _OrderExpirationTimeSpan.Value = value; }
        }

        protected readonly StrategyParam<int> _StopLossLevel;
        [DisplayName("StopLossLevel")]
        public int StopLossLevel
        {
            get { return _StopLossLevel.Value; }
            set { _StopLossLevel.Value = value; }
        }

        protected readonly StrategyParam<int> _TakeProfitLevel;
        [DisplayName("TakeProfitLevel")]
        public int TakeProfitLevel
        {
            get { return _TakeProfitLevel.Value; }
            set { _TakeProfitLevel.Value = value; }
        }

        protected readonly StrategyParam<int> _TakeProfitOffset;
        [DisplayName("TakeProfitOffset")]
        public int TakeProfitOffset
        {
            get { return _TakeProfitOffset.Value; }
            set { _TakeProfitOffset.Value = value; }
        }

        public AKStrategy()
        {
            _CandleTimeFrame = this.Param("CandleTimeFrame", TimeSpan.FromMinutes(5));
            Log += OnLog;
        }

        protected new virtual void OnStarted()
        {
            Logger.Info("Waiting for PortfolioFoundEvent event.");
            PortfolioFoundEvent.WaitOne();

            Logger.Info("Waiting for SecurityFound event.");
            SecurityFoundEvent.WaitOne();

            OrdersCheckTimer = new Timer(CheckOrdersElapsed, null, 0, OrdersCheckInterval.Milliseconds);
            Series = new CandleSeries(typeof(TimeFrameCandle), Security, CandleTimeFrame)
            {
                From = CurrentTime - TimeSpan.FromTicks(CandleTimeFrame.Ticks * IndicatorLength)
            };

            this.WhenNewMyTrades()
                .Do(OnNewOrderTrades)
                .Apply(this);

            this.GetCandleManager()
                .WhenCandlesFinished(Series)
                .Do(ProcessFinishedCandle)
                .Apply(this);

            this.GetCandleManager()
                .WhenCandlesChanged(Series)
                .Do(ProcessCandle)
                .Apply(this);

            this.GetCandleManager().Start(Series);

            Security
                .WhenChanged(SafeGetConnector())
                .Do(ProcessSecurityChange)
                .Apply(this);
            
            //            SafeGetConnector().RegisterMarketDepth(Security);

            base.OnStarted();
        }
        protected new virtual void OnStopped()
        {
            //            SafeGetConnector().UnRegisterMarketDepth(Security);
            if (this.GetCandleManager() != null)
                this.GetCandleManager().Stop(Series);

            base.OnStopped();
        }
        protected new virtual void OnReseted()
        {
/*            if (_area != null)
                new ChartRemoveAreaCommand(_area).Process(this);
            _area = null;
*/
            base.OnReseted();
        }

        protected void OnLog(LogMessage message)
        {
            if (message.Level > LogLevels.Debug)
            {
                Logger.InfoFormat("[{0}] {1}", message.Source, message.Message);
            }
        }
        protected override void OnError(Exception error)
        {
            Logger.Error(error);
            base.OnError(error);
        }
        protected virtual void ProcessSecurityChange()
        {
            if (Security.LastTrade.Time < this.StartedTime)
            {
                if (Security.LastTrade.Time.Minute == 0 && Security.LastTrade.Time.Second == 0)
                {
                    Logger.DebugFormat("Loading Trades ... {0}", Security.LastTrade.Time);
                }
            }
        }

        protected virtual void CheckOrdersElapsed(object stateInfo)
        {
            var now = this.CurrentTime;
            foreach (var order in Orders.Where(o => o.IsMatchedEmpty()))
            {
                if (order.Time.Add(OrderExpirationTimeSpan) < now)
                {
                    CancelOrder(order);
                }
            }
        }

        protected virtual void ProcessCandle(Candle candle)
        {
        }

        protected virtual void ProcessFinishedCandle(Candle candle)
        {
        }

        protected virtual void OnNewOrderTrades(IEnumerable<MyTrade> trades)
        {
            foreach (var t in trades)
            {
                if (t.Order.Type == OrderTypes.Conditional)
                {
                    continue;
                }

                var direction = (t.Order.Direction == Sides.Buy) ? Sides.Sell : Sides.Buy;

                var order = new Order
                {
                    Type = OrderTypes.Conditional,
                    Volume = t.Order.Volume,
                    //Price = stopLossLevel,
                    Direction = direction,
                    Security = t.Order.Security,
                    Condition = new QuikOrderCondition
                    {
                        Type = QuikOrderConditionTypes.TakeProfitStopLimit,
                        StopPrice =
                            (direction == Sides.Sell)
                                ? t.Order.Price + TakeProfitLevel
                                : t.Order.Price - TakeProfitLevel,
                        StopLimitPrice =
                            (direction == Sides.Sell) ? t.Order.Price - StopLossLevel : t.Order.Price + StopLossLevel,
                        Offset = TakeProfitOffset,
                        IsMarketStopLimit = true,
                        IsMarketTakeProfit = true
                    }
                };

                RegisterOrder(order);
            }

/*
            foreach (var t in trades)
            {
                new ChartDrawCommand(t.Trade.Time, new Dictionary<IChartElement, object>
                                    {
                                        { _area.Elements[3], t }
                                    }).Process(this);
            }
*/
        }
    }
}
