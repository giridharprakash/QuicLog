using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Json;

namespace QuicLog.Core.Logging
{
    public static class QuicLogger
    {
        private static IConfiguration _configuration;
        private static QuicLogConfigOptions _options;

        public static void Configure( string configSection = "QuicLog")
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: false)
                .Build();
            _options = QuicLogConfigOptions.Create(_configuration, configSection);
            CreateLogger();
        }
        public static void LogAndFlush()
        {
            Log.CloseAndFlush();
        }
        public static void Info(string message)
        {
            Log.Logger.Information(message);
            LogAndFlush();
        }
        private static void CreateLogger()
        {
            Log.Logger = CreateLoggerFromConfiguration();
        }

        private static ILogger CreateLoggerFromConfiguration()
        {
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration);
            if (_options.IsApplicationInsights)
                return CreateApplicationInsightsLogger(loggerConfig);
            if (_options.IsRollingFile)
                return CreateRollingFileLogger(loggerConfig);
            if (_options.IsConsole)
                return CreateConsoleLogger(loggerConfig);
            return CreateConsoleLogger(loggerConfig);
        }

        private static ILogger CreateConsoleLogger(LoggerConfiguration loggerConfig)
        {
            return loggerConfig.WriteTo.Console().CreateLogger();
        }

        private static ILogger CreateRollingFileLogger(LoggerConfiguration loggerConfig)
        {
            return loggerConfig.WriteTo.RollingFile( new JsonFormatter(), _options.RollingFilePath).CreateLogger();
        }

        private static ILogger CreateApplicationInsightsLogger(LoggerConfiguration loggerConfig)
        {
            return loggerConfig.WriteTo.ApplicationInsights(_options.InstrumentationKey,null).CreateLogger();
        }
    }
}