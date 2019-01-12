

const d1 = new Date()

setTimeout(() => {
  d2 = new Date()
  console.log(d2 - d1)
}, 2000)