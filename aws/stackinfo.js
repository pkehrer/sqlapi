const { getImageRevision } = require('./docker'),
  opt = require('../secrets/options'),
  yaml = require('js-yaml'),
  fs = require('fs'),
  path = require('path')

const loadTemplate = name =>
  yaml.safeLoad(fs.readFileSync(path.join(__dirname, `templates/${name}.yml`), 'utf8'))

module.exports = {
  deploymentUser: {
    StackName: 'sqlapi-deploymentuser',
    template: loadTemplate('deploymentuser')
  },

  db: {
    StackName: 'sqlapidb',
    Parameters: [
      { ParameterKey: "DBUsername", ParameterValue: opt.username },
      { ParameterKey: "DBPassword", ParameterValue: opt.password }
    ],
    template: loadTemplate('db')
  },

  ecr: {
    StackName: 'sqlapiecr',
    template: loadTemplate('ecr')
  },

  ecs: {
    StackName: 'sqlapi',
    Parameters: [
      { ParameterKey: "ImageRevision", ParameterValue: getImageRevision().toString() }
    ],
    template: loadTemplate('ecs')
  }
}