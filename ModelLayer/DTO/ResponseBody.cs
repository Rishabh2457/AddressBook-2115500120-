using System;
namespace ModelLayer.DTO
{
    //standard response body 
    public class ResponseBody<T>
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "";
        public T Data { get; set; } = default(T);

    }
}
