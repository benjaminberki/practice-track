module App

open Browser.Dom
open Browser.Types

let root = document.getElementById("root")

if not (isNull root) then
    root.innerHTML <- """
    <h1>Practice Tracker</h1>

    <input id="piece" placeholder="Piece name" />
    <input id="minutes" placeholder="Minutes" type="number" />

    <button id="add">Add session</button>

    <ul id="list"></ul>
    """

    let pieceInput = document.getElementById("piece") :?> HTMLInputElement
    let minutesInput = document.getElementById("minutes") :?> HTMLInputElement
    let addButton = document.getElementById("add") :?> HTMLButtonElement
    let list = document.getElementById("list") :?> HTMLUListElement

    addButton.onclick <- fun _ ->
        let piece = pieceInput.value
        let minutes = minutesInput.value

        let item = document.createElement("li")
        item.textContent <- piece + " - " + minutes + " minutes"

        list.appendChild(item) |> ignore
        