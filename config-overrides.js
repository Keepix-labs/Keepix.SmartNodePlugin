const path = require('path');
const http = require('http');
const express = require('express');
const fs = require('fs');
const { exec } = require('child_process');

const pluginId = fs.readdirSync('.').filter(x => x.endsWith('.csproj')).map(x => x.replace('.csproj', ''))[0];

const execPluginFunction = async (argObject) => {
    return await new Promise((resolve) => {
        // double stringify for escapes double quotes
        exec(`./dist/${pluginId} ${JSON.stringify(JSON.stringify(argObject))}`, (error, stdout, stderr) => {
            const result = JSON.parse(stdout);

            resolve({
                result: JSON.parse(result.jsonResult),
                stdOut: result.stdOut
            });
        });
    });
};

const executeServer = () => {
    if (!process.argv.join(' ').includes('start.js')) {
        return ;
    }
    console.log('Start Dev Server.');

    const app = express();
    app.set('port', 2000);
    app.set('host', '0.0.0.0');

    app.get(`/plugins/${pluginId}/:key`, async (req, res) => {

        const result = await execPluginFunction({
            key: req.params.key,
            ... req.body
        });
        res.send(result);
    });
    app.post(`/plugins/${pluginId}/:key`, (req, res) => {
        res.send(true);
    });
    app.get(`/plugins/${pluginId}/watch/task/:taskId`, (req, res) => {
        res.send(true);
    });

    http.createServer(app).listen(app.get('port'), app.get('host'), function(){
        console.log('Express server listening on port ' + app.get('port'));
    });
};

module.exports = {
    paths: function (paths, env) {
        executeServer();
        paths.appIndexJs = path.resolve(__dirname, 'src-front/index.tsx');
        paths.appSrc = path.resolve(__dirname, 'src-front');
        return paths;
    },
}