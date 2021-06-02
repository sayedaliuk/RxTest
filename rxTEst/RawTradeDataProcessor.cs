using System;
using System.Collections.Concurrent;

namespace rxTEst
{
    public class RawTradeDataProcessor
    {
        private readonly DataBlotter _sink;
        private readonly ConcurrentDictionary<string, object> _inProcTrades = new ConcurrentDictionary<string, object>();
        //private readonly Random _seq = new Random();

        public RawTradeDataProcessor(DataBlotter sink)
        {
            _sink = sink;
        }

        public void Process(StockExchangeDataDto dto)
        {
            try
            {
                if (_inProcTrades.ContainsKey(dto.ExternalTradeId))
                    throw new DataMisalignedException($"Source trade already in processing. Duplicate request");

                if (!_inProcTrades.TryAdd(dto.ExternalTradeId, dto))
                    throw new OperationCanceledException($"Failed to process source trade ref : {dto.ExternalTradeId}");

                var trade = new TradeDto
                {
                    ExecutionTimestamp = DateTime.UtcNow,
                    TradeId = Guid.NewGuid().ToString(),
                    TradeSourceRef = dto.ExternalTradeId
                };



                _sink.Publish(trade);
            }
            catch(Exception ex)
            {
                var error = new ErrorResult
                {
                    ErrorCode = 101,
                    Message = string.Join(". ", "Error : Stock Exchange raw trade data procesing", ex.Message, ex.InnerException?.Message, ex.StackTrace)
                };

                _sink.Publish(error);
            }
        }
        public void Process(InternalTradeDataDto dto)
        {
            try
            {
                if (_inProcTrades.ContainsKey(dto.InternalTradeId))
                    throw new OperationCanceledException($"Source trade already in processing. Duplicate request");

                if (!_inProcTrades.TryAdd(dto.InternalTradeId, dto))
                    throw new OperationCanceledException($"Failed to process source trade ref : {dto.InternalTradeId}");

                var trade = new TradeDto
                {
                    ExecutionTimestamp = DateTime.UtcNow,
                    TradeId = Guid.NewGuid().ToString(),
                    TradeSourceRef = dto.InternalTradeId
                };

                _sink.Publish(trade);
            }
            catch (Exception ex)
            {
                var error = new ErrorResult
                {
                    ErrorCode = 201,
                    Message = string.Join(". ", "Error : Internal raw trade data procesing", ex.Message, ex.InnerException?.Message, ex.StackTrace)
                };

                _sink.Publish(error);
            }
        }

    }
}
