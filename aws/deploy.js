const AWS = require('aws-sdk'),
  _ = require('lodash'),
  poll = require('async-polling'),
  opt = require('../secrets/options'),
  { getLoginCommand } = require('./docker')

const cloudformation = new AWS.CloudFormation(opt.awsConfig)
const ecr = new AWS.ECR(opt.awsConfig)

const cfParams = [
  { ParameterKey: "DBUsername", ParameterValue: opt.username },
  { ParameterKey: "DBPassword", ParameterValue: opt.password }
]

async function getDockerLogin() {
  const command = await getLoginCommand()
  cfParams.push({ ParameterKey: "DockerLogin", ParameterValue: command })
}

async function run() {
  await getDockerLogin()

  const stack = await getStack()
  if (process.argv[2] === "delete") {
    await deleteEcrRepository()
    if (!stack) {
      throw new Error("Cannot delete stack cause it don't exist")
    }
    await deleteStack()
    await pollStack()
  }
  if (!stack) {
    await createStack()
    await pollStack()
  } else if (isInProgress(stack)) {
    throw new Error("Stack status is " + isInProgress(stack))
  } else {
    await updateStack()
    await pollStack()
  }
}

async function deleteEcrRepository() {
  await ecr.deleteRepository({
    repositoryName: opt.name,
    force: true
  }).promise()
}

function isInProgress(stack) {
  return _.endsWith(stack.StackStatus, "IN_PROGRESS") ? stack.StackStatus : null
}

async function getStack() {
  try {
    const response = await cloudformation.describeStacks({
      StackName: opt.name
    }).promise()
    return _.get(response, 'Stacks[0]')
  } catch (error) {
    return null
  }
}

async function updateStack() {
  console.log('Updating stack...')
  const response = await cloudformation.updateStack({
    StackName: opt.name,
    Parameters: cfParams,
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(require('./templates/sqlapi.json'))
  }).promise()
}

async function createStack() {
  console.log('Creating stack...')
  const response = await cloudformation.createStack({
    StackName: opt.name,
    OnFailure: 'DELETE',
    Parameters: cfParams,
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(require('./templates/sqlapi.json'))
  }).promise()
}

async function deleteStack() {
  const response = await cloudformation.deleteStack({
    StackName: opt.name,
    RetainResources: []
  }).promise()
}

function pollStack() {
  return new Promise((res, rej) => {
    const polling = poll(function (end) {
      cloudformation.describeStacks({
        StackName: opt.name
      }, (err, response) => {
        if (err) {
          end(err)
        } else {
          const status = response.Stacks[0].StackStatus
          end(null, status)
        }
      })
    }, 2000)
    polling.on('error', function (error) {
      polling.stop()
      rej(error)
    })
    polling.on('result', function (status) {
      console.log('Current Status: ' + status)
      if (!_.endsWith(status, "IN_PROGRESS")) {
        polling.stop()
        res(status)
      }
    })
    polling.run()
  })
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