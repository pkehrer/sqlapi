const template = require('./templates/db.json'),
  opt = require('../secrets/options'),
  _ = require('lodash'),
  fs = require('fs'),
  path = require('path'),
  { runStack } = require('./stackrunner')

const Parameters = [
  { ParameterKey: "DBUsername", ParameterValue: opt.username },
  { ParameterKey: "DBPassword", ParameterValue: opt.password }
]

const stackInfo = { StackName: 'sqlapidb', Parameters, template }

async function setConnectionString(stack) {
  const server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const dotnetConfig = {
    Server: server,
    Username: opt.username,
    Password: opt.password,
    Database: 'sqlapi'
  }
  const dbConfigPath = path.join(__dirname, '../secrets/db.json')
  const configObj = { DbConnectionConfig: dotnetConfig }
  fs.writeFileSync(dbConfigPath, JSON.stringify(configObj, null, 2))
}

runStack(stackInfo, { afterCreate: setConnectionString })

