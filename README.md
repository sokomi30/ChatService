# ChatService — Real-time Chat Application

A production-ready real-time chat application built with **ASP.NET Core 10**, **MongoDB**, **React 19**, and **WebSocket**. Demonstrates modern full-stack architecture, security best practices, and clean code principles.

> **For Portfolio**: This project showcases full-stack development skills with proper layering, testing, security, and DevOps practices.

---

## 🎯 Features

- ✅ **Real-time Chat** — WebSocket-based messaging with instant delivery
- ✅ **User Authentication** — JWT-based auth with bcrypt password hashing
- ✅ **Message Persistence** — MongoDB storage with chat history (last 50 messages)
- ✅ **User Presence** — Join/leave notifications for all connected users
- ✅ **Input Validation** — DataAnnotations for username/password with custom rules
- ✅ **Error Handling** — Global middleware with structured JSON error responses
- ✅ **Security** — JWT token validation for WebSocket, CORS policy, rate-limiting ready
- ✅ **Docker Support** — Compose setup for MongoDB, API, and frontend
- ✅ **Unit Tests** — 16+ tests covering auth, chat services, and controllers
- ✅ **API Documentation** — Full REST endpoint docs with examples
- ✅ **Clean Architecture** — Dependency injection, service layer, controller separation

Полнофункциональное приложение чата с регистрацией, аутентификацией через JWT и real-time обменом сообщений через WebSocket.

---

## 🚀 Quick Start

### Option 1: Local Development (3 steps)

**Prerequisites**: .NET 10 SDK, Node.js 20+, Docker

```bash
# 1. Start MongoDB
docker run -d -p 27017:27017 --name chat_mongo mongo:7

# 2. Run Backend (terminal 1)
cd ChatService.Api
dotnet restore
dotnet run
# ➜ http://localhost:5139/swagger

# 3. Run Frontend (terminal 2)
cd ChatService.Web
npm install
npm run dev
# ➜ http://localhost:5173
```

**Test It:**
1. Open http://localhost:5173 in 2 browser tabs
2. Register different users in each
3. Send messages → see real-time updates

---

### Option 2: Docker Compose (1 command)

```bash
# Set JWT secret
export JWT_KEY="your-secret-key-min-32-chars-long!!!!!!!!!!!"

# Start all services
docker-compose up --build

# ➜ Frontend: http://localhost:3000
# ➜ API: http://localhost:5139 (proxied through nginx)
```

---

## 📐 Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Browser (Client)                       │
│                    React + TypeScript                        │
│              - Auth (Register/Login)                         │
│              - Real-time Chat via WebSocket                  │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/WebSocket
         ┌───────────────┴───────────────┐
         │                               │
    ┌────v─────┐              ┌──────────v────┐
    │  Nginx   │              │  .NET 10 API  │
    │  (Proxy) │              │  ASP.NET Core │
    │          │              │               │
    │ /api → API              │ Controllers   │
    │ /ws → API               │ - Auth        │
    │ / → SPA                 │ - WebSocket   │
    └──────────┘              └───────┬───────┘
                                      │
                              ┌───────v────────┐
                              │    MongoDB     │
                              │   (Database)   │
                              └────────────────┘
```

**API Layer:**
- `AuthController` → Login/Register endpoints
- `ChatWebSocketHandler` → Real-time messaging logic

**Service Layer:**
- `IAuthService` — Register, login, user validation
- `IChatMessageService` — Save/retrieve messages
- `ITokenService` — JWT token generation

**Middleware:**
- `ErrorHandlingMiddleware` — Centralized exception handling

---

## 📚 API Documentation

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "alice",
  "password": "securepass123"
}
```

**Success (201):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error (409 - Username taken):**
```json
{
  "statusCode": 409,
  "message": "Username is already taken"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "alice",
  "password": "securepass123"
}
```

