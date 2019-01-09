const AWS = require('aws-sdk'),
  { deleteStack, createOrUpdateStack, getStack } = require('./cloudformation'),
  template = require('./templates/db.json'),
  opt = require('../secrets/options'),
  _ = require('lodash'),
  fs = require('fs'),
  path = require('path')

const Parameters = [
  { ParameterKey: "DBUsername", ParameterValue: opt.username },
  { ParameterKey: "DBPassword", ParameterValue: opt.password }
]

async function run() {
  const options = {
    StackName: 'sqlapidb',
    Parameters,
    template
  }

  if (process.argv[2] === 'delete') {
    await deleteStack(options)
  } else {
    try {
      await createOrUpdateStack(options)
    } finally {
      const stack = await getStack(options)
      configureConnectionString(stack)
    }

  }
}

function configureConnectionString(stack) {
  console.log(stack)
  const server = _.find(stack.Outputs, o => o.OutputKey === 'dbaddress').OutputValue
  const dotnetConfig = {
    Server: server,
    Username: opt.username,
    Password: opt.password,
    Database: 'sqlapi'
  }
  const appSettingsPath = path.join(__dirname, '../Service/appsettings.json')
  const fileContents = fs.readFileSync(appSettingsPath).toString()
  const fileObj = JSON.parse(fileContents)
  fileObj.DbConnectionConfig = dotnetConfig
  fs.writeFileSync(appSettingsPath, JSON.stringify(fileObj, null, 2))
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