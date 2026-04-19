module App

open Browser

let root = Browser.Dom.document.getElementById("root")

if not (isNull root) then
    root.innerHTML <- "<h1>F# App működik!</h1>"