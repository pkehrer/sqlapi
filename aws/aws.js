const AWS = require('aws-sdk'),
  _ = require('lodash')

let credentials = { region: 'us-east-1' }

function setUserCredentials(stack) {
  const accessKeyId = _.find(stack.Outputs, o => o.OutputKey === 'accesskeyid').OutputValue
  const secretAccessKey = _.find(stack.Outputs, o => o.OutputKey === 'secretaccesskey').OutputValue
  const region = 'us-east-1'
  credentials = { accessKeyId, secretAccessKey, region }
}

module.exports = {
  cloudformation: () => new AWS.CloudFormation(credentials),
  ecr: () => new AWS.ECR(credentials),
  ecs: () => new AWS.ECS(credentials),
  setUserCredentials
}