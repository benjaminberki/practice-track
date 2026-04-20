# Practice Tracker

A web-based practice tracking application built with **F# and Fable**.  
The application allows users to record, manage, and analyze their practice sessions in a structured and persistent way.

---

## Motivation

Consistent practice is essential in learning music or any skill.  
This application was designed to help users:

- Track their daily practice sessions
- Monitor time spent practicing
- Identify patterns in their learning habits

The project also explores functional programming concepts in a real-world web application.

---

## Features

### Session Management
- Add new practice sessions
- Store:
  - Piece title
  - Practice duration (minutes)
  - Date
  - Category (e.g. technique, repertoire)
- Delete sessions with confirmation

### Persistent Storage
- Uses **browser localStorage**
- Sessions remain after page refresh
- JSON-based data storage

### Statistics Dashboard
Automatically calculated statistics:

- Total practice time
- Number of sessions
- Average session length
- Longest session
- Most practiced category

### Filtering / Search
- Filter sessions by:
  - piece name
  - category
- Real-time search

### UI & UX
- Clean card-based layout
- Responsive structure
- Styled using inline CSS
- Immediate feedback (validation errors)

---

## Technical Highlights

- Functional programming with F#
- Record types for domain modeling:
  ```fsharp
  type PracticeSession = { ... }

## How to run

npm install
npm start