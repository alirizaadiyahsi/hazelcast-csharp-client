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

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal class AddressProvider
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(AddressProvider));

        private readonly HazelcastCloudDiscovery _hzCloudDiscovery;
        private readonly GetAddressDictionary _addressProviderFn;
        private readonly IList<string> _configAddresses;

        private IDictionary<Address, Address> _privateToPublic;
        private readonly bool _canTranslate;


        public AddressProvider(ClientConfig clientConfig)
        {
            var networkConfig = clientConfig.GetNetworkConfig();
            var configAddressList = networkConfig.GetAddresses();
            var cloudConfig = networkConfig.GetCloudConfig();

            //Fail fast validate multiple setup
            if (configAddressList.Count > 0 && cloudConfig != null && cloudConfig.IsEnabled())
            {
                throw new ConfigurationException("Only one address configuration method can be enabled at a time.");
            }

            if (cloudConfig != null && cloudConfig.IsEnabled())
            {
                var token = cloudConfig.GetDiscoveryToken();
                var connectionTimeoutInMillis = networkConfig.GetConnectionTimeout();
                connectionTimeoutInMillis = connectionTimeoutInMillis == 0 ? int.MaxValue : connectionTimeoutInMillis;
                    
                var urlBase = Environment.GetEnvironmentVariable(HazelcastCloudDiscovery.CloudUrlBaseProperty);
                
                _hzCloudDiscovery = new HazelcastCloudDiscovery(token, connectionTimeoutInMillis, urlBase??HazelcastCloudDiscovery.CloudUrlBase);
                _addressProviderFn = GetHzCloudConfigAddresses;
                _canTranslate = true;
            }
            else
            {
                _configAddresses = configAddressList.Count > 0 ? configAddressList : new List<string>{"localhost"};
                _addressProviderFn = GetHzConfigAddresses;
            }
        }

        public IEnumerable<Address> GetAddresses()
        {
            if (_privateToPublic == null)
            {
                Refresh();
            }
            return _privateToPublic != null ? _privateToPublic.Keys : Enumerable.Empty<Address>();
        }

        private void Refresh()
        {
            try
            {
                _privateToPublic = _addressProviderFn();
            }
            catch (Exception e)
            {
                Logger.Warning("Address provider failed to load addresses: ", e);
            }
        }

        public Address TranslateToPublic(Address address)
        {
            if (!_canTranslate || address == null)
            {
                return address;
            }
            Address publicAddress;
            if (_privateToPublic != null && _privateToPublic.TryGetValue(address, out publicAddress))
            {
                return publicAddress;
            }
            Refresh();
            return _privateToPublic != null && _privateToPublic.TryGetValue(address, out publicAddress) ? publicAddress : null;
        }

        //Config address provider
        private IDictionary<Address, Address> GetHzConfigAddresses()
        {
            var possibleAddresses = new List<Address>();
            foreach (var cfgAddress in _configAddresses)
            {
                possibleAddresses.AddRange(AddressUtil.ParsePossibleAddresses(cfgAddress));
            }
            return possibleAddresses.ToDictionary(address => address, address => address);
        }

        //Hz cloud address provider
        private IDictionary<Address, Address> GetHzCloudConfigAddresses()
        {
            return _hzCloudDiscovery.DiscoverNodes();
        }
    }
}