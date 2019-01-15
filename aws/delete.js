const { createOrUpdateStack, deleteStack } = require('./cloudformation'),
  { setUserCredentials, ecr } = require('./aws'),
  stackInfo = require('./stackinfo')

async function deleteEcrRepository() {
  await ecr().deleteRepository({
    repositoryName: 'sqlapi',
    force: true
  }).promise()
}


async function run() {

  const deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
  setUserCredentials(deploymentUserStack)

  await deleteStack(stackInfo.db)
  await deleteEcrRepository() // must be manually deleted before stack :(
  await deleteStack(stackInfo.ecr)
  await deleteStack(stackInfo.ecs)
  await deleteStack(stackInfo.deploymentUser)
}

run()
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })