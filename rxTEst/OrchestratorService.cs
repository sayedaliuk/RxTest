using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace rxTEst
{
    /// <summary>
    // Download data from stock exchange web api (upstream)
    // Monitor a folder for new trade files (upstream)
    // process all raw trade data (raw data processor)
    // generate trade dto  (trade generator)
    // send trades to Trade capture API (trade capture API proxy)
    // create trade file of generated trade (file handler)
    //  -- move the file to processed folder if trade successfully sent to trade capture
    //  -- if trade generation failed or if trade capture returned error then move the generated trade to error folder
    /// </summary>
    public class OrchestratorService
    {
        private readonly DataBlotter _sink;
        private readonly RawTradeDataProcessor _processor;
        private ConcurrentDictionary<string, object> _inProcRawTrades = new ConcurrentDictionary<string, object>();
        Random _tradeIdSeq = new Random(1);

        public OrchestratorService()
        {
            _sink = new DataBlotter();
            _processor = new RawTradeDataProcessor(_sink);
        }

        public void Start()
        {
            Observable.Interval(TimeSpan.FromMilliseconds(10))
                      .Subscribe(x => DownloadStockExchangeData());

            Observable.Interval(TimeSpan.FromMilliseconds(15))
                        .Subscribe(x => ReadInternalTradeFiles());


            _sink.Observe()
                 .OfType<StockExchangeDataDto>()
                 .Subscribe(x =>
                 {
                     if (!_inProcRawTrades.TryAdd(x.ExternalTradeId, x))
                         return;

                     Console.WriteLine($"Stock trade : {x.ExternalTradeId}");
                     _processor.Process(x);
                 });

            _sink.Observe()
                 .OfType<InternalTradeDataDto>()
                 .Where(x => !_inProcRawTrades.ContainsKey(x.InternalTradeId))
                 .Subscribe(x =>
                 {
                     if (!_inProcRawTrades.TryAdd(x.InternalTradeId, x))
                         return;

                     Console.WriteLine($"Internal trade : {x.InternalTradeId}");
                     _processor.Process(x);
                 });

            _sink.Observe()
                .OfType<TradeDto>()
                .Subscribe(x =>
                {
                    Console.WriteLine($"1 new trade generated. Trade Id : {x.TradeId}. Trade Source Ref : {x.TradeSourceRef}");
                    Task.Factory.StartNew(() => SendTradeToRepository(x));
                });

            _sink.Observe()
                .OfType<ErrorResult>()
                .Subscribe(x =>
                {
                    Console.WriteLine($"ERROR : {x.ToString()}");
                });
        }

        public void DownloadStockExchangeData()
        {
            var nextVal = _tradeIdSeq.Next(1, 100);

            // downloads data 
            var d = new StockExchangeDataDto
            {
                ExternalTradeId = nextVal.ToString()  //Guid.NewGuid().ToString()
            };

            _sink.Publish(d);

        }
        public void ReadInternalTradeFiles()
        {
            var d = new InternalTradeDataDto
            {
                InternalTradeId = DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")
            };

            _sink.Publish(d);
        }
        public void SendTradeToRepository(TradeDto dto)
        {
            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(x =>
                {
                    Console.WriteLine($"1 trade saved. Trade Id : {dto.TradeId}");
                })
                .GetAwaiter()
                .GetResult();
        }
    }
}
