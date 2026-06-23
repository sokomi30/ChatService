# Contributing to ChatService

## Code Style & Standards

### Backend (.NET / C#)

- **Naming**: PascalCase for classes, methods; camelCase for private fields and local variables
- **Async/Await**: Always use async/await, never `.Result` or `.Wait()`
- **Error Handling**: Use try/catch at service boundary, let middleware handle logging
- **Validation**: Use DataAnnotations on DTOs; never trust user input
- **Logging**: Use `ILogger<T>` injected via DI, log at appropriate levels

### Frontend (React / TypeScript)

- **Components**: Functional components with hooks, one component per file
- **State**: Use `useState` for local state, prefer simple patterns over Redux
- **Styling**: Tailwind CSS utilities, BEM naming for custom classes
- **TypeScript**: Explicit types, no `any`, use interfaces for props

---

## Development Workflow

### Adding a Feature

1. **Create a branch**: `git checkout -b feature/your-feature-name`
2. **Make changes**: Follow code style above
3. **Build & Test**:
   ```bash
   dotnet build ChatService.slnx
   npm run build --prefix ChatService.Web
   ```
4. **Commit**: `git commit -m "Add feature: description"`
5. **Push & Create PR**: Describe what changed and why

### Debugging

**Backend**:
```bash
cd ChatService.Api
dotnet run --configuration Debug
# Attach debugger in VS Code: F5
```

**Frontend**:
```bash
cd ChatService.Web
npm run dev
# DevTools: F12 in browser
```

**WebSocket**:
```javascript
// In browser console:
const ws = new WebSocket('ws://localhost:5173/ws?access_token=YOUR_TOKEN');
ws.onmessage = (e) => console.log('Message:', JSON.parse(e.data));
ws.onerror = (e) => console.error('Error:', e);
```

---

## Project Priorities

### High Priority (Core Features)
- [ ] Test coverage for auth & WebSocket
- [ ] Message persistence & history
- [ ] User profile management

### Medium Priority (UX/DX)
- [ ] Improve error messages
- [ ] Add loading states & spinners
- [ ] Add keyboard shortcuts
- [ ] Mobile responsive design

### Nice to Have
- [ ] Message search
- [ ] User mentions
- [ ] Emoji support
- [ ] Dark mode

---

## Questions?

Open an issue or check the main [README.md](README.md).
