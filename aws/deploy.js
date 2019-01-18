const fs = require('fs'),
  path = require('path'),
  _ = require('lodash'),
  { setUserCredentials } = require('./aws'),
  { createOrUpdateStack, getStack } = require('./cloudformation'),
  { dockerBuild, dotnetPublish } = require('./system'),
  { dockerPush } = require('./docker'),
  { project } = require('./config'),
  stackInfo = require('./stackinfo')

function setDbConnectionString(stack) {
  const server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const dotnetConfig = {
    Server: server,
    Username: opt.username,
    Password: opt.password,
    Database: project
  }
  const configObj = { DbConnectionConfig: dotnetConfig }
  const dbConfigPath = path.join(__dirname, '../secrets/db.json')
  fs.writeFileSync(dbConfigPath, JSON.stringify(configObj, null, 2))
}

function sleep(sec) {
  console.log(`sleeping for ${sec}sec...`)
  return new Promise(res => setTimeout(res, sec * 1000))
}

(async function () {

  let deploymentUserStack = await getStack(stackInfo.deploymentUser)

  if (!deploymentUserStack) {
    deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
    sleep(15)
  }

  setUserCredentials(deploymentUserStack)

  const dbStack = await createOrUpdateStack(stackInfo.db)
  setDbConnectionString(dbStack)
  await createOrUpdateStack(stackInfo.ecr)

  await dotnetPublish()
  await dockerBuild()
  await dockerPush()

  await createOrUpdateStack(stackInfo.ecs)

})().catch(error => {
  console.log('*** ERROR ***')
  console.log(error.toString())
})