using System;

namespace rxTEst
{
    public class TradeDto
    {
        public string TradeId { get; set; }
        public string TradeSourceRef { get; set; }
        public DateTime? ExecutionTimestamp { get; set; }
    }
}
