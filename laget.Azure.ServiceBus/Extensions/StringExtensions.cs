﻿using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using System;
using System.Text;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class StringExtensions
    {
        internal static byte[] ToBytes(this string @string)
        {
            return Encoding.UTF8.GetBytes(@string);
        }

        internal static async Task<ServiceBusMessage> ToServiceBusMessageAsync(this string @string, BlobContainerClient blobContainerClient, string queueOrTopicName)
        {
            var bytes = @string.ToBytes();
            if (bytes.Length > MessageConstants.MaxMessageSize)
            {
                if (blobContainerClient == null)
                {
                    throw new ArgumentException($"The body is too large, maximum size is ${MessageConstants.MaxMessageSize / 1024}KB");
                }

                var blobName = Guid.NewGuid().ToString();
                await blobContainerClient.UploadBlobAsync($"{queueOrTopicName}/{blobName}", new BinaryData(bytes));
                var msg = new ServiceBusMessage();
                msg.ApplicationProperties.Add(MessageConstants.BlobIdHeader, blobName);
                return msg;
            }

            return new ServiceBusMessage(bytes);
        }
    }
}
