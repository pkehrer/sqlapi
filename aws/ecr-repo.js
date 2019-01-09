const AWS = require('aws-sdk'),
  opt = require('../secrets/options'),
  dockerPush = require('./docker-push'),
  { deleteStack, createOrUpdateStack } = require('./cloudformation'),
  template = require('./templates/ecrRepo.json')

const ecr = new AWS.ECR(opt.awsConfig)

async function run() {
  const options = {
    StackName: 'sqlapiecr',
    Parameters: [],
    template
  }

  if (process.argv[2] === 'delete') {
    await deleteEcrRepository()
    await deleteStack(options)
  } else {
    try {
      await createOrUpdateStack(options)
    } finally {
      await dockerPush.run()
    }
  }
}

async function deleteEcrRepository() {
  await ecr.deleteRepository({
    repositoryName: 'sqlapi',
    force: true
  }).promise()
}

run()
  .then(() => {
    console.log("Done")
    //process.exit()
  })
  .catch(error => {
    console.log("*** ERROR ***")
    console.log(error.toString())
  })