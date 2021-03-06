﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderDistribution
{
    public enum OrderServiceStateEnum
    {
        OFF,
        FATAL,
        ERROR,
        WARN,
        INFO,
        DEBUG
    }

    public class OrderServiceState
    {
        public OrderServiceStateEnum State { get; set; }

        public string Message { get; set; }

        public OrderServiceErrorCodeEnum ErrorCode { get; set; }

        public override string ToString()
        {
            return $"【ProcessInfo】【{State}】{Message} {ErrorCode}- {DateTime.Now}";
        }

    }
}
