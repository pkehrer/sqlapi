const { ecs } = require('./aws')

async function run() {
  await ecs().updateService({
    cluster: 'sqlapi',
    service: 'sqalpi',
    forceNewDeployment: true
  }).promise()
}

run()
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })