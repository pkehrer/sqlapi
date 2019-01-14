const AWS = require('aws-sdk'),
  fs = require('fs'),
  path = require('path')

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


module.exports = {
  cloudformation: () => new AWS.CloudFormation(credentials),
  ecr: () => new AWS.ECR(credentials),
  ecs: () => new AWS.ECS(credentials),
  updateConfig
}