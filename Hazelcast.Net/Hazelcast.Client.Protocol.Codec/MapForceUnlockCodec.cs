// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.2
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapForceUnlockCodec
    {
        private static int CalculateRequestDataSize(string name, IData key, long referenceId)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += ParameterUtil.CalculateDataSize(key);
            dataSize += Bits.LongSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, IData key, long referenceId)
        {
            var requiredDataSize = CalculateRequestDataSize(name, key, referenceId);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapForceUnlock);
            clientMessage.SetRetryable(true);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.Set(referenceId);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE IS EMPTY *****************//
    }
}