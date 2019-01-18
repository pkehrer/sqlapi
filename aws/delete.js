const { createOrUpdateStack, deleteStack, getStack } = require('./cloudformation'),
  { setUserCredentials, ecr } = require('./aws'),
  stackInfo = require('./stackinfo')



async function deleteEcrRepository() {
  await ecr().deleteRepository({
    repositoryName: 'sqlapi',
    force: true
  }).promise()
    .catch(() => console.log('error deleting ecr repository. (doesn\'t exist?)'))
}

async function deleteIfExists(stackInfo) {
  if (await getStack(stackInfo)) {
    await deleteStack(stackInfo)
  } else
    console.log(`Stack with name ${stackInfo.StackName} does not exist...`)
}

async function run() {

  const deploymentUserStack = await createOrUpdateStack(stackInfo.deploymentUser)
  setUserCredentials(deploymentUserStack)
  await deleteIfExists(stackInfo.db)
  await deleteEcrRepository() // must be manually deleted before stack :(
  await deleteIfExists(stackInfo.ecr)
  await deleteIfExists(stackInfo.ecs)
  await deleteStack(stackInfo.deploymentUser)
}

run()
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })