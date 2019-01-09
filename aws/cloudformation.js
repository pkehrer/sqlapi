
const AWS = require('aws-sdk'),
  _ = require('lodash'),
  poll = require('async-polling'),
  opt = require('../secrets/options')

const cloudformation = new AWS.CloudFormation(opt.awsConfig)

async function deleteStack(opt) {
  const stack = await getStack(opt)
  if (!stack) {
    throw new Error("Cannot delete stack cause it don't exist")
  }
  await doDeleteStack(opt)
  try {
    await pollStack(opt)
  } catch (error) {
    // This happens cause the stack disappears when it finishes deleting
  }
}

async function createOrUpdateStack(opt) {
  const stack = await getStack(opt)
  if (!stack) {
    await createStack(opt)
    return await pollStack(opt)
  } else if (isInProgress(stack)) {
    throw new Error("Stack status is " + isInProgress(stack))
  } else {
    await updateStack(opt)
    return await pollStack(opt)
  }
}

async function getStack({ StackName }) {
  try {
    const response = await cloudformation.describeStacks({ StackName }).promise()
    return _.get(response, 'Stacks[0]')
  } catch (error) {
    return null
  }
}

function pollStack({ StackName }) {
  return new Promise((res, rej) => {
    const polling = poll(function (end) {
      cloudformation.describeStacks({ StackName }, (err, response) => {
        if (err) {
          end(err)
        } else {
          const stack = response.Stacks[0]
          end(null, stack)
        }
      })
    }, 2000)
    polling.on('error', function (error) {
      polling.stop()
      rej(error)
    })
    polling.on('result', function (stack) {
      console.log('Current Status: ' + stack.StackStatus)
      if (!_.endsWith(stack.StackStatus, "IN_PROGRESS")) {
        polling.stop()
        res(stack)
      }
    })
    polling.run()
  })
}


function isInProgress(stack) {
  return _.endsWith(stack.StackStatus, "IN_PROGRESS") ? stack.StackStatus : null
}
async function updateStack({ StackName, Parameters, template }) {
  console.log(`Updating stack ${StackName}...`)
  const response = await cloudformation.updateStack({
    StackName,
    Tags: [{ Key: 'Project', Value: 'sqlapi' }],
    Parameters,
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(template)
  }).promise()
}

async function createStack({ StackName, Parameters, template }) {
  console.log(`Creating stack ${StackName}...`)
  const response = await cloudformation.createStack({
    StackName,
    OnFailure: 'DELETE',
    Parameters,
    Tags: [{ Key: 'Project', Value: 'sqlapi' }],
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(template)
  }).promise()
}

async function doDeleteStack({ StackName }) {
  const response = await cloudformation.deleteStack({
    StackName,
    RetainResources: []
  }).promise()
}

module.exports = { deleteStack, createOrUpdateStack, getStack }