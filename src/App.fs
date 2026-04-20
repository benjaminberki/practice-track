module App



open Browser.Dom

open Browser.Types

open Fable.Core

open Fable.Core.JsInterop



// --- DOMAIN MODELS ---

// Using a record for the session data

type PracticeSession =

    { Id: int

      Piece: string

      Minutes: int

      Date: string

      Category: string

      Notes: string }



// Model for practice statistics

type Statistics =

    { TotalMinutes: int

      SessionCount: int

      AverageMinutes: float

      LongestSession: int

      MostPracticedCategory: string }



// --- CONSTANTS ---

let storageKey = "university_project_practice_tracker"



// --- APPLICATION STATE ---

let mutable sessions : PracticeSession array = [||]

let mutable nextId = 1



// --- NATIVE UTILITIES ---

// Using inline for performance and direct JS mapping to avoid Fable translation errors

let inline jsParse<'T> (json: string) : 'T = JS.JSON.parse(json) :?> 'T

let inline jsStringify (value: obj) : string = JS.JSON.stringify(value)



// --- DATA PERSISTENCE LAYER ---

let saveToLocalStorage () =

    try

        let json = jsStringify sessions

        window.localStorage.setItem(storageKey, json)

    with ex ->

        console.error("Failed to save data: ", ex.Message)



let loadFromLocalStorage () =

    let stored = window.localStorage.getItem(storageKey)

    if not (isNull stored) && stored <> "" then

        try

            sessions <- jsParse<PracticeSession array> stored

            if sessions.Length > 0 then

                nextId <- (sessions |> Array.maxBy (fun s -> s.Id)).Id + 1

        with _ -> 

            sessions <- [||]

            nextId <- 1

    else

        sessions <- [||]

        nextId <- 1



// --- BUSINESS LOGIC (STATISTICS) ---

let calculateStatistics (items: PracticeSession array) : Statistics =

    if items.Length = 0 then

        { TotalMinutes = 0; SessionCount = 0; AverageMinutes = 0.0; LongestSession = 0; MostPracticedCategory = "N/A" }

    else

        let total = items |> Array.sumBy (fun s -> s.Minutes)

        let maxMin = items |> Array.maxBy (fun s -> s.Minutes) |> fun s -> s.Minutes

        

        // Grouping logic to find the most practiced category

        let topCategory = 

            items 

            |> Array.groupBy (fun s -> s.Category)

            |> Array.map (fun (cat, group) -> cat, group.Length)

            |> Array.sortByDescending snd

            |> Array.tryHead

            |> function | Some (c, _) -> c | None -> "N/A"



        { TotalMinutes = total

          SessionCount = items.Length

          AverageMinutes = float total / float items.Length

          LongestSession = maxMin

          MostPracticedCategory = topCategory }



// --- UI COMPONENTS & RENDERING ---



// Forward reference for the render function to allow circular event calls

let mutable refreshUI : unit -> unit = fun () -> ()



let createDeleteButton (id: int) =

    let btn = document.createElement("button") :?> HTMLButtonElement

    btn.textContent <- "Remove"

    btn.className <- "delete-button"

    btn.setAttribute("style", "background:#e74c3c; color:white; border:none; padding:8px 12px; cursor:pointer; border-radius:4px; font-weight:bold;")

    btn.onclick <- fun _ ->

        if window.confirm("Are you sure you want to delete this session?") then

            sessions <- sessions |> Array.filter (fun s -> s.Id <> id)

            saveToLocalStorage()

            refreshUI()

    btn



