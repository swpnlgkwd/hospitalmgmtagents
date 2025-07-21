# 🏥 Hospital Shift Scheduling System (ShiftGenie)

An intelligent hospital staff scheduling solution that combines modern ASP.NET Core, Angular, and Azure OpenAI agentic capabilities. It allows schedulers and employees to manage shifts efficiently with the help of a proactive assistant — **ShiftGenie**.

---

## 🚀 Features

- 🧠 **ShiftGenie – Agentic AI Assistant**
  - Understands prompts and context (e.g., “find available staff”)
  - Offers proactive suggestions (e.g., fatigue alerts, uncovered shifts)
  - Executes backend functions through persistent threads and tools

- 📅 **FullCalendar-Based Shift Viewer**
  - View and filter shifts by role, department, or individual
  - Custom event styling with department, role, and type labels

- 🧑‍⚕️ **Role-Based System**
  - **Scheduler:** Full access to all staff, assignments, and calendar views
  - **Employee:** View personal shifts and submit leave requests

- 🧠 **Smart Suggestion Chips**
  - Automatically surfaced actions (e.g., “3 uncovered shifts tomorrow”)
  - Context-aware and dismissable

- 🛡️ **Secure Authentication**
  - JWT-based login with embedded role and staff ID claims

- 💬 **Chat Interface**
  - Chat widget powered by Azure AI Persistent Agent
  - Prompts and responses displayed inline
  - Actionable tools behind-the-scenes

---

## 🧩 Folder Structure

/HospitalStaffMgmtSolution
├── HospitalStaffMgmtApis # ASP.NET Core backend
│ ├── Agents # Agent setup, tool handlers
│ ├── Business # Services and interfaces
│ ├── Controllers # API endpoints
│ ├── Data
│ │ ├── Model # DTOs and database entities
│ │ └── Repository # SQL logic with validations
│ ├── SystemPrompt/systemprompt.txt # Agent instructions
│ └── Program.cs # Entry point
│
├── HospitalStaffMgmtUI # Angular frontend (v17+)
│ ├── app
│ │ ├── components
│ │ │ ├── calendar # FullCalendar wrapper
│ │ │ ├── chat # AI assistant widget
│ │ │ └── suggestions # Smart chips
│ │ ├── services # Auth, API, agent calls
│ │ └── pages # Login & dashboard
│ └── assets
│
└── README.md

markdown
Copy
Edit

---

## 🧠 AI Agent Tools Registered

These tools are defined using `FunctionToolDefinition` and exposed to the agent:

- `findAvailableStaff`
- `assignStaffToShift`
- `cancelShiftAssignment`
- `getStaffSchedule`
- `getShiftCalendar`
- `requestLeave`
- `swapShifts`
- `autoReplaceShiftsForLeave`

The system prompt is stored in:
/HospitalStaffMgmtApis/SystemPrompt/systemprompt.txt

yaml
Copy
Edit

---

## 🔐 Authentication

- Role-based JWTs using `JwtTokenService`
- Claims include: `staffId`, `name`, `roleName`
- Protected backend endpoints using `[Authorize]`

---

## 🧪 Sample Test Users

| Username     | Password | Role      |
|--------------|----------|-----------|
| scheduler1   | test123  | Scheduler |
| nurse1       | test123  | Employee  |

---

## 🛠️ Tech Stack

| Layer        | Technology                         |
|--------------|-------------------------------------|
| Frontend     | Angular 17+, Tailwind CSS          |
| Backend      | .NET 8 Web API (isolated process)  |
| Database     | SQL Server                         |
| AI Agent     | Azure OpenAI + Persistent Agent    |
| Calendar     | FullCalendar v6                    |
| Auth         | JWT                                |

---

## 🔧 Getting Started

### 📦 Backend

```bash
cd HospitalStaffMgmtApis
dotnet build
dotnet run
🌐 Frontend
bash
Copy
Edit
cd HospitalStaffMgmtUI
npm install
ng serve
💡 Smart Suggestion Chips
Defined in database (SmartSuggestion table)

Backend evaluates triggers for:

Uncovered shifts

Leave conflicts

Long shift streaks

Clickable by user to invoke relevant agent tools

📜 License
MIT License – Use freely with attribution.

🙋 Contributing
This is a sample system to showcase AI-assisted shift planning. Contributions to improve functionality, UX, or agent capabilities are welcome.

📞 Contact
Built by Swapnil Gaikwad
Email: [swpnlgkwd@hotmail.com]
GitHub: [github.com/swpngkwd]



