const opt = require('../secrets/options'),
  template = require('./templates/ecs.json'),
  { runStack } = require('./stackrunner')

const Parameters = [
  { ParameterKey: "Timestamp", ParameterValue: new Date().getTime().toString() }
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
