namespace BookingApi.Domain;

public class Booking
{
    public Booking(string title, DateTime startDate, DateTime endDate, int createdByUserId)
    {
        Title = title;
        StartDate = startDate;
        EndDate = endDate;
        CreatedByUserId = createdByUserId;
    }
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}