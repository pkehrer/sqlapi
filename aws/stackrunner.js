const { createOrUpdateStack, deleteStack } = require('./cloudformation')

async function runStack(stackInfo, { beforeDelete, afterCreate } = {}) {
  if (process.argv[2] === 'delete') {
    if (beforeDelete) {
      await beforeDelete()
    }
    await deleteStack(stackInfo)
  } else {
    const stack = await createOrUpdateStack(stackInfo)
    if (afterCreate) {
      await afterCreate(stack)
    }
  }
  console.log("Done.")
}

module.exports = { runStack }