let renderSessionList (listElement: HTMLUListElement) (query: string) =

    listElement.innerHTML <- ""

    

    let filtered = 

        sessions 

        |> Array.filter (fun s -> 

            s.Piece.ToLower().Contains(query) || 

            s.Category.ToLower().Contains(query))

        |> Array.sortByDescending (fun s -> s.Date)



    if filtered.Length = 0 then

        let emptyMsg = document.createElement("p")

        emptyMsg.textContent <- "No sessions found."

        emptyMsg.setAttribute("style", "text-align:center; color:#95a5a6; padding:20px;")

        listElement.appendChild(emptyMsg) |> ignore

    else

        filtered |> Array.iter (fun s ->

            let li = document.createElement("li") :?> HTMLLIElement

            li.setAttribute("style", "display:flex; justify-content:space-between; align-items:center; background:#ffffff; margin:10px 0; padding:15px; border-radius:8px; border-left:5px solid #3498db; box-shadow: 0 2px 5px rgba(0,0,0,0.05);")

            

            let container = document.createElement("div")

            let title = document.createElement("div")

            title.innerHTML <- sprintf "<span style='font-size:1.1em; font-weight:bold; color:#2c3e50;'>%s</span>" s.Piece

            

            let details = document.createElement("div")

            details.innerHTML <- sprintf "<small style='color:#7f8c8d;'>%d minutes • %s • <strong>%s</strong></small>" 

                                    s.Minutes s.Date s.Category

            

            container.appendChild(title) |> ignore

            container.appendChild(details) |> ignore

            

            let delBtn = createDeleteButton s.Id

            

            li.appendChild(container) |> ignore

            li.appendChild(delBtn) |> ignore

            listElement.appendChild(li) |> ignore

        )



let renderStatsBoard (statsElement: HTMLElement) =

    let stats = calculateStatistics sessions

    statsElement.innerHTML <- sprintf """

        <div style="display:grid; grid-template-columns: 1fr 1fr; gap:10px; text-align:center;">

            <div style="background:#fff; padding:10px; border-radius:6px; border:1px solid #eee;">

                <div style="font-size:0.8em; color:#7f8c8d;">Total Time</div>

                <div style="font-weight:bold;">%d min</div>

            </div>

            <div style="background:#fff; padding:10px; border-radius:6px; border:1px solid #eee;">

                <div style="font-size:0.8em; color:#7f8c8d;">Sessions</div>

                <div style="font-weight:bold;">%d</div>

            </div>

            <div style="background:#fff; padding:10px; border-radius:6px; border:1px solid #eee;">

                <div style="font-size:0.8em; color:#7f8c8d;">Avg Duration</div>

                <div style="font-weight:bold;">%.1f min</div>

            </div>

            <div style="background:#fff; padding:10px; border-radius:6px; border:1px solid #eee;">

                <div style="font-size:0.8em; color:#7f8c8d;">Top Category</div>

                <div style="font-weight:bold;">%s</div>

            </div>

        </div>

    """ stats.TotalMinutes stats.SessionCount stats.AverageMinutes stats.MostPracticedCategory



let coreRender () =

    let listElement = document.getElementById("list") :?> HTMLUListElement

    let statsElement = document.getElementById("stats-board") :?> HTMLElement

    let searchInput = document.getElementById("search-input") :?> HTMLInputElement

    let errorDisplay = document.getElementById("error-display") :?> HTMLElement

    

    errorDisplay.textContent <- ""

    renderStatsBoard statsElement

    renderSessionList listElement (searchInput.value.ToLower())



// Set the reference

refreshUI <- coreRender



// --- FORM VALIDATION & INTERACTION ---

let handleAddSession () =

    let pInput = document.getElementById("piece-input") :?> HTMLInputElement

    let mInput = document.getElementById("mins-input") :?> HTMLInputElement

    let dInput = document.getElementById("date-input") :?> HTMLInputElement

    let cInput = document.getElementById("cat-input") :?> HTMLInputElement

    let errorDisplay = document.getElementById("error-display") :?> HTMLElement



    // Simple validation logic

    if System.String.IsNullOrWhiteSpace(pInput.value) then

        errorDisplay.textContent <- "Error: Piece title cannot be empty."

    elif System.String.IsNullOrWhiteSpace(mInput.value) || (int mInput.value) <= 0 then

        errorDisplay.textContent <- "Error: Please enter a valid number of minutes."

    else

        let newSession = {

            Id = nextId

            Piece = pInput.value

            Minutes = int mInput.value

            Date = dInput.value

            Category = if cInput.value = "" then "Uncategorized" else cInput.value

            Notes = ""

        }

        

        nextId <- nextId + 1

        sessions <- Array.append [| newSession |] sessions // Add to top

        saveToLocalStorage()

        

        // Reset inputs

        pInput.value <- ""

        mInput.value <- ""

        cInput.value <- ""

        errorDisplay.textContent <- ""

        

        refreshUI()



