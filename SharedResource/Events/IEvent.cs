﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedResource.Events
{
    public interface IEvent
    {
        public string EventType { get; set; }
    }
}
