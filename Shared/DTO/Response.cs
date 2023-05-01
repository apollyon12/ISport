namespace iSportsRecruiting.Shared.DTO
{
    public class Request<T>
    {
        public T Payload { get; set; }

        public Request() { }
        public Request(T payload)
        {
            Payload = payload;
        }
    }

    public class Response
    {
        public ResponseStatus Status { get; set; }

        private string _message;
        public string Message
        {
            get
            {
                if (_message is null)
                    _message = Status.GetMessage();

                return _message;
            }
            set => _message = value;
        }
    }
    public class Response<T> : Response
    {
        public T Payload { get; set; }
        public int? Total { get; set; }

        public Response() { }
        public Response(T payload)
        {
            Payload = payload;
        }
    }

    public enum ResponseStatus
    {
        Ok,
        InternalError,
        IncorrectEmailOrPassword,
        Error,
        AccountDisabled,
    }

    public static class ResponseStatusExtensions
    {
        public static string GetMessage(this ResponseStatus status) => status switch
        {
            ResponseStatus.IncorrectEmailOrPassword => "Incorrect Email or Password, please verify and try again",
            _ => string.Empty,
        };
    }

}
