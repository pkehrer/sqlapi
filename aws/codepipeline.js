const template = require('./templates/codepipeline.json'),
  { runStack } = require('./stackrunner')


const stackInfo = {
  StackName: 'sqlapicodepipeline',
  Parameters: [],
  template
}


runStack(stackInfo)
  .catch(error => {
    console.log("*** ERROR ***")
    console.log(error.toString())
  })
