﻿//
// Copyright 2017 the original author or authors.
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

using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Steeltoe.Management.Endpoint.Info.Contributor.Test
{
    public class AppSettingsInfoContributorTest : BaseTest
    {
        [Fact]
        public void ConstributeWithConfigNull()
        {
            var contributor = new AppSettingsInfoContributor(null);
            InfoBuilder builder = new InfoBuilder();
            contributor.Contribute(builder);
            var result = builder.Build();
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);

        }

        [Fact]
        public void ContributeWithNullBUilderThrows()
        {
            var appsettings = @"
{
    'info': {
        'application': {
            'name': 'foobar',
            'version': '1.0.0',
            'date': '5/1/2008',
            'time' : '8:30:52 AM'
        },
        'NET' : {
            'type': 'Core',
            'version': '1.1.0'
        }
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();
            var settings = new AppSettingsInfoContributor(config);

            Assert.Throws<ArgumentNullException>(() => settings.Contribute(null));
        }
        [Fact]
        public void ContributeAddsToBuilder()
        {
            var appsettings = @"
{
    'info': {
        'application': {
            'name': 'foobar',
            'version': '1.0.0',
            'date': '5/1/2008',
            'time' : '8:30:52 AM'
        },
        'NET': {
            'type': 'Core',
            'version': '1.1.0',
            'ASPNET' : {
                'type': 'Core',
                'version': '1.1.0'
            }
        }
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();
            var settings = new AppSettingsInfoContributor(config);

            InfoBuilder builder = new InfoBuilder();
            settings.Contribute(builder);

            Dictionary<string,object> info = builder.Build();
            Assert.NotNull(info);
            Assert.Equal(2, info.Count);
            Assert.True(info.ContainsKey("application"));
            Assert.True(info.ContainsKey("NET"));

            var appNode = info["application"] as Dictionary<string, object>;
            Assert.NotNull(appNode);
            Assert.Equal("foobar", appNode["name"]);

            var netNode = info["NET"] as Dictionary<string, object>;
            Assert.NotNull(netNode);
            Assert.Equal("Core", netNode["type"]);

            Assert.NotNull(netNode["ASPNET"]);
        }

    }
}
