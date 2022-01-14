using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoomBookingAPI.Model;
using Microsoft.Extensions.Logging;

namespace RoomBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly Authentication auth;
        private MyDbContext context;

        public RoomsController(ILogger<RoomsController> logger)
        {
            _logger = logger;
            context = MyDbContext.GetDbContext();
            auth = new Authentication();
        }

        [HttpPost]
        [Route("api/[controller]/AddRoom")]
        public async Task AddRoom([FromBody] AddRoomPostBody data)
        {
            try
            {
                if (!auth.TryCheckToken(data.token))
                    return;

                if (context.Rooms.AsQueryable().Any(r => r.Number == data.Number))
                    return;

                var room = new Room()
                {
                    Floor = data.Floor,
                    Number = data.Number,
                    RoomsNumber = data.RoomsNumber
                };
                context.Rooms.Add(room);

                context.SaveChanges();
            }
            catch(Exception e)
            {
                _logger.LogDebug(e.Message);
            }
        }

        [HttpPost]
        [Route("api/[controller]/ModifyRoom")]
        public async Task ModifyRoom([FromBody] ModifyRoomPostBody data)
        {
            try
            {
                if (!auth.TryCheckToken(data.token))
                    return;

                var room = context.Rooms.AsQueryable().FirstOrDefault(r => r.Number == data.SearchRoomNumber);

                if (room == null)
                    return;

                room.Floor = data.NewFloor;
                room.Number = data.NewNumber;
                room.RoomsNumber = data.NewRoomsNumber;

                context.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogDebug(e.Message);
            }
        }

        [HttpPost]
        [Route("api/[controller]/DeleteRoom")]
        public async Task DeleteRoom([FromBody] DeleteRoomPostBody data)
        {
            try
            {
                if (!auth.TryCheckToken(data.token))
                    return;

                var room = context.Rooms.AsQueryable().FirstOrDefault(r => r.Number == data.SearchRoomNumber);

                if (room == null)
                    return;

                context.Rooms.Remove(room);

                context.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogDebug(e.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/FindRooms")]
        public async Task<IQueryable<Room>> FindRooms(int NumberRooms = 0, int Floor = 0, DateTime? From = null, DateTime? To = null)
        {
            var rooms = context.Rooms.AsQueryable();

            if (From < To)
                return null;

            if (NumberRooms > 0)
                rooms = rooms.Where(r => r.RoomsNumber == NumberRooms);

            if (Floor > 0)
                rooms = rooms.Where(r => r.Floor == Floor);

            if(From != null && To != null)
            {
                rooms = from r in rooms
                        join b in context.Bookings.AsQueryable() on r.Number equals b.RoomNumber into rb
                        from sub in rb.DefaultIfEmpty()
                        where (sub.DateFrom != null && sub.DateFrom > To) || (sub.DateTo != null && sub.DateTo < From) || (sub.DateFrom == null && sub.DateTo == null)
                        select r;
            }

            return rooms;
        }
    }

    public class AddRoomPostBody
    {
        public string token { get; set; }
        public int RoomsNumber { get; set; }
        public int Floor { get; set; }
        public int Number { get; set; }
    }

    public class ModifyRoomPostBody
    {
        public string token { get; set; }
        public int SearchRoomNumber { get; set; }
        public int NewRoomsNumber { get; set; }
        public int NewFloor { get; set; }
        public int NewNumber { get; set; }
    }

    public class DeleteRoomPostBody
    {
        public string token { get; set; }
        public int SearchRoomNumber { get; set; }
    }
}
