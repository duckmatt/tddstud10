﻿module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.MarginGlpyhTagAndBoundGeneratorTests

open Xunit
open Microsoft.VisualStudio.Text.Formatting
open System.Windows
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System
open Microsoft.VisualStudio.Text
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

let getMTSForline (ss : SnapshotSpan) : IMappingTagSpan<_> seq = 
    let mts = StubMappingTagSpan<TestMarkerTag>()
    mts.Tag <- { testCase = TestCase("FQN:" + ss.GetText(), Uri("ext://test"), "source") }
    upcast [ mts :> IMappingTagSpan<_> ]

[<Fact>]
let ``Empty enumeration returned if there are no input lines``() = 
    let gp = MarginGlpyhTagAndBoundGenerator(fun _ -> [] :> IMappingTagSpan<TestMarkerTag> seq)
    let es = (Point(0.0, 0.0), [] :> ITextViewLine seq) |> gp.Generate
    Assert.Empty(es)

[<Fact>]
let ``Enumeration with 2 tags returned if there is 1 empty line and 2 non empty ones``() = 
    let content = """first non-empty line

third non-empty line"""
    let tv = StubWpfTextView(Point(100.0, 50.0), 25.0, content)
    let lines = (tv :> ITextView).TextViewLines :> ITextViewLine seq
    let gp = MarginGlpyhTagAndBoundGenerator(getMTSForline)
    let es = ((tv :> IWpfTextView).ViewportLocation, lines) |> gp.Generate
    Assert.Equal
        ([| "FQN:first non-empty line"; "FQN:third non-empty line" |], 
         es |> Seq.map (fun (t, _) -> t.testCase.FullyQualifiedName))

[<Fact>]
let ``Check glyph positions returned when there is 1 empty line and 2 non empty ones``() = 
    let content = """first non-empty line

third non-empty line"""
    let tv = StubWpfTextView(Point(100.0, 50.0), 25.0, content)
    let lines = (tv :> ITextView).TextViewLines :> ITextViewLine seq
    let gp = MarginGlpyhTagAndBoundGenerator(getMTSForline)
    let es = ((tv :> IWpfTextView).ViewportLocation, lines) |> gp.Generate
    Assert.Equal([| Rect(1.0, 8.5, 8.0, 8.0)
                    Rect(1.0, 58.5, 8.0, 8.0) |], es |> Seq.map snd)