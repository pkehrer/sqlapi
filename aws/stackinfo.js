const { getImageRevision } = require('./system'),
  yaml = require('js-yaml'),
  fs = require('fs'),
  path = require('path'),
  { project } = require('./config')

const loadTemplate = name =>
  yaml.safeLoad(fs.readFileSync(path.join(__dirname, `templates/${name}.yml`), 'utf8'))

function randString(length) {
  const next = () => Math.random().toString(36).substr(2)
  let s = next()
  while (s.length < length) {
    s += next()
  }
  return s.substr(0, length)
}

module.exports = {
  deploymentUser: {
    StackName: `${project}-deploymentuser`,
    template: loadTemplate('deploymentuser')
  },

  db: {
    StackName: `${project}-db`,
    Parameters: [
      { ParameterKey: "DBUsername", ParameterValue: "u" + randString(7) },
      { ParameterKey: "DBPassword", ParameterValue: randString(24) }
    ],
    template: loadTemplate('db')
  },

  ecr: {
    StackName: `${project}-ecr`,
    template: loadTemplate('ecr')
  },

  ecs: {
    StackName: project,
    Parameters: [
      { ParameterKey: "ImageRevision", ParameterValue: getImageRevision().toString() },
      { ParameterKey: "StackName", ParameterValue: project }
    ],
    template: loadTemplate('ecs')
  }
}