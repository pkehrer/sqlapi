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
  const command = 'dotnet publish -f netcoreapp2.2 -c Release'
  try {
    system(command, root)
  } catch (error) {
    console.log("retrying...")
    system(command, root)
  }
}

function system(command, opts = { cwd: null }) {
  console.log("running command: " + command)
  const output = cp.execSync(command, { stdio: ['ignore', 'pipe', 'ignore'], cwd: opts.cwd })
  console.log(output.toString())
}

module.exports = { system, getImageRevision, dockerBuild, dotnetPublish }