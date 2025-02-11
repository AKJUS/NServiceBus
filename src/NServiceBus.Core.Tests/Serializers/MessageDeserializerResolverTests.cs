﻿namespace NServiceBus.Core.Tests.Serializers;

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Serialization;

[TestFixture]
public class MessageDeserializerResolverTests
{
    [TestCase(ContentTypes.Xml)]
    [TestCase(ContentTypes.Json)]
    public void RetrievesSerializerByContentType(string contentType)
    {
        var expectedResolver = new FakeSerializer(contentType);
        var resolver = new MessageDeserializerResolver(new FakeSerializer("default"), new IMessageSerializer[]
        {
            new FakeSerializer("some/content/type"),
            expectedResolver,
            new FakeSerializer("another/content/type")
        });

        var headers = new Dictionary<string, string>
        {
            {Headers.ContentType, contentType}
        };
        var serializer = resolver.Resolve(headers);
        Assert.That(serializer, Is.SameAs(expectedResolver));
    }

    [Test]
    public void UnknownContentTypeFallsBackToDefaultSerialization()
    {
        var mainSerializer = new FakeSerializer(ContentTypes.Xml);
        var resolver = new MessageDeserializerResolver(mainSerializer, new IMessageSerializer[]
        {
            new FakeSerializer(ContentTypes.Json)
        });

        var headers = new Dictionary<string, string>
        {
            {Headers.ContentType, "unknown/unsupported"}
        };
        var serializer = resolver.Resolve(headers);

        Assert.That(serializer, Is.SameAs(mainSerializer));
    }

    [Test]
    public void NoContentTypeFallsBackToDefaultSerialization()
    {
        var mainSerializer = new FakeSerializer(ContentTypes.Xml);
        var resolver = new MessageDeserializerResolver(mainSerializer, new IMessageSerializer[]
        {
            new FakeSerializer(ContentTypes.Json)
        });

        var serializer = resolver.Resolve([]);

        Assert.That(serializer, Is.EqualTo(mainSerializer));
    }

    [TestCase(null)]
    [TestCase("")]
    public void EmptyContentTypeFallsBackToDefaultSerialization(string headerValue)
    {
        var mainSerializer = new FakeSerializer(ContentTypes.Xml);
        var resolver = new MessageDeserializerResolver(mainSerializer, new IMessageSerializer[]
        {
            new FakeSerializer(ContentTypes.Json)
        });

        var serializer = resolver.Resolve(new Dictionary<string, string>()
        {
            { Headers.ContentType, headerValue}
        });

        Assert.That(serializer, Is.EqualTo(mainSerializer));
    }

    [Test]
    public void MultipleDeserializersWithSameContentTypeShouldThrowException()
    {
        var deserializer1 = new FakeSerializer("my/content/type");
        var deserializer2 = new FakeSerializer("my/content/type");

        Assert.That(() => new MessageDeserializerResolver(new FakeSerializer("xml"), new IMessageSerializer[]
        {
            deserializer1,
            deserializer2
        }), Throws.Exception.TypeOf<Exception>().And.Message.Contains($"Multiple deserializers are registered for content-type '{deserializer1.ContentType}'. Remove ambiguous deserializers."));
    }

    class FakeSerializer : IMessageSerializer
    {
        public FakeSerializer(string contentType)
        {
            ContentType = contentType;
        }

        public void Serialize(object message, Stream stream)
        {
        }

        public object[] Deserialize(ReadOnlyMemory<byte> body, IList<Type> messageTypes = null)
        {
            throw new NotImplementedException();
        }

        public string ContentType { get; }
    }
}