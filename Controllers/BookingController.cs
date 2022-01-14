using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private MyDbContext context;

        public BookingController(ILogger<BookingController> logger)
        {
            _logger = logger;
            context = MyDbContext.GetDbContext();
        }


        [HttpPost]
        [Route("api/[controller]/BookRoom")]
        public async Task BookRoom([FromBody] BookRoomPostBody data)
        {
            try
            {
                var room = context.Rooms.AsQueryable().FirstOrDefault(r => r.Number == data.RoomNumber);
                var customer = context.Customers.AsQueryable().FirstOrDefault(c => c.Id == data.CustomerId);

                if (room == null || customer == null)
                    return;

                if (data.From > data.To)
                    return;

                var check = from r in context.Rooms
                              join b in context.Bookings on r.Number equals b.RoomNumber
                              where b.DateFrom <= data.To || b.DateTo >= data.From
                              select r;

                if (check != null)
                    return;

                var booking = new Booking()
                {
                    CustomerId = data.CustomerId,
                    RoomNumber = data.RoomNumber,
                    DateFrom = data.From,
                    DateTo = data.To,
                    Status = "Active"
                };

                context.Bookings.Add(booking);

                context.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogDebug(e.Message);
            }
        }
    }

    public class BookRoomPostBody
    {
        public int RoomNumber { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int CustomerId { get; set; }
    }
}
