const _ = require('lodash'),
  cp = require('child_process'),
  opt = require('../secrets/options'),
  { getLoginCommand } = require('./docker')

async function run() {
  const loginCommand = await getLoginCommand()
  system(loginCommand)
  await dockerPush()
}

async function dockerPush() {
  system(`docker tag ${opt.name}:latest 916437080264.dkr.ecr.us-east-1.amazonaws.com/${opt.name}:latest`)
  system(`docker push 916437080264.dkr.ecr.us-east-1.amazonaws.com/${opt.name}:latest`)
}

function system(command) {
  console.log("running command: " + command)
  const output = cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'] })
  console.log(output.toString())
}

run()
  .then(() => console.log('Done'))
  .catch(error => {
    console.log('*** ERROR ***')
    console.log(error.toString())
  })