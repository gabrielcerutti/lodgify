namespace Lodgify.Api.Database.Entities
{
    public class ShowtimeEntity
    {
        public ShowtimeEntity()
        {
        }

        public ShowtimeEntity(int id, MovieEntity movie, DateTime sessionDate, int auditoriumId)
        {
            Id = id;
            Movie = movie;
            SessionDate = sessionDate;
            AuditoriumId = auditoriumId;
        }

        public int Id { get; set; }
        public MovieEntity Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
        public ICollection<TicketEntity> Tickets { get; set; }
    }
}
