# ChatBox

**ChatBox** is a real-time messaging app inspired by WhatsApp, built with ASP.NET Core Web API. It supports one-to-one and group chats, message statuses, media sharing, and user privacy features.

---

## 📌 Features

- 🔒 User Registration & Authentication
- 💬 One-to-One Messaging
- 👥 Group Chats with Admins
- 📤 Message Delivery Status (Sent, Delivered, Seen)
- 📎 Media & File Sharing
- 🚫 Block/Unblock Users

---

## 🧱 ERD Overview

The system is designed using a normalized database schema to support both individual and group messaging efficiently.

**Main Tables**:
- `User`
- `Chat` (handles both group and personal chats)
- `ChatParticipant`
- `Message`
- `MessageStatus`
- `BlockedUser`

> Diagram not included here – see `/docs/ERD.png` or use [dbdiagram.io](https://dbdiagram.io) to visualize.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server or compatible DB
- (Optional) Visual Studio / VS Code

### Installation

```bash
# Clone the repo
git clone https://github.com/mostafijur566/ChatBox.git
cd ChatBox

# Restore dependencies
dotnet restore

# Apply EF Core migrations (after setting connection string in appsettings.json)
dotnet ef migrations add Initial
dotnet ef database update

# Run the project
dotnet run
```

---

## 📝 License

This project is licensed under the [MIT License](LICENSE).

---

## ✨ Credits

Built with ❤️ by [@mostafijur566](https://github.com/mostafijur566)
