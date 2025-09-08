<p align="center">
  <img src="galactic-orbit-campus.gif" alt="gif" width="600" height="400">
</p>

<h1 align="center">Galactic Orbit</h1>

---

## 🔍 Summary
**Galactic Orbit** is a mobile app that transforms college campuses into interactive AR-based game boards, encouraging students to explore, connect, and engage with their environment through gamified missions and challenges.  

Inspired by **Pokémon Go** and **Ingress**, the app blends real-world exploration with digital gameplay, social features, and AI-enhanced content — without letting AI take over the experience. AI supports gameplay by generating dynamic clues, customizing missions, and powering NPC-like chat characters.  

The app enhances **school pride, student engagement, and campus exploration**, especially for new students or those seeking community.  

---

## 🛠️ MVP Features
- **Map-Based Game Interface:** Real-time campus map with collectibles and missions at campus landmarks.  
- **Location Tracking:** GPS-based triggers to unlock nearby content.  
- **AR Item Interaction:** Collect 3D badges or items using the camera.  
- **Quests & Challenges:** Missions like “Visit the Library,” “Snap a photo at the fountain,” or “Answer trivia.”  
- **User Profiles:** Secure login (Firebase/Auth0), inventory management, and XP tracking.  

---

## 🧠 AI Support (MVP)
- **AI-Generated Clues:** GPT-based riddles tailored to campus spots.  
- **NPC Chat Interface:** Interact with mascots or “ghosts of campus past” for side quests.  
- **Basic Personalization:** Mission suggestions based on user progress.  

---

## 🚀 Stretch Goal Features
- **Gameplay:** Team competitions, dorm-vs-dorm events, and time-limited scavenger hunts.  
- **Customization:** Avatars, cosmetics, and collectible badges.  
- **Advanced AI:** Dynamic NPCs with memory, personalized missions based on majors, and mood-aware suggestions.  
- **Technical:** AR creatures (Unity/Lightship SDK), crowdsourced mission builders, QR/NFC check-ins.  

---

# 📅 Timeline (10-Week Plan)

| 🏁 Week | 📌 Task |
|--------|--------|
| **1**  | **Define project scope, assign team roles, and research campus AR/gamification trends.** <br>• Identify MVP features. <br>• Assign dev/design roles. <br>• Interview students and campus staff for engagement ideas. |
| **2**  | **Design wireframes and finalize tech stack.** <br>• Create Figma mockups of map, AR items, and profile screens. <br>• Select stack (Frontend: Unity + AR SDK, Backend: Firebase/Node.js, DB: Firestore). |
| **3**  | **Backend + Location Tracking Setup.** <br>• Initialize Firebase/Node.js backend. <br>• Configure GPS-based mission unlocks. <br>• Test map integration with real campus coordinates. |
| **4**  | **Implement user authentication and basic profiles.** <br>• Firebase/Auth0 for login. <br>• Basic inventory and XP schema. <br>• Connect frontend with backend for player data. |
| **5**  | **AR item collection system.** <br>• Add basic AR badge/3D object collection. <br>• Test Unity/Lightship SDK integration. <br>• Store collected items in inventory. |
| **6**  | **Quest & Challenge System.** <br>• Static challenges (trivia, photo quests, visit locations). <br>• Backend logic for quest completion + XP. |
| **7**  | **AI Integration (Phase 1).** <br>• AI-generated location riddles. <br>• NPC chat prototype for side quests. <br>• Initial personalization suggestions. |
| **8**  | **UI/UX Refinement and Social Layer.** <br>• Enhance map/AR visuals. <br>• Add leaderboard mockups for stretch goals. <br>• Improve onboarding flow for new students. |
| **9**  | **Testing and Feedback.** <br>• Unit and integration testing. <br>• Pilot test with small student group. <br>• Gather feedback and fix bugs. |
| **10** | **Deployment & Documentation.** <br>• Deploy app to Google Play (beta). <br>• Write developer + user docs. <br>• Launch v1 on one test campus. |

---

## 🏆 Competition

### 🎮 Direct Competitors  
- **Pokémon Go / Niantic (Ingress, Peridot):**  
  - Strengths: Massive brand recognition, refined AR + GPS mechanics, global community.  
  - Weaknesses: Not campus-specific, limited focus on fostering school spirit or student life.  

- **Campus GooseChase (scavenger hunt apps):**  
  - Strengths: Event-based missions, lightweight AR-lite experiences.  
  - Weaknesses: Not persistent, lacks gamified progression and community-building features.  

---

### 🌟 Galactic Orbit Differentiators  
- **Campus-Centered Gameplay:** Focused on **college identity** (mascots, landmarks, dorm rivalries).  
- **Persistent Ecosystem:** Unlike one-off scavenger hunts, gameplay evolves **throughout the semester**.  
- **AI-Powered Missions:** Keeps content **fresh, adaptive, and context-aware** without requiring constant admin input.  
- **Social & Cultural Impact:** Strengthens **student engagement, pride, and community belonging** — more than just a game.  

---

## ⚠️ Roadblocks

### 🔧 Technical
- **Location Accuracy:** Ensuring reliable GPS indoors (potentially add **NFC/QR checkpoints**).  
- **AR SDK Stability:** ARKit/ARCore/Lightship bugs and device fragmentation.  
- **AI Integration:** Cost and efficiency of real-time clue generation.  

### 🎓 Institutional
- **Campus Buy-In:** Need approval for branding (mascot usage, logos, event tie-ins).  
- **Privacy Concerns:** Secure handling of **location + student data**.  
- **WiFi Dead Zones:** Some campuses have unreliable coverage in key areas.  

### 👥 User Engagement
- **Onboarding Curve:** New students may drop off if the app feels complex.  
- **Retention:** Avoid novelty fade-out — missions must **refresh weekly**.  
- **Balancing AI:** Ensure AI enhances gameplay but doesn’t overpower authentic exploration.  

---

## ✅ Mitigation Strategies
- **Tech:** Fallback to QR/NFC tags where GPS fails. Use cloud functions to optimize AI calls.  
- **Institutional:** Early pilot with **student orgs & housing staff** to show value.  
- **Engagement:** Seasonal events (midterms, homecoming, finals week), dorm-vs-dorm competitions, and reward loops.

 ### Git Commands

| Command                        | What it does                                |
|--------------------------------|---------------------------------------------|
| `git branch`                   | Lists all the branches                      |
| `git branch "branch name"`     | Creates a new branch                        |
| `git checkout "branch name"`   | Switches to the specified branch            |
| `git checkout -b "branch name"`| Combines branch creation and checkout       |
| `git add .`                    | Stages all changed files                    |
| `git commit -m "Testing123"`   | Commits with a message                      |
| `git push origin "branch"`     | Pushes to the specified branch              |
| `git pull origin "branch"`     | Pulls updates from the specified branch     |


## 💫 The Team

- **Project Manager:** Shreya S Ramani
- **Industry Mentor:** 
- **Developer 1:** 
- **Developer 2:** 
- **Developer 3:**
- **Developer 4:**
- **Developer 5:**
