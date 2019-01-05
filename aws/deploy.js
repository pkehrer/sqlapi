const AWS = require('aws-sdk'),
  _ = require('lodash'),
  opt = require('./options')


const cloudformation = new AWS.CloudFormation({ region: 'us-east-1' })

const cfParams = [
  { ParameterKey: "DBName", ParameterValue: opt.name },
  { ParameterKey: "DBUsername", ParameterValue: opt.username },
  { ParameterKey: "DBPassword", ParameterValue: opt.password }
]

async function run() {
  const stack = await getStack()
  if (!stack) {
    await createStack()
  } else if (isInProgress(stack)) {
    throw new Error("Stack status is " + isInProgress(stack))
  } else {
    await updateStack()
  }
  console.log('Stack:')
  console.log(JSON.stringify(stack))
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
  const response = await cloudformation.updateStack({
    StackName: opt.name,
    Parameters: cfParams,
    TemplateBody: JSON.stringify(require('./templates/sqlapi.json'))
  }).promise()
  console.log('Update Stack Response:')
  console.log(response)
}

async function pollStack() {
  await 
}

async function createStack() {
  const response = await cloudformation.createStack({
    StackName: opt.name,
    OnFailure: 'DELETE',
    Parameters: cfParams,
    TemplateBody: JSON.stringify(require('./templates/sqlapi.json'))
  }).promise()
  console.log('Create Stack response:')
  console.log(response)
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