**Success (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error (401 - Invalid credentials):**
```json
{
  "statusCode": 401,
  "message": "Invalid username or password"
}
```

### WebSocket

#### Connect
```
WS ws://localhost:5139/ws?access_token=<JWT_TOKEN>
```

**Message Format (Client → Server):**
```json
{
  "text": "Hello, world!"
}
```

**Message Format (Server → Client):**

**Chat history on connect:**
```json
{
  "type": "history",
  "messages": [
    {
      "type": "message",
      "username": "alice",
      "text": "Hi everyone",
      "timestamp": "2026-06-23T14:30:00Z"
    }
  ]
}
```

**New message:**
```json
{
  "type": "message",
  "username": "bob",
  "text": "Hey there",
  "timestamp": "2026-06-23T14:31:15Z"
}
```

**User joined:**
```json
{
  "type": "user_joined",
  "username": "charlie"
}
```

**User left:**
```json
{
  "type": "user_left",
  "username": "charlie"
}
```

---

## ⚙️ Configuration

### Backend Environment Variables

| Variable | Default | Required | Description |
|----------|---------|----------|-------------|
| `MongoDB__ConnectionString` | `mongodb://localhost:27017` | ❌ | MongoDB URI |
| `Jwt__Key` | — | ✅ | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | `ChatService` | ❌ | JWT issuer claim |
| `Jwt__Audience` | `ChatClient` | ❌ | JWT audience claim |
| `Cors__AllowedOrigins__0` | `http://localhost:5173` | ❌ | Allowed CORS origin |
| `ASPNETCORE_ENVIRONMENT` | `Development` | ❌ | `Development` or `Production` |

### Frontend Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | `` (relative) | API base URL for non-proxied requests |

---

## 🧪 Testing

### Run Unit Tests
```bash
cd ChatService.Tests
dotnet test
```

**Coverage:**
- `AuthServiceTests` — 5 tests (register, login, duplicate username, invalid password)
- `ChatMessageServiceTests` — 3 tests (message retrieval, validation)
- `TokenServiceTests` — 3 tests (token generation, expiry, claims)
- `AuthControllerTests` — 5 tests (HTTP status codes, error responses)

### Run Linter (Frontend)
```bash
cd ChatService.Web
npm run lint
```

---

## 📦 Project Structure

```
ChatService/
├── ChatService.Api/                          # .NET 10 backend
│   ├── Controllers/
│   │   └── AuthController.cs                 # Login/Register endpoints
│   ├── Models/
│   │   ├── User.cs                          # User entity
│   │   ├── ChatMessage.cs                   # Message entity
│   │   └── Validation.cs                    # DTOs with DataAnnotations
│   ├── Services/
│   │   ├── IServices.cs                     # Service interfaces
│   │   ├── AuthService.cs                   # Authentication logic
│   │   ├── ChatMessageService.cs            # Message persistence
│   │   └── TokenService.cs                  # JWT generation
│   ├── WebSocket/
│   │   └── ChatWebSocketHandler.cs          # Real-time messaging handler
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs       # Global error handler
│   ├── Program.cs                            # Startup & DI configuration
│   ├── appsettings.json                      # Production config
│   ├── appsettings.Development.json         # Dev config
│   ├── ChatService.Api.csproj
│   └── Dockerfile                            # Multi-stage build
│
├── ChatService.Web/                          # React + TypeScript frontend
│   ├── src/
│   │   ├── App.tsx                          # Main chat component
│   │   ├── main.tsx                         # React entry point
│   │   ├── App.css                          # Tailwind CSS
│   │   └── index.css
│   ├── vite.config.ts                       # Vite + dev proxy config
│   ├── tsconfig.json
│   ├── package.json
│   ├── nginx.conf                           # Production reverse proxy
│   ├── Dockerfile                           # Multi-stage build
│   └── README.md
│
├── ChatService.Tests/                        # xUnit test suite
│   ├── Services/
│   │   ├── AuthServiceTests.cs
│   │   ├── ChatMessageServiceTests.cs
│   │   └── TokenServiceTests.cs
│   ├── Controllers/
│   │   └── AuthControllerTests.cs
│   └── ChatService.Tests.csproj
│
├── docker-compose.yml                        # Multi-container orchestration
├── .env.example                              # Environment template
├── .gitignore
├── CONTRIBUTING.md                           # Development guidelines
├── README.md                                 # This file
└── ChatService.slnx                          # Solution file

```

---

## 🔒 Security Considerations

### ✅ Implemented

- **Password Hashing** — BCrypt with automatic salt
- **JWT Authentication** — 2-hour token expiry
- **Input Validation** — DataAnnotations with regex (alphanumeric + `-_` for username)
- **CORS Policy** — Restricted to configured origins only
- **Error Handling** — No stack traces in production responses
- **WebSocket Security** — JWT token required, no username query spoofing
- **Environment Configuration** — Secrets via environment variables, not hardcoded

### ⚠️ Production Checklist

- [ ] Enable HTTPS/WSS with TLS certificates
- [ ] Use strong JWT secret (min 32 chars, rotate regularly)
- [ ] Configure MongoDB authentication (username/password)
- [ ] Add rate limiting (e.g., Redis-backed)
- [ ] Set up message moderation/content filtering
- [ ] Add request logging & monitoring
- [ ] Enable database backups
- [ ] Implement message pagination (currently loads last 50)
- [ ] Add WebSocket heartbeat/keepalive

---

## 🛠️ Development

### Adding a Feature

1. **Create a branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make changes** following [CONTRIBUTING.md](CONTRIBUTING.md)

3. **Run tests:**
   ```bash
   dotnet test ChatService.Tests
   cd ChatService.Web && npm run lint
   ```

4. **Commit & push:**
   ```bash
   git commit -m "feat: your feature description"
   git push origin feature/your-feature-name
   ```

### Debugging

**Backend:**
```bash
cd ChatService.Api
dotnet run --configuration Debug
# Attach VS Code debugger (F5)
```

**Frontend:**
```bash
cd ChatService.Web
npm run dev
# Open DevTools (F12) → Console/Network
```

**WebSocket:**
```javascript
// Browser console
const ws = new WebSocket('ws://localhost:5173/ws?access_token=YOUR_TOKEN');
ws.onmessage = (e) => console.log('Message:', JSON.parse(e.data));
ws.onerror = (e) => console.error('Error:', e);
```

---

## 📋 Roadmap

### Phase 1: MVP ✅
- [x] User auth (register/login)
- [x] Real-time chat
- [x] Message history
- [x] User presence (join/leave)

### Phase 2: Polish 🔄
- [ ] Swagger/OpenAPI documentation
- [ ] Message sanitization (prevent XSS)
- [ ] Rate limiting on auth endpoints
- [ ] Frontend UX (loading states, error handling)
- [ ] WebSocket integration tests

### Phase 3: Scale 📈
- [ ] Message search & filtering
- [ ] User profiles & avatars
- [ ] Typing indicators
- [ ] Redis pub/sub for horizontal scaling
- [ ] End-to-end encryption
- [ ] Mobile app (React Native)

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| **"Jwt:Key is required"** | Set `JWT_KEY` env var (min 32 chars) |
| **Frontend can't connect to API** | Ensure API runs on `http://localhost:5139` (dev) or check nginx proxy (Docker) |
| **WebSocket 401 error** | Token expired (2h limit). Login again to get new token. |
| **MongoDB connection fails** | Ensure MongoDB runs: `docker ps \| grep mongo`. Check connection string. |
| **Docker build fails** | Run `docker-compose down -v` then `docker-compose up --build` to clean up. |

---

## 📊 Performance Notes

- **Message History**: Loads last 50 messages on connection (efficient for most use cases)
- **WebSocket Buffer**: 4KB per message (suitable for text chat)
- **JWT Expiry**: 2 hours (balance between security and UX)
- **Database**: MongoDB default indexes sufficient for <10k messages

---

## 📝 License

MIT

---

## 👤 Author

Portfolio project demonstrating full-stack development with modern architecture, testing, and DevOps practices.

**Tech Stack:**
- **Backend**: .NET 10, ASP.NET Core, MongoDB, xUnit, Moq
- **Frontend**: React 19, TypeScript, Tailwind CSS, Vite
- **DevOps**: Docker, Docker Compose, Nginx
- **Testing**: xUnit, Moq (unit tests), 16+ tests
- **Security**: JWT, BCrypt, CORS, Input Validation

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

- **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download)
- **Node.js 20+** — [Download](https://nodejs.org/)
- **Docker & Docker Compose** — [Download](https://www.docker.com/products/docker-desktop)

### 2. Local Development

#### Start MongoDB (Docker)
```bash
docker run -d -p 27017:27017 --name chat_mongo mongo:7
```

#### Build & Run Backend
```bash
cd ChatService.Api
dotnet restore
dotnet run
# API runs on http://localhost:5139
# Swagger OpenAPI: http://localhost:5139/swagger
```

#### Build & Run Frontend (Dev Mode)
```bash
cd ChatService.Web
npm install
npm run dev
# App runs on http://localhost:5173
# Vite dev proxy forwards /api and /ws to http://localhost:5139
```

#### Test the Flow
1. Open http://localhost:5173
2. Click "Create account" → Register with username (3-50 chars, alphanumeric + `-_`) and password (6+ chars)
3. Click "Join Chat" → Start chatting
4. Open another tab/window → Register different user → See real-time messages

---

### 3. Docker Compose (Production-like)

#### Set Environment
Create `.env` file in project root:
```bash
JWT_KEY=your-secret-key-min-32-chars-long-for-security!!!
```

Or on Linux/macOS:
```bash
export JWT_KEY="your-secret-key-min-32-chars-long-for-security!!!"
```

#### Start All Services
```bash
docker-compose up --build
# Frontend: http://localhost:3000
# API: http://localhost:5139 (internal only, proxied through nginx)
# MongoDB: localhost:27017 (internal only)
```

#### Stop Services
```bash
docker-compose down
# Or with data cleanup:
docker-compose down -v
```

---

## Configuration

### Backend (API)

Environment variables (set in `.env` or docker-compose or launchSettings.json):

| Variable | Default | Description |
|----------|---------|-------------|
| `MongoDB__ConnectionString` | `mongodb://localhost:27017` | MongoDB connection URI |
| `Jwt__Key` | ❌ Required | Secret key for JWT signing (min 32 chars in production) |
| `Jwt__Issuer` | `ChatService` | JWT issuer claim |
| `Jwt__Audience` | `ChatClient` | JWT audience claim |
| `Cors__AllowedOrigins__0` | `http://localhost:5173` | Frontend origin (can have multiple) |

**Development settings** are in `appsettings.Development.json`.

### Frontend

Environment variables (in `.env` or process.env):

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | `` (relative path) | API base URL (e.g., `http://localhost:5139`) |

---

## API Endpoints

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john",
  "password": "securepassword"
}
```

**Response (201 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john",
  "password": "securepassword"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

### WebSocket

#### Connect
```
WS /ws?access_token=<JWT_TOKEN>
```

**Message Format (Client → Server):**
```json
{
  "text": "Hello, world!"
}
```

**Message Format (Server → Client):**

History:
```json
{
  "type": "history",
  "messages": [
    {
      "type": "message",
      "username": "alice",
      "text": "Hi everyone",
      "timestamp": "2026-06-23T14:30:00Z"
    }
  ]
}
```

New message:
```json
{
  "type": "message",
  "username": "bob",
  "text": "Hey there",
  "timestamp": "2026-06-23T14:31:15Z"
}
```

User events:
```json
{
  "type": "user_joined",
  "username": "charlie"
}
```

```json
{
  "type": "user_left",
  "username": "charlie"
}
```

---

## Project Structure

```
ChatService/
├── ChatService.Api/              # Backend API
│   ├── Controllers/
│   │   └── AuthController.cs     # Login/Register endpoints
│   ├── Models/
│   │   ├── User.cs              # User entity
│   │   ├── ChatMessage.cs        # Message entity
│   │   └── Validation.cs         # DTOs & validation
│   ├── WebSocket/
│   │   └── ChatWebSocketHandler.cs  # Real-time chat logic
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs # Global error handler
│   ├── Program.cs                # Startup & DI configuration
│   ├── appsettings.json          # Production settings
│   ├── appsettings.Development.json
│   └── Dockerfile                # API container image
│
├── ChatService.Web/              # Frontend SPA
│   ├── src/
│   │   ├── App.tsx               # Main chat component
│   │   ├── main.tsx              # React entry point
│   │   ├── App.css               # Styles (Tailwind)
│   │   └── index.css
│   ├── vite.config.ts            # Vite build config with dev proxy
│   ├── tsconfig.json             # TypeScript config
│   ├── package.json              # Dependencies & scripts
│   ├── nginx.conf                # Production reverse proxy config
│   └── Dockerfile                # Frontend container image
│
├── docker-compose.yml            # Multi-container orchestration
├── .env.example                  # Environment template
└── README.md                      # This file
```

---

## Security Considerations

### ✅ Implemented
- **Password hashing** — BCrypt with salt
- **JWT authentication** — 2-hour token expiry
- **CORS policy** — Restricted to configured origins
- **Input validation** — Username/password constraints
- **Error handling** — No stack traces exposed in responses

### ⚠️ For Production
1. **Use HTTPS/WSS** — Enable TLS certificates
2. **Rotate JWT key regularly** — Implement key rotation strategy
3. **Add rate limiting** — Prevent brute-force attacks
4. **Add request logging** — Monitor API usage
5. **Use MongoDB connection pooling** — Configure for concurrent load
6. **Add message rate limiting** — Prevent spam in chat
7. **Implement message moderation** — Content filtering
8. **Enable MongoDB authentication** — Username/password for DB

---

## Development Tasks

### Run Tests
```bash
cd ChatService.Api
dotnet test
```

### Run Linter (Frontend)
```bash
cd ChatService.Web
npm run lint
```

### Format Code
```bash
cd ChatService.Web
npx prettier --write src/
```

---

## Troubleshooting

### Backend fails to start: "Jwt:Key is required"
- Set `JWT_KEY` environment variable (min 32 chars)
- Example: `export JWT_KEY="MySecretKeyWith32OrMoreCharacters!!!"`

### Frontend can't connect to API
- **In dev mode**: Ensure API is running on `http://localhost:5139`
- **In Docker**: Nginx proxy may not be set up; check `nginx.conf`
- **Check browser console**: Look for CORS/WebSocket errors

### WebSocket connection fails with 401
- Token may have expired (2-hour expiry)
- Login again to get a new token
- Check that `access_token` is being sent in query params

### MongoDB connection fails
- Ensure MongoDB is running: `docker ps | grep mongo`
- Check connection string in config (default: `mongodb://localhost:27017`)

---

## Next Steps & Roadmap

- [ ] Add message search
- [ ] Add user profiles & avatars
- [ ] Add message reactions & threading
- [ ] Add file upload/sharing
- [ ] Add push notifications
- [ ] Add end-to-end encryption
- [ ] Scale WebSocket with Redis pub/sub
- [ ] Add API rate limiting
- [ ] Add message moderation
- [ ] Mobile app (React Native)

---

## License

MIT