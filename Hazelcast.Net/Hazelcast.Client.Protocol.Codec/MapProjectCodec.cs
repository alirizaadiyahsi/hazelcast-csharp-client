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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.4 - Update:1.4
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapProjectCodec
    {
        private static int CalculateRequestDataSize(string name, IData projection)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += ParameterUtil.CalculateDataSize(projection);
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, IData projection)
        {
            var requiredDataSize = CalculateRequestDataSize(name, projection);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapProject);
            clientMessage.SetRetryable(true);
            clientMessage.Set(name);
            clientMessage.Set(projection);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public IList<IData> response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var responseSize = clientMessage.GetInt();
            var response = new List<IData>(responseSize);
            for (var responseIndex = 0; responseIndex < responseSize; responseIndex++)
            {
                var responseItemIsNull = clientMessage.GetBoolean();
                if (!responseItemIsNull)
                {
                    var responseItem = clientMessage.GetData();
                    response.Add(responseItem);
                }
                else
                {
                    response.Add(null);
                }
            }
            parameters.response = response;
            return parameters;
        }
    }
}