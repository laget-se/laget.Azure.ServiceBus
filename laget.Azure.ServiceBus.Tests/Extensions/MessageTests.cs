﻿using Azure.Messaging.ServiceBus;
using laget.Azure.ServiceBus.Extensions;
using System;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Extensions
{
    public class MessageTests
    {
        //public MessageTests()
        //{
        //    // One-time setup
        //    // Only use this setup for testing, do not attempt to set system properties in production
        //    _serviceBusMessage = new ServiceBusMessage { MessageId = "SomeCoolId1" };

        //    var systemProperties = new Microsoft.Azure.ServiceBus.Message.SystemPropertiesCollection();
        //    var bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty;

        //    var sequenceNumber = 1;
        //    var enqueueTime = DateTime.Now.AddSeconds(5);
        //    systemProperties.GetType().InvokeMember("EnqueuedTimeUtc", bindings, Type.DefaultBinder, systemProperties, new object[] { enqueueTime });
        //    systemProperties.GetType().InvokeMember("SequenceNumber", bindings, Type.DefaultBinder, systemProperties, new object[] { sequenceNumber });

        //    // Set mocked-up system properties for current message
        //    bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty;
        //    _serviceBusMessage.GetType().InvokeMember("SystemProperties", bindings, Type.DefaultBinder, _serviceBusMessage, new object[] { systemProperties });
        //}

        [Fact]
        public void ShouldDeserializeMessage()
        {
            var body = new BinaryData("{ \"Name\": \"Jane Doe\" }".ToBytes());
            var serviceBusMessage = new ServiceBusMessage(body);
            var model = serviceBusMessage.Deserialize<Models.User>();

            Assert.Equal("Jane Doe", model.Name);
        }

        [Fact]
        public void ShouldConvertMessage()
        {
            var userMessage = new Models.User
            {
                Category = "UserCategory",
                CreatedAt = DateTime.Now,
                Id = "UserId1",
                Name = "Jane Doe",
                Type = "UserType"
            };

            var serviceBusMessage = new ServiceBusMessage(userMessage.ToBytes());
            var model = serviceBusMessage.Deserialize<Models.User>();

            Assert.Equal("Jane Doe", model.Name);
        }
    }
}
