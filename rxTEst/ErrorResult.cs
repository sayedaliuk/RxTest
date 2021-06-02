namespace rxTEst
{
    public class ErrorResult
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }

        public override string ToString()
        {
            return $"Error Code = {ErrorCode}. Error Msg = {Message}";
        }
    }
}
