
using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace RabbitMqTools.Conductor.Azure
{
    public static class AzureFactory
    {
        public static IAzure GetAzure()
        { 
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(
                Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            return azure;
        }
    }
}