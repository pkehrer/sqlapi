const AWS = require('aws-sdk'),
  fs = require('fs'),
  path = require('path'),
  _ = require('lodash')

const credentialsPath = '../secrets/deploymentuser.json'

let credentials
if (fs.existsSync(path.join(__dirname, credentialsPath))) {
  credentials = require('../secrets/deploymentuser.json')
} else {
  credentials = { region: 'us-east-1' }
}

function updateConfig(config) {
  credentials = config
}

function setUserCredentials(stack) {
  const accessKeyId = _.find(stack.Outputs, o => o.OutputKey === 'accesskeyid').OutputValue
  const secretAccessKey = _.find(stack.Outputs, o => o.OutputKey === 'secretaccesskey').OutputValue
  const region = 'us-east-1'
  updateConfig({ accessKeyId, secretAccessKey, region })
}

module.exports = {
  cloudformation: () => new AWS.CloudFormation(credentials),
  ecr: () => new AWS.ECR(credentials),
  ecs: () => new AWS.ECS(credentials),
  updateConfig,
  setUserCredentials
}