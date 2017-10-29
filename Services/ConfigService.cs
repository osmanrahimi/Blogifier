﻿using Microsoft.Extensions.Configuration;

namespace Blogifier.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IConfiguration _config;

        public ConfigService(IConfiguration config)
        {
            _config = config;
        }

        public string GetSetting(string key)
        {
            return _config.GetSection("Blogifier").GetValue<string>(key);
        }
    }
}
