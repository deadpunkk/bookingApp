namespace BookingApi.Domain;

public class Booking
{
    public Booking(string title, DateTime startDate, DateTime endDate)
    {
        Title = title;
        StartDate = startDate;
        EndDate = endDate;
    }
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}