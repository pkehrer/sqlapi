
const _ = require('lodash'),
  { pollStack } = require('./stackpoller'),
  { cloudformation } = require('./aws')

async function deleteStack(opt) {
  const stack = await getStack(opt)
  if (!stack) {
    throw new Error("Cannot delete stack cause it don't exist")
  }

  console.log(`Deleting stack ${opt.StackName}...`)
  await cloudformation().deleteStack({
    StackName: opt.StackName,
    RetainResources: []
  }).promise()

  await pollStack(opt)
}

async function createOrUpdateStack(opt) {
  const stack = await getStack(opt)
  if (!stack) {
    await createStack(opt)
    return await pollStack(opt)
  } else if (isInProgress(stack)) {
    throw new Error("Stack status is " + isInProgress(stack))
  } else {
    try {
      await updateStack(opt)
      return await pollStack(opt)
    } catch (error) {
      if (error.message == "No updates are to be performed.") {
        console.log(error.message)
        return await getStack(opt)
      } else {
        throw error
      }
    }
  }
}

async function getStack({ StackName }) {
  try {
    const response = await cloudformation().describeStacks({ StackName }).promise()
    return _.get(response, 'Stacks[0]')
  } catch (error) {
    return null
  }
}

async function updateStack({ StackName, Parameters, template }) {
  console.log(`Updating stack ${StackName}...`)
  await cloudformation().updateStack({
    StackName,
    Tags: [{ Key: 'Project', Value: 'sqlapi' }],
    Parameters: Parameters || [],
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(template)
  }).promise()
}

async function createStack({ StackName, Parameters, template }) {
  console.log(`Creating stack ${StackName}...`)
  await cloudformation().createStack({
    StackName,
    OnFailure: 'DELETE',
    Parameters: Parameters || [],
    Tags: [{ Key: 'Project', Value: 'sqlapi' }],
    Capabilities: ['CAPABILITY_NAMED_IAM'],
    TemplateBody: JSON.stringify(template)
  }).promise()
}

function isInProgress(stack) {
  return _.endsWith(stack.StackStatus, "IN_PROGRESS") ? stack.StackStatus : null
}

module.exports = { deleteStack, createOrUpdateStack, getStack }