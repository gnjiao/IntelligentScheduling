﻿using DeviceAsset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<AGVNode> gVNodes = new List<AGVNode>()
            {
            new AGVNode(){ StationName="Wait_D",Action=AGVAction.Wait,OrderName="Wait_Order"}
            };

            Client client = new Client();


            var transportOrder = new TransportOrder()
            {

                Deadline = DateTime.UtcNow.AddMinutes(20),
                Destinations = new List<DestinationOrder>()
                {
                    new DestinationOrder(){ LocationName="Wait_D",Operation="JackUnload",
                        Properties =new List<Property>()
                         {
                        new Property(){ Key="device:queryAtExecuted",Value="WaitQuery:wait"}
                            }
                    },

                    new DestinationOrder(){ LocationName="C",Operation="JackLoad",Properties=new List<Property>(){
                    }},

                },
                Properties = new List<Property>(),
                Dependencies = new List<string>()
            };

            string name = $"Wait_Order_{DateTime.Now.ToShortTimeString()}";
            Console.WriteLine(name);
            client.TransportOrders2(name, transportOrder);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
