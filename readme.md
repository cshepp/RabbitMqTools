# RmqTools

## Overview

This repo contains several projects that aim to help the user experiment with RabbitMQ.

Tools included:

- Conductor: a command line app that creates and manages temporary RabbitMQ infrastructure in Azure
- Consumer: a console app that consumes messages and is configurable at runtime (via an HTTP API)
- Producer: a console app that produces messages and is configurable at runtime (via an HTTP API)

## Setup

These instructions are for Windows only. Sorry. (I'm open to pull requests for linux/macOS!)

Before you can use Conductor to create networks and VMs in Azure, you need to setup your development environment. 

In Powershell:

```powershell
# Install the Azure Powershell module
Install-Module -Name AzureRM -AllowClobber

# Allow downloaded modules to run
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

Import-Module -Name AzureRM

# Running this will open IE and allow you to sign into Azure
# Take note of the output....you'll need the SubscriptionId/TenantId later
Login-AzureRmAccount

$password = ConvertTo-SecureString "<your password here>" -AsPlainText -Force
$sp = New-AzureRmADServicePrincipal -DisplayName "<display name here>" -Password $password

# Take note of the output of these two commands...you'll need to reference them
New-AzureRmRoleAssignment -ServicePrincipalName $sp.ApplicationId -RoleDefinitionName Contributor
$sp | Select DisplayName, ApplicationId
```

Create a file called `azureauth.properties` somewhere secure, and add the follow information:

```
subscription=<subscription_id>
client=<client_id>
key=<password>
tenant=<tenant_id>
managementURI=https://management.core.windows.net/
baseURL=https://management.azure.com/
authURL=https://login.windows.net/
graphURL=https://graph.windows.net/
```

- `<subscription_id>` is noted in the output of the `Login-AzureRmAccount` command
- `<client_id>` is the ApplicationId in the command output
- `<password>` is whatever you supplied for `<your password here>` in the commands above
- `<tenant_id>` is noted in the output of the `Login-AzureRmAccount` command

Finally, back in Powershell, run the following:

```powershell
[Environment]::SetEnvironmentVariable("AZURE_AUTH_LOCATION", "C:\path\to\your\azureauth.properties", "User")
```

Now Conductor should be able to authenticate with Azure.

## Usage

TODO

- Modify config files
- ...todo