namespace AngularAuthAPI.Classes
{
    public class Response<T>
    {
        
        public Response(T data) 
        {
            Success = true;
            Message = string.Empty;
            Errors = null;
            Data = data;
        }

        public static Response<T> GetError<T>(T data, string message, string?[] error=null )
        {
            return new Response<T>(data)
            {
                Success = false,
                Message = message,
                Errors = error
            };
        }

        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[]? Errors { get; set; }

    }
}