// --- INITIALIZATION ---

let initializeApp () =

    let root = document.getElementById("root")

    if isNull root then () else



    // Detailed UI structure with inline CSS for university presentation

    root.innerHTML <- """

        <div style="max-width:550px; margin:40px auto; font-family:'Segoe UI', Tahoma, sans-serif; padding:30px; background:#f4f7f6; border-radius:15px; box-shadow: 0 10px 25px rgba(0,0,0,0.1);">

            <header style="text-align:center; margin-bottom:25px;">

                <h1 style="color:#2c3e50; margin:0;">Practice Tracker</h1>

              

            </header>

            

            <div id="form-container" style="background:#fff; padding:20px; border-radius:10px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); margin-bottom:25px;">

                <h3 style="margin-top:0; color:#34495e; border-bottom:1px solid #eee; padding-bottom:10px;">Add New Session</h3>

                <div style="display:grid; gap:12px;">

                    <input id="piece-input" placeholder="Piece Title (e.g. Moonlight Sonata)" style="padding:12px; border:1px solid #dcdde1; border-radius:6px;">

                    <div style="display:flex; gap:10px;">

                        <input id="mins-input" type="number" placeholder="Minutes" style="flex:1; padding:12px; border:1px solid #dcdde1; border-radius:6px;">

                        <input id="date-input" type="date" style="flex:1; padding:12px; border:1px solid #dcdde1; border-radius:6px;">

                    </div>

                    <input id="cat-input" placeholder="Category (e.g. Technique, Repertoire)" style="padding:12px; border:1px solid #dcdde1; border-radius:6px;">

                    <button id="add-btn" style="padding:15px; background:#2ecc71; color:white; border:none; cursor:pointer; border-radius:6px; font-weight:bold; font-size:1.1em; transition: 0.3s;">Save Session</button>

                    <div id="error-display" style="color:#e74c3c; font-size:0.85em; font-weight:bold; min-height:1em;"></div>

                </div>

            </div>



            <div id="stats-container" style="margin-bottom:25px;">

                <h3 style="color:#34495e;">Practice Statistics</h3>

                <div id="stats-board"></div>

            </div>



            <div id="list-container">

                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:10px;">

                    <h3 style="margin:0; color:#34495e;">Session Log</h3>

                    <input id="search-input" placeholder="Filter by name..." style="padding:8px; border:1px solid #dcdde1; border-radius:6px; font-size:0.8em;">

                </div>

                <ul id="list" style="padding:0; list-style:none;"></ul>

            </div>

            

            <footer style="margin-top:30px; text-align:center; font-size:0.7em; color:#bdc3c7; border-top:1px solid #eee; padding-top:10px;">

                F# Web Application Framework - Functional Programming Course

            </footer>

        </div>

    """



    // Event Listeners

    let addBtn = document.getElementById("add-btn") :?> HTMLButtonElement

    let searchIn = document.getElementById("search-input") :?> HTMLInputElement

    let dateIn = document.getElementById("date-input") :?> HTMLInputElement



    // Initialize with current date

    dateIn.valueAsDate <- JS.Constructors.Date.Create()



    addBtn.onclick <- fun _ -> handleAddSession()

    searchIn.oninput <- fun _ -> refreshUI()



    // Startup logic

    loadFromLocalStorage()

    refreshUI()



// Entry point

initializeApp()