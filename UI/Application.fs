open System
open System.Reflection
open System.Windows
open System.Windows.Controls
open Microsoft.FSharp.Reflection

module Reflections =
  let getProperty obj propertyName =
    match obj.GetType().GetProperty(propertyName) with
    | null -> None
    | property -> Some(property)

  let getPropertyValue obj propertyName =
    match getProperty obj propertyName with
    | None -> None
    | Some(propertyInfo) -> Some(propertyInfo.GetValue(obj))

  let setPropertyValue obj value propertyName =
    match getProperty obj propertyName  with
    | None -> failwith $"Object'{nameof(obj)}'does not contain property '{propertyName}'"
    | Some(propertyInfo) -> propertyInfo.SetValue(obj, value)

module Elements =
  type Attribute =
    | Name of string
    | Width of int
    | Height of int
    
    with
      member this.CaseName case =
        match case.GetType() with
        | unionType when 
            unionType.IsGenericType 
            && unionType.GetGenericTypeDefinition() = typedefof<Attribute> -> 
                match FSharpValue.GetUnionFields(case, unionType) with
                | _, [| caseValue |] -> string caseValue
                | _ -> failwith "Invalid union case value"
        | _ -> failwith $"'case' is not part of Attributes"

      member this.GetValue case = case.GetType().GetFields().[0].GetValue(case)

  let textBox (attributes: Attribute list) =
    let _textBox = TextBox()
    for attribute in attributes do
      Reflections.setPropertyValue 
        _textBox
        attribute.GetValue
        (attribute.CaseName attribute)


[<STAThread>]
[<EntryPoint>]
let main(_) =
    let mainWindow = Window()

    let app = Application()
    app.MainWindow <- mainWindow

    app.Run()