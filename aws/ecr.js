const AWS = require('aws-sdk'),
  opt = require('../secrets/options'),
  { dockerPush } = require('./docker'),
  template = require('./templates/ecr.json'),
  { runStack } = require('./stackrunner')

const ecr = new AWS.ECR(opt.awsConfig)

const stackInfo = {
  StackName: 'sqlapiecr',
  Parameters: [],
  template
}

async function deleteEcrRepository() {
  await ecr.deleteRepository({
    repositoryName: 'sqlapi',
    force: true
  }).promise()
}

runStack(
  stackInfo,
  { beforeDelete: deleteEcrRepository, afterCreate: dockerPush })
  .catch(error => {
    console.log("*** ERROR ***")
    console.log(error.toString())
  })
