const fs = require('fs'),
  path = require('path'),
  _ = require('lodash'),
  { setUserCredentials } = require('./aws'),
  { createOrUpdateStack } = require('./cloudformation'),
  { dockerPush } = require('./docker'),
  opt = require('../secrets/options'),
  stackInfo = require('./stackinfo')

function setDbConnectionString(stack) {
  const server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const dotnetConfig = { Server: server, Username: opt.username, Password: opt.password, Database: 'sqlapi' }
  const configObj = { DbConnectionConfig: dotnetConfig }
  const dbConfigPath = path.join(__dirname, '../secrets/db.json')
  fs.writeFileSync(dbConfigPath, JSON.stringify(configObj, null, 2))
}

(async function () {
  const deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
  setUserCredentials(deploymentUserStack)

  const dbStack = await createOrUpdateStack(stackInfo.db)
  setDbConnectionString(dbStack)

  await createOrUpdateStack(stackInfo.ecr)
  await dockerPush()

  await createOrUpdateStack(stackInfo.ecs)
})().catch(error => {
  console.log('*** ERROR ***')
  console.log(error.toString())
})