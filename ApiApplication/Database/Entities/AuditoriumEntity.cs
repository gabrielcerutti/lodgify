﻿namespace Showtime.Api.Database.Entities
{
    public class AuditoriumEntity
    {
        public int Id { get; set; }
        public List<ShowtimeEntity> Showtimes { get; set; }
        public ICollection<SeatEntity> Seats { get; set; }

    }
}
