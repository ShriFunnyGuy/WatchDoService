

namespace WatchDogService.Entities
{
    public class ApiResponseModel<T>
    {
        public string Message { get; set; }
        public bool Result { get; set; }
        public T Data { get; set; }
    }
}
