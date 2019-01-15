const opt = require('../secrets/options'),
  _ = require('lodash'),
  fs = require('fs'),
  path = require('path'),
  { createOrUpdateStack } = require('./cloudformation'),
  { updateConfig, setUserCredentials } = require('./aws'),
  stackInfo = require('./stackinfo'),
  { dockerPush } = require('./docker')



function setDbConnectionString(stack) {
  const server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const dotnetConfig = { Server: server, Username: opt.username, Password: opt.password, Database: 'sqlapi' }
  const configObj = { DbConnectionConfig: dotnetConfig }
  const dbConfigPath = path.join(__dirname, '../secrets/db.json')
  fs.writeFileSync(dbConfigPath, JSON.stringify(configObj, null, 2))
}

async function run() {
  updateConfig({ region: 'us-east-1' })

  const deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
  setUserCredentials(deploymentUserStack)

  const dbStack = await createOrUpdateStack(stackInfo.db)
  setDbConnectionString(dbStack)

  await createOrUpdateStack(stackInfo.ecr)
  await dockerPush()

  await createOrUpdateStack(stackInfo.ecs)
}

run()
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })