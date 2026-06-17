import { useState, useRef } from 'react'

interface Message {
  type: 'message' | 'history' | 'user_joined' | 'user_left'
  username?: string
  text?: string
  timestamp?: string
  messages?: Message[]
}

function App() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [token, setToken] = useState('')
  const [joined, setJoined] = useState(false)
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const ws = useRef<WebSocket | null>(null)

  const auth = async () => {
    const url = `http://localhost:5139/api/auth/${mode}`
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })
    if (res.ok) {
      const data = await res.json()
      setToken(data.token)
    } else {
      alert('Error')
    }
  }

  const join = () => {
    if (!username.trim()) return
    setMessages([])
    ws.current = new WebSocket(`ws://localhost:5139/ws?username=${username}`)

    ws.current.onopen = () => console.log('Connected!')
    ws.current.onmessage = (e) => {
      const data = JSON.parse(e.data)
      if (data.type === 'history') {
        setMessages(data.messages)
      } else {
        setMessages(prev => [...prev, data])
      }
    }

    setJoined(true)
  }

  const send = () => {
    if (!input.trim() || !ws.current) return
    if (ws.current.readyState !== WebSocket.OPEN) {
      setTimeout(send, 100)
      return
    }
    ws.current.send(JSON.stringify({ text: input }))
    setInput('')
  }

  if (!token) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="bg-gray-800 p-8 rounded-lg shadow-lg w-96">
          <h1 className="text-white text-2xl mb-4">💬 Chat Login</h1>
          <input className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 mb-3"
            placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} />
          <input className="w-full p-2 rounded bg-gray-700 text-white border border-gray-600 mb-4"
            type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} />
          <button onClick={auth} className="w-full bg-blue-600 text-white p-2 rounded hover:bg-blue-700 mb-2">
            {mode === 'login' ? 'Login' : 'Register'}
          </button>
          <button onClick={() => setMode(mode === 'login' ? 'register' : 'login')}
            className="w-full text-gray-400 text-sm hover:text-white">
            {mode === 'login' ? 'Create account' : 'Back to login'}
          </button>
        </div>
      </div>
    )
  }

  if (!joined) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="bg-gray-800 p-8 rounded-lg shadow-lg">
          <h1 className="text-white text-2xl mb-4">💬 Join Chat</h1>
          <p className="text-gray-400 mb-4">Logged in as: {username}</p>
          <button onClick={join} className="w-full bg-green-600 text-white p-2 rounded hover:bg-green-700">
            Join Chat
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-900 flex flex-col">
      <div className="bg-gray-800 p-4 text-white flex justify-between items-center">
        <h1 className="text-xl font-bold">💬 Chat Room</h1>
        <span className="text-gray-400">👤 {username}</span>
      </div>
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.map((msg, i) => (
          <div key={i}>
            {msg.type === 'user_joined' && (
              <p className="text-green-400 text-center text-sm">✅ {msg.username} joined</p>
            )}
            {msg.type === 'user_left' && (
              <p className="text-red-400 text-center text-sm">👋 {msg.username} left</p>
            )}
            {msg.type === 'message' && (
              <div className="flex">
                <div className="max-w-md p-2 rounded-lg bg-gray-700 text-white">
                  <p>
                    <span className="font-bold text-blue-400">{msg.username}</span>
                    <span className="text-gray-400 mx-1">·</span>
                    <span>{msg.text}</span>
                  </p>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
      <div className="bg-gray-800 p-4 flex gap-2">
        <input className="flex-1 p-2 rounded bg-gray-700 text-white border border-gray-600"
          placeholder="Type a message..." value={input}
          onChange={e => setInput(e.target.value)} onKeyDown={e => e.key === 'Enter' && send()} />
        <button onClick={send} className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700">Send</button>
      </div>
    </div>
  )
}

export default App