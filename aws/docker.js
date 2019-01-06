const AWS = require('aws-sdk'),
  _ = require('lodash'),
  opt = require('../secrets/options')

const ecr = new AWS.ECR(opt.awsConfig)

async function getLoginCommand(extra = "") {
  const response = await ecr.getAuthorizationToken({}).promise()
  const base64Token = response.authorizationData[0].authorizationToken
  const decoded = Buffer.from(base64Token, 'base64').toString()
  const username = _.split(decoded, ':')[0]
  const password = _.split(decoded, ':')[1]
  return `docker login ${extra} -u ${username} -p ${password} ${response.authorizationData[0].proxyEndpoint}`
}

module.exports = { getLoginCommand }