const { getImageRevision } = require('./docker'),
  opt = require('../secrets/options')

module.exports = {
  deploymentUser: {
    StackName: 'sqlapi-deploymentuser',
    template: require('./templates/deploymentuser.json')
  },

  db: {
    StackName: 'sqlapidb',
    Parameters: [
      { ParameterKey: "DBUsername", ParameterValue: opt.username },
      { ParameterKey: "DBPassword", ParameterValue: opt.password }
    ],
    template: require('./templates/db.json')
  },

  ecr: {
    StackName: 'sqlapiecr',
    template: require('./templates/ecr.json')
  },

  ecs: {
    StackName: 'sqlapi',
    Parameters: [
      { ParameterKey: "ImageRevision", ParameterValue: getImageRevision().toString() }
    ],
    template: require('./templates/ecs.json')
  }
}