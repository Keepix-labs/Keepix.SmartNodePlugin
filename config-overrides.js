const path = require('path');
const http = require('http');
const express = require('express');
const fs = require('fs');
const { exec } = require('child_process');
const cors = require('cors');

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

    app.use(cors());
    app.use(express.json());
    app.set('port', 2000);
    app.set('host', '0.0.0.0');
    var runningTasks = {};

    const executePlugin = async (dto, isAsync = false) => {
        if (isAsync) {
            const taskId = `${pluginId}-${dto.key}`;

            // skip duplicate running
            if (runningTasks[taskId] != undefined && runningTasks[taskId].status != 'FINISHED') {
                return {
                    taskId: taskId,
                    aborted: true,
                    reason: "Already running"
                };
            }

            // save the task status
            runningTasks[taskId] = {
                status: 'RUNNING'
            };

            // run asynchronously
            execPluginFunction(dto).then((result) => {
                runningTasks[taskId] = {
                    status: 'FINISHED',
                    result: result
                };
            });

            return { taskId: taskId };
        } else {
            return await execPluginFunction(dto);
        }
    }

    app.get(`/plugins/${pluginId}/:key`, async (req, res) => {
        const isAsync = req.query.isAsync === 'true';
        res.send(await executePlugin({
            key: req.params.key
        }, isAsync));
    });
    app.post(`/plugins/${pluginId}/:key`, async (req, res) => {
        const isAsync = req.query.isAsync === 'true';

        console.log(req.body);

        res.send(await executePlugin({
            key: req.params.key,
            ... req.body
        }, isAsync));
    });
    app.get(`/plugins/${pluginId}/watch/task/:taskId`, (req, res) => {
        res.send(runningTasks[req.params.taskId]);
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