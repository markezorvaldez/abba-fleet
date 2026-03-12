import { useEffect, useState } from 'react'

function App() {
  const [status, setStatus] = useState<string>('checking...')

  useEffect(() => {
    fetch('/api/health')
      .then(res => res.json())
      .then(data => setStatus(data.status))
      .catch(() => setStatus('unreachable'))
  }, [])

  return (
    <div>
      <h1>ABBA Fleet</h1>
      <p>API status: {status}</p>
    </div>
  )
}

export default App
