﻿using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

#pragma warning disable 8618

public abstract class BaseQueueReciever
{
    protected static FundingConfig _fundingConfig;

    public static void SetConfig(FundingConfig fundingConfig)
    {
        _fundingConfig = fundingConfig;
    }

    protected static async Task<List<T>> GetMatchingMessages<T>(string azureServiceBusNamespace, string queueName, Func<T, bool> predicate)
    {
        var matchingMessages = new List<T>();

        var client = new ServiceBusClient(azureServiceBusNamespace, new DefaultAzureCredential());

        var receiver = client.CreateReceiver(queueName);

        try
        {
            matchingMessages = await GetMessages(receiver, predicate);
        }
        catch (Exception ex)
        {
            throw new Exception("QueueReciever failed to retrieve messages", ex);
        }
        finally
        {
            await receiver.DisposeAsync();
            await client.DisposeAsync();
        }

        return matchingMessages;
    }

    private static async Task<List<T>> GetMessages<T>(ServiceBusReceiver receiver, Func<T, bool> predicate)
    {
        var events = await receiver.ReceiveMessagesAsync(10000);

        var deserializedMessages = events.Select(x => SafeParse<T>(x)).Where(x => x != null).ToList();
        var matchingMessages = deserializedMessages.Where(x => predicate(x!.Message)).ToList();

        matchingMessages.ForEach(x => receiver.CompleteMessageAsync(x!.EventMessage));
        return matchingMessages.Select(x => x!.Message).ToList();
    }

    /// <summary>
    /// If the json cannot be deserialized into the expected type, then message will be abandoned
    /// </summary>
    private static DeserializedMessageContainer<T>? SafeParse<T>(ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        var messageContainer = new DeserializedMessageContainer<T>
        {
            EventMessage = serviceBusReceivedMessage
        };

        try
        {
            string messageBody = serviceBusReceivedMessage.Body.ToString();
            messageBody = messageBody.Trim('\uFEFF', '\u200B', '\u0000');// Remove Byte Order Mark (BOM) 
            messageContainer.Message = JsonConvert.DeserializeObject<T>(messageBody)!;
        }
        catch
        {
            return null;
        }

        return messageContainer;
    }
}

public class DeserializedMessageContainer<T>
{
    public ServiceBusReceivedMessage EventMessage { get; set; }
    public T Message { get; set; }
}

#pragma warning restore 8618