const opt = require('../secrets/options'),
  { deleteStack, createOrUpdateStack } = require('./cloudformation'),
  ecsTemplate = require('./templates/sqlapi.json')

async function run() {
  const options = {
    StackName: opt.name,
    Parameters: [],
    template: ecsTemplate
  }
  if (process.argv[2] === "delete") {
    await deleteStack(options)
  } else {
    await createOrUpdateStack(options)
  }
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