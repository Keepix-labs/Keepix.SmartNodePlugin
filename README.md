# Keepix Smart Node Plugin

## Pre-requies

Install Dotnet  
`dotnet version 7.0.402`  
- Linux: (source: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual)  
`wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh`  
`chmod +x ./dotnet-install.sh`  
`./dotnet-install.sh --version 7.0.402`  
`./dotnet-install.sh --version 7.0.402 --runtime aspnetcore`  
`./dotnet-install.sh --channel 7.0.402`  
`export DOTNET_ROOT=$HOME/.dotnet`  
`export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools`  
- Macos: (source: https://github.com/isen-ng/homebrew-dotnet-sdk-versions)  
`brew tap isen-ng/dotnet-sdk-versions`  
`brew install --cask 7.0.402`  
see your installed versions: `dotnet --list-sdks`   
- Windows:  
Install it via "Visual Studio" and if necessary change the version 7.? on globals.json  

Install nodejs v18  
`curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash`  
`nvm install v18`  
`nvm use v18`  

# Install

`npm install`  
`dotnet restore`  

## Run

`npm run start`  

Go on http://localhost:3000
Also an api mock of keepix-server are available on http://localhost:2000/plugins/Keepix.SmartNodePlugin/status like routes where you can see on keepix-server.  

## Publish Release

public/package.json are used for publish the build of the plugin.  
  
The plugin is loaded by the keepix-server.
Warn attention to set the path of the plugin correctly on the package.json at the build step like this PUBLIC_URL=/plugins/Keepix.SmartNodePlugin/view

`npm run build`  
`npm run publish`  

## Documentation Section

### Plugin Dotnet

The plugin using the Keepix.PluginSystem library who override the STDout and STDerr and format an result like this:  
`{ "result": "{ "x": "x" }", "stdOut": "logs" }`  

The plugin using the [KeepixPluginFn("function-name")] annotation for exposing functions.

Example of input for running manually one function:
`./Plugin.SmartNodePlugin '{ "key": "install", "data1": "a", "data2": "b" }'` 

Example of query for running from the front-end one function on the debug server (Runned at the npm run start moment):  
  
GET http://localhost:2000/plugins/Keepix.SmartNodePlugin/status  
Result: { ... }  
  
POST http://localhost:2000/plugins/Keepix.SmartNodePlugin/install?async=true  
Body: { "key": "install", "data1": "a", "data2": "b" }  
Result: { "taskId": "Keepix.SmartNodePlugin-install" }

GET http://localhost:2000/plugins/Keepix.SmartNodePlugin/watch/task/Keepix.SmartNodePlugin-install  
Result: { "status": "RUNNING" }
Result: { "status": "ERROR", "description": "" }
Result: { "status": "FINISHED", "description": "" }

### Plugin LifeCycle Required Exposed Functions

Install function called at the installation time  
`[KeepixPluginFn("install")]`  
Uninstall function called at the uninstallation time  
`[KeepixPluginFn("uninstall")]`  

### Plugin Front-end

The plugin need a front-end static code in the final build directory index.html file  
Here we are using a React framework  
The Front-end application will be loaded by the Keepix with an iframe at the following endpoint url:  
  
`http|https://hostname/plugins/Keepix.SmartNodePlugin/view`  

Here the plugin Name is 'Keepix.SmartNodePlugin' but in function of your plugin name change this line in the package.json:  
  
`PUBLIC_URL=/plugins/Keepix.SmartNodePlugin/view react-app-rewired build`  

For developping locally your plugin on the config-overrides.js you can see a  
express.js server simulation the routes of the real keepix server  

`GET /plugins/nameOfThePlugin/:key`  
`POST /plugins/nameOfThePlugin/:key`  
`GET /plugins/nameOfThePlugin/watch/tasks/:taskId`  

Enjoy now you are ready for build a new Keepix plugin, Nice Coding!  

## Contributors

Jeremy Guyet
Frederic Marinho
