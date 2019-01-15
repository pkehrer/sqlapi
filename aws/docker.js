const AWS = require('aws-sdk'),
  _ = require('lodash'),
  opt = require('../secrets/options'),
  cp = require('child_process'),
  { ecr } = require('./aws')


async function dockerPush() {
  const loginCommand = await getLoginCommand()
  system(loginCommand)
  const revision = getImageRevision()
  system(`docker tag ${opt.name}:latest 916437080264.dkr.ecr.us-east-1.amazonaws.com/${opt.name}:${revision}`)
  system(`docker push 916437080264.dkr.ecr.us-east-1.amazonaws.com/${opt.name}:${revision}`)
}

async function getLoginCommand(extra = "") {
  const response = await ecr().getAuthorizationToken({}).promise()
  const base64Token = response.authorizationData[0].authorizationToken
  const decoded = Buffer.from(base64Token, 'base64').toString()
  const username = _.split(decoded, ':')[0]
  const password = _.split(decoded, ':')[1]
  return `docker login ${extra} -u ${username} -p ${password} ${response.authorizationData[0].proxyEndpoint}`
}

function getImageRevision() {
  const command = 'git log -1 --pretty=%h'
  return cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'] })
}

function system(command) {
  console.log("running command: " + command)
  const output = cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'] })
  console.log(output.toString())
}

module.exports = { dockerPush, getImageRevision }