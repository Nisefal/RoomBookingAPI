using System;
using System.Collections.Generic;

#nullable disable

namespace RoomBookingAPI
{
    public partial class Token
    {
        public int AccountId { get; set; }
        public string Token1 { get; set; }
        public DateTime Expires { get; set; }
    }
}
