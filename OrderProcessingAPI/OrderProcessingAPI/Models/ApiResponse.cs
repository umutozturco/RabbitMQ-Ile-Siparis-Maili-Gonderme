namespace OrderProcessingAPI.Models
{
    public class ApiResponse<T>
    {
        public Status Status { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorCode { get; set; }
        public T Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(Status status, string resultMessage, T data = default(T))
        {
            Status = status;
            ResultMessage = resultMessage;
            Data = data;
        }

        public ApiResponse(Status status, string resultMessage, string errorCode, T data = default(T))
        {
            Status = status;
            ResultMessage = resultMessage;
            ErrorCode = errorCode;
            Data = data;
        }
    }

    public enum Status
	{
		Success,
		Failed
	}
}
