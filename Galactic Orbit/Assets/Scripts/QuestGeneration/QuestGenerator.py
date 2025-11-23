import math
import random
import requests
import json

# ==========================
# CONSTANTS & CONFIG
# ==========================

LMSTUDIO_URL = "http://127.0.0.1:1234/v1/completions"
DATABASE_URL = "https://galactic-orbit-default-rtdb.firebaseio.com/"

UTD_LOCATIONS_FROM_CSV = [
    "Eugene McDermott Library (MC)",
    "Student Union (SU)",
    "Activity Center (AB)",
    "University Theatre (TH)",
    "Erik Jonsson Academic Center (JO)",
    "Engineering and Computer Science South (ECSS)",
    "Engineering and Computer Science West (ESCW)",
    "Engineering and Computer Science North (ECSN)",
    "Cecil H. Green Hall (GR)",
    "Bioengineering and Sciences Building (BSB)",
    "Administration Building (AD)",
    "Sciences Building (SCI)",
    "Callier Center Richardson (CR)",
    "Founders Building (FO)",
    "Founders North (FN)",
    "Classroom Building (CB)",
    "Karl Hoblitzelle Hall (HH)",
    "Student Services Building (SSB)",
    "Student Services Building Addition (SSA)",
    "Naveen Jindal School of Management (JSOM)",
    "Science Learning Center (SLC)"
]

# ==========================
# PROMPT GENERATION
# ==========================

def build_payload(location):
    print(f"Building prompt for location: {location.split('(')[0].strip()}\n")
    prompt = (
        "You are a quest generator for a Galaxy-themed AR campus exploration game.\n"
        f"A Planet AR Object can be found at {location.split('(')[0].strip()}.\n\n"
        "Your output must strictly follow this format:\n\n"
        "Title: <title>\n"
        "Description: <description>\n\n"
        "Title rules:\n"
        "- Must follow the style of these lines about finding the Planet object:\n"
        "  • Short and engaging.\n"
        "  • Something about uncovering, discovering, finding, and exploring something in <location>"
        "  • Finding object"
        f"Replacing <location> with the full name of: {location.split('(')[0].strip()}.\n"
        "- Must be exactly one sentence.\n\n"
        "Description rules:\n"
        "- Exactly two sentences.\n"
        "- No explanations, no reasoning.\n"
        "- No extra text.\n"
        "- ONLY ONE quest must be generated.\n\n"
        f"Player Context: You are near the {location.split('(')[0].strip()}."
    )

    return {
        "model": "qwen2.5-7b-instruct",
        "prompt": prompt,
        "max_tokens": 300,
        "temperature": 0.8,
        "n": 1,
        "stream": False
    }

# ==========================
# PARSE MODEL RESPONSE
# ==========================

def parseJSONResponse(response):
    data = response.json()
    quests = []

    for choice in data.get("choices", []):
        quest_text = choice["text"].strip()

        title_start = quest_text.find("Title: ") + len("Title: ")
        description_start = quest_text.find("Description: ") + len("Description: ")

        # Find line ends
        title_end = quest_text.find("\n", title_start)
        description_end = quest_text.find("\n", description_start)

        title = quest_text[title_start:title_end].strip()
        description = quest_text[description_start:description_end].strip()

        quests.append({
            "title": title,
            "description": description,
            "xp": math.floor(random.uniform(50, 150))
        })

    return quests

# ==========================
# FIREBASE UPLOAD
# ==========================

def add_quest_to_firebase(quest):
    url = f"{DATABASE_URL}/quests.json"
    response = requests.post(url, json=quest)
    return response.json()

# ==========================
# MAIN PROGRAM
# ==========================

def clear_all_quests():
    url = f"{DATABASE_URL}/quests.json"
    response = requests.delete(url)
    if response.status_code == 200:
        print("All quests deleted successfully.")
    else:
        print("Failed to delete quests:", response.text)

def main():
    numberOfQuestsToGenerate = 5

    clear_all_quests()
    for i in range(numberOfQuestsToGenerate):
        # Build and send request
        print(f"Generating quest {i+1}...\n")
        
        index = math.floor(random.random() * len(UTD_LOCATIONS_FROM_CSV))
        location = UTD_LOCATIONS_FROM_CSV[index]
        UTD_LOCATIONS_FROM_CSV.pop(index)

        payload = build_payload(location)
        response = requests.post(LMSTUDIO_URL, json=payload)

        # Parse quests
        quests = parseJSONResponse(response)

        # Upload to Firebase
        for quest in quests:
            print(f"Uploading quest: {quest['title']}")
            quest["location"] = location
            result = add_quest_to_firebase(quest)
            print("Firebase ID:", result.get("name", "ERROR"))
            print()

# ==========================
# ENTRY POINT
# ==========================

if __name__ == "__main__":
    main()
