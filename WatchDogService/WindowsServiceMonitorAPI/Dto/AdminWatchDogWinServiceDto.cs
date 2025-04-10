namespace WatchDogServiceApi.Dto
{
    public class AdminWatchDogWinServicesDto
    {
        public int Pid { get; set; }
        public string ServiceName { get; set; }
        public string ServerName { get; set; }
        public string ServiceStatus { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifyById { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
