﻿using Microsoft.AspNetCore.Mvc;
using System;
using WebApiOrleans.Grains;

namespace WebApiOrleans.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<UtilController> _logger;
    private readonly IChatObserver _chatObserver;

    public UtilController(
        IGrainFactory grainFactory,
        ILogger<UtilController> logger,
        IChatObserver chatObserver)
    {
        _grainFactory = grainFactory;
        _logger = logger;
        _chatObserver = chatObserver;
    }

    [HttpGet("ping-grain")]
    public async Task<IActionResult> PingGrain()
    {
        var grain = _grainFactory.GetGrain<IPingGrain>(Guid.NewGuid());
        await grain.Ping();

        return Ok();
    }

    private readonly string _reminderGuid = Guid.Parse("201A46ED-A443-4605-AC59-B627314A0611").ToString();

    [HttpGet("reminder-start")]
    public async Task<IActionResult> ReminderStart()
    {
        var grain = _grainFactory.GetGrain<IReminderGrain>(_reminderGuid);
        await grain.StartMessage();

        return Ok();
    }

    [HttpGet("reminder-stop")]
    public async Task<IActionResult> ReminderStop()
    {
        var grain = _grainFactory.GetGrain<IReminderGrain>(_reminderGuid);
        await grain.StopMessage();

        return Ok();
    }

    [HttpGet("chat-demo")]
    public async Task<IActionResult> ChatSubscribe()
    {
        var client1 = _grainFactory.GetGrain<IClientChatGrain>("user1");
        var client2 = _grainFactory.GetGrain<IClientChatGrain>("user2");

        //Create a reference for chat, usable for subscribing to the observable grain.
        var reference = _grainFactory.CreateObjectReference<IChatObserver>(_chatObserver);

        //Subscribe the instance to receive messages.
        await client1.Subscribe(reference);
        await client2.Subscribe(reference);

        await client1.SendUpdateMessage("Hello, from Client 1");
        await client2.SendUpdateMessage("Hello, from Client 2");

        return Ok();
    }


    [HttpGet("chat-subscribe/{user}")]
    public async Task<IActionResult> ChatSubscribe(string user)
    {
        var client = _grainFactory.GetGrain<IClientChatGrain>(user);

        //Create a reference for chat, usable for subscribing to the observable grain.
        var reference = _grainFactory.CreateObjectReference<IChatObserver>(_chatObserver);

        //Subscribe the instance to receive messages.
        await client.Subscribe(reference);

        return Ok();
    }

    [HttpGet("chat-message/{user}/{message}")]
    public async Task<IActionResult> ChatMessage(string user, string message)
    {
        var client = _grainFactory.GetGrain<IClientChatGrain>(user);

        //Subscribe the instance to receive messages.
        await client.SendUpdateMessage(message);

        return Ok();
    }

    [HttpGet("parent-child/{parentId:int}/{message}")]
    public async Task<IActionResult> ParentChild(int parentId, string message)
    {
        var parentGrain = _grainFactory.GetGrain<IParentGrain>(parentId);
        await parentGrain.SayHello(message);
        return Ok();
    }


    [HttpGet("streaming")]
    public async Task<IActionResult> Streaming()
    {
        string streamProviderName = "StreamProvider";
        string streamNamespace = "StreamNamespace";
        Guid streamGuid = Guid.NewGuid();


        var producer = _grainFactory.GetGrain<IStreamProducerGrain>(0);
        var consumer = _grainFactory.GetGrain<IStreamConsumerGrain>(0);
        await producer.StartStreaming(streamProviderName, streamNamespace, streamGuid);
        await consumer.StartConsume(streamProviderName, streamNamespace, streamGuid);

        return Ok();
    }
}
