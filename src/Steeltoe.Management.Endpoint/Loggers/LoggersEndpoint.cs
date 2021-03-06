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

using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Logging.CloudFoundry;
using System;
using System.Collections.Generic;


namespace Steeltoe.Management.Endpoint.Loggers
{
    public class LoggersEndpoint : AbstractEndpoint<Dictionary<string, object>, LoggersChangeRequest>
    {
        ILogger<LoggersEndpoint> _logger;
        protected new ILoggersOptions Options
        {
            get
            {
                return options as ILoggersOptions;
            }
        }

        public LoggersEndpoint(ILoggersOptions options, ILogger<LoggersEndpoint> logger = null) : base(options)
        {
            _logger = logger;
        }

        public override Dictionary<string, object> Invoke(LoggersChangeRequest request)
        {
            _logger.LogDebug("Invoke({0})", request);

            return DoInvoke(CloudFoundryLoggerProvider.Instance, request);

        }

        public virtual Dictionary<string, object> DoInvoke(ICloudFoundryLoggerProvider provider, LoggersChangeRequest request)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (request != null)
            {
                SetLogLevel(provider, request.Name, request.Level);
            }
            else
            {
                AddLevels(result);
                var configuration = GetLoggerConfigurations(provider);
                Dictionary<string, LoggerLevels> loggers = new Dictionary<string, LoggerLevels>();
                foreach (var c in configuration)
                {
                    _logger.LogTrace("Adding " + c.ToString());
                    LoggerLevels lv = new LoggerLevels(c.ConfiguredLevel, c.EffectiveLevel);
                    loggers.Add(c.Name, lv);
                }
                result.Add("loggers", loggers);
            }

            return result;
        }

        public virtual void AddLevels(Dictionary<string, object> result)
        {
            result.Add("levels", levels);
        }

        public virtual ICollection<ILoggerConfiguration> GetLoggerConfigurations(ICloudFoundryLoggerProvider provider)
        {
            if (provider == null)
            {
                _logger?.LogInformation("Unable to access Cloud Foundry Logging provider, log configuration unavailable");
                return new List<ILoggerConfiguration>();
            }
            return provider.GetLoggerConfigurations();
        }

        public virtual void SetLogLevel(ICloudFoundryLoggerProvider provider, string name, string level)
        {
            if (provider == null)
            {
                _logger?.LogInformation("Unable to access Cloud Foundry Logging provider, log level not changed");
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (string.IsNullOrEmpty(level))
            {
                throw new ArgumentException(nameof(level));
            }

            provider.SetLogLevel(name, LoggerLevels.MapLogLevel(level));
        }

        private static List<string> levels = new List<string>() {
            LoggerLevels.MapLogLevel(LogLevel.None),
            LoggerLevels.MapLogLevel(LogLevel.Critical),
            LoggerLevels.MapLogLevel(LogLevel.Error),
            LoggerLevels.MapLogLevel(LogLevel.Warning),
            LoggerLevels.MapLogLevel(LogLevel.Information),
            LoggerLevels.MapLogLevel(LogLevel.Debug),
            LoggerLevels.MapLogLevel(LogLevel.Trace)
        };
    }
}
