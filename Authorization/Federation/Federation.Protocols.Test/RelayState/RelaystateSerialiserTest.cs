﻿using System.Threading.Tasks;
using DeflateCompression;
using Federation.Protocols.Endocing;
using Federation.Protocols.RelayState;
using Kernel.Federation.Protocols;
using NUnit.Framework;
using Serialisation.JSON;
using Serialisation.JSON.SettingsProviders;

namespace Federation.Protocols.Test.RelayState
{
    [TestFixture]
    internal class RelayStateSerialiserTest
    {
        [Test]
        public async Task SerialiseDeserialiseTest()
        {
            //ARRANGE
            var relayState = "Test state";
            var compressor = new DeflateCompressor();
            var messageEncoder = new MessageEncoding(compressor);
            var jsonSerialiser = new NSJsonSerializer(new DefaultSettingsProvider());
            var serialiser = new RelaystateSerialiser(jsonSerialiser, messageEncoder) as IRelayStateSerialiser;
            //ACT
            var serialised = await serialiser.Serialize(relayState);
            var deserialised = await serialiser.Deserialize(serialised);
            //ASSERT
            Assert.AreEqual(relayState, deserialised);
        }
    }
}