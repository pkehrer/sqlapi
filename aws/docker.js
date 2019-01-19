const _ = require('lodash'),
  { ecr } = require('./aws'),
  { system, getImageRevision } = require('./system'),
  { project } = require('./config')

async function getLoginCommand(extra = "") {
  const response = await ecr().getAuthorizationToken({}).promise()
  const base64Token = response.authorizationData[0].authorizationToken
  const decoded = Buffer.from(base64Token, 'base64').toString()
  const username = _.split(decoded, ':')[0]
  const password = _.split(decoded, ':')[1]
  return `docker login ${extra} -u ${username} -p ${password} ${response.authorizationData[0].proxyEndpoint}`
}

async function dockerPush() {
  const loginCommand = await getLoginCommand()
  system(loginCommand)
  const revision = getImageRevision()
  system(`docker tag ${project}:latest 916437080264.dkr.ecr.us-east-1.amazonaws.com/${project}:${revision}`)
  system(`docker push 916437080264.dkr.ecr.us-east-1.amazonaws.com/${project}:${revision}`)
}

module.exports = { dockerPush }