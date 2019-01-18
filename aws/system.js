const cp = require('child_process'),
  path = require('path')

const root = { cwd: path.join(__dirname, "..") }

function getImageRevision() {
  const command = 'git log -1 --pretty=%h'
  return cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'] })
}

async function dockerBuild() {
  system('docker build -t sqlapi .', root)
}

async function dotnetPublish() {
  system('dotnet publish -f netcoreapp2.2 -c Release', root)
}

function system(command, opts) {
  console.log("running command: " + command)
  const output = cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'], cwd: opts.cwd })
  console.log(output.toString())
}

module.exports = { system, getImageRevision, dockerBuild, dotnetPublish }