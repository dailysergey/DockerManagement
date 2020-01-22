﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PortainerApi.Models.Auth
{

    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthSuccess
    {
        public string JWT { get; set; }
    }
}
