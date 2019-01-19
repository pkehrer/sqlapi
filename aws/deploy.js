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
  const Server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const Username = _.find(stack.Parameters, p => p.ParameterKey === 'DBUsername').ParameterValue
  const Password = _.find(stack.Parameters, p => p.ParameterKey === 'DBPassword').ParameterValue

  const configObj = {
    DbConnectionConfig: {
      Server, Username, Password, Database: project
    }
  }

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
    await sleep(15)
  }

  setUserCredentials(deploymentUserStack)

  let dbStack = await getStack(stackInfo.db)
  if (!dbStack) {
    dbStack = await createOrUpdateStack(stackInfo.db)
  }
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