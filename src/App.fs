module App

open Browser.Dom
open Browser.Types

let storageKey = "practiceSessions"

let root = document.getElementById("root")

let updateStats (list: HTMLUListElement) =
    let stats = document.getElementById("stats")
    let items = list.getElementsByTagName("li")

    let mutable totalMinutes = 0

    for i in 0 .. items.length - 1 do
        let item = items.item(i) :?> HTMLLIElement
        let minutesText = item.getAttribute("data-minutes")

        if not (isNull minutesText) then
            match System.Int32.TryParse(minutesText) with
            | true, value -> totalMinutes <- totalMinutes + value
            | _ -> ()

    stats.textContent <-
        "Sessions: " + string items.length +
        " | Total minutes: " + string totalMinutes

let createListItem (list: HTMLUListElement) (piece: string) (minutes: string) =
    let item = document.createElement("li") :?> HTMLLIElement
    item.setAttribute("data-minutes", minutes)

    let text = document.createElement("span")
    text.textContent <- piece + " - " + minutes + " minutes"

    let deleteBtn = document.createElement("button") :?> HTMLButtonElement
    deleteBtn.textContent <- "Delete"
    deleteBtn.setAttribute("style", "margin-left: 10px;")

    deleteBtn.onclick <- fun _ ->
        list.removeChild(item) |> ignore
        let currentHtml = list.innerHTML
        window.localStorage.setItem(storageKey, currentHtml)
        updateStats list

    item.appendChild(text) |> ignore
    item.appendChild(deleteBtn) |> ignore
    item

if not (isNull root) then
    root.innerHTML <- """
    <h1>Practice Tracker</h1>

    <input id="piece" placeholder="Piece name" />
    <input id="minutes" placeholder="Minutes" type="number" />

    <button id="add">Add session</button>

    <p id="error" style="color:red;"></p>

    <ul id="list"></ul>

    <p id="stats">Statistics will appear here</p>
    """

    let pieceInput = document.getElementById("piece") :?> HTMLInputElement
    let minutesInput = document.getElementById("minutes") :?> HTMLInputElement
    let addButton = document.getElementById("add") :?> HTMLButtonElement
    let list = document.getElementById("list") :?> HTMLUListElement
    let error = document.getElementById("error")

    let savedHtml = window.localStorage.getItem(storageKey)
    if not (isNull savedHtml) then
        list.innerHTML <- savedHtml

        let buttons = list.getElementsByTagName("button")
        for i in 0 .. buttons.length - 1 do
            let btn = buttons.item(i) :?> HTMLButtonElement
            btn.onclick <- fun _ ->
                let parent = btn.parentElement
                if not (isNull parent) then
                    list.removeChild(parent) |> ignore
                    window.localStorage.setItem(storageKey, list.innerHTML)
                    updateStats list

    updateStats list

    addButton.onclick <- fun _ ->
        let piece = pieceInput.value.Trim()
        let minutes = minutesInput.value.Trim()

        if piece = "" || minutes = "" then
            error.textContent <- "Please fill in all fields!"
        else
            error.textContent <- ""

            let item = createListItem list piece minutes
            list.appendChild(item) |> ignore

            window.localStorage.setItem(storageKey, list.innerHTML)
            updateStats list

            pieceInput.value <- ""
            minutesInput.value <- ""