
const _ = require('lodash'),
  poll = require('async-polling'),
  { cloudformation } = require('./aws')

function inline(txt) {
  if (process.stdout.clearLine) {
    process.stdout.clearLine()
    process.stdout.cursorTo(0)
    process.stdout.write(txt)
  } else {
    console.log('> ' + txt)
  }
}

function pollStack({ StackName }) {
  console.log(`Polling stack status for ${StackName}...`)
  return new Promise((res, rej) => {

    let stack, lastStatus, lastStatusTime

    const polling = poll(function (end) {
      cloudformation().describeStacks({ StackName }, (err, response) => {
        if (err) {
          if (err.message = `Stack with id ${StackName} does not exist` && stack) {
            stack.StackStatus = "DELETE_COMPLETE"
            end(null, stack)
          } else {
            end(err)
          }
        } else {
          stack = response.Stacks[0]
          end(null, stack)
        }
      })
    }, 2000)

    polling.on('error', function (error) {
      polling.stop()
      rej(error)
    })

    polling.on('result', function (stack) {
      if (stack.StackStatus != lastStatus) {
        if (lastStatus) { console.log("") }
        lastStatusTime = new Date()
      }

      lastStatus = stack.StackStatus
      const timeSinceChange = (new Date() - lastStatusTime) / 1000
      const timeString = timeSinceChange > 0 ? `(${formatTime(timeSinceChange)})` : ''

      inline(`Current Status: ${stack.StackStatus} ${timeString}`)

      if (!_.endsWith(stack.StackStatus, "IN_PROGRESS")) {
        polling.stop()
        console.log("")
        setTimeout(() => res(stack), 500)
      }
    })

    polling.run()
  })
}

function formatTime(sec) {
  const hours = Math.floor(sec / 3600)
  const minutes = Math.floor((sec - 3600 * hours) / 60)
  const seconds = Math.floor(sec - 3600 * hours - 60 * minutes)

  const hstr = hours > 1 ? `${hours}hrs ` : hours == 1 ? '1hr ' : ''
  const mstr = minutes > 0 ? `${minutes}m ` : ''
  const sstr = `${seconds}s`

  return hstr + mstr + sstr
}

module.exports = { pollStack }