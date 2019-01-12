const template = require('./templates/deploymentuser.json'),
  { runStack } = require('./stackrunner'),
  { updateConfig } = require('./aws')
fs = require('fs'),
  path = require('path'),
  _ = require('lodash')

const stackInfo = {
  StackName: 'sqlapi-deploymentuser',
  Parameters: [],
  template
}

async function writeUserCredentials(stack) {
  const accessKeyId = _.find(stack.Outputs, o => o.OutputKey === 'accesskeyid').OutputValue
  const secretAccessKey = _.find(stack.Outputs, o => o.OutputKey === 'secretaccesskey').OutputValue
  const region = 'us-east-1'
  const creds = { accessKeyId, secretAccessKey, region }
  const secretPath = path.join(__dirname, '../secrets/deploymentuser.json')
  fs.writeFileSync(secretPath, JSON.stringify(creds, null, 2))
}
updateConfig({ region: 'us-east-1' })
runStack(stackInfo, { afterCreate: writeUserCredentials })