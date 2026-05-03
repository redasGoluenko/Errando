namespace Errando.DTOs
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int ClientCount { get; set; }
        public int RunnerCount { get; set; }
        
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public decimal TaskCompletionRate { get; set; }
        
        public int TotalComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int UnresolvedComplaints { get; set; }
        public decimal ComplaintResolutionRate { get; set; }
        
        public decimal TotalRevenue { get; set; }
        public int SuccessfulPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        
        public decimal AverageSystemRating { get; set; }
        public int UsersCreatedThisMonth { get; set; }
        public int TasksCreatedThisMonth { get; set; }
    }
}
