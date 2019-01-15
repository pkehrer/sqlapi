const opt = require('../secrets/options'),
  template = require('./templates/ecs.json'),
  { runStack } = require('./stackrunner'),
  { getImageRevision } = require('./docker')

const Parameters = [
  { ParameterKey: "ImageRevision", ParameterValue: getImageRevision().toString() }
]

const stackInfo = {
  StackName: opt.name,
  Parameters,
  template
}

runStack(stackInfo)
  .catch(error => {
    console.log("*** ERROR ***")
    console.log(error.toString())
  })
