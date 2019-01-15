const { createOrUpdateStack, deleteStack } = require('./cloudformation'),
  { updateConfig } = require('./aws'),
  stackInfo = require('./stackinfo')

async function deleteEcrRepository() {
  await ecr().deleteRepository({
    repositoryName: 'sqlapi',
    force: true
  }).promise()
}


async function run() {
  updateConfig({ region: 'us-east-1' })

  const deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
  setUserCredentials(deploymentUserStack)

  await deleteStack(stackInfo.db)
  await deleteStack(stackInfo.ecr)
  await deleteEcrRepository()
  await deleteStack(stackInfo.ecs)
  await deleteStack(stackInfo.deploymentUser)
}

run()
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })