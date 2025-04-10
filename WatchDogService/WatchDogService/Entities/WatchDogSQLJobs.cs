namespace WatchDogService.Entities
{
    public class WatchDogSQLJobs
    {
        public int WatchDogSqlJobID { get; set; }
        public string SqlServerName { get; set; }
        public string JobName { get; set; }
        public string? JobStatus { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifyById { get; set; }
        public DateTime? ModifyDate { get; set; }

    }
}
