∩╗┐Imports System.Runtime.InteropServices
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic
    Friend Class SubmissionBinder
        Inherits Binder

        Private _submissionBinder As Binder
        Private _typeSymbol As NamedTypeSymbol

        Public Sub New(sourceModule As SourceModuleSymbol, root As SyntaxTree, submissionBinder As Binder)
            Me.New(BinderBuilder.CreateBinderForSourceFileImports(sourceModule, root), sourceModule.ContainingSourceAssembly.DeclaringCompilation.SourceScriptClass, submissionBinder)
        End Sub

        Public Sub New(containingBinder As Binder, typeSymbol As NamedTypeSymbol, submissionBinder As Binder)
            MyBase.New(containingBinder)
            _submissionBinder = submissionBinder
            _typeSymbol = typeSymbol
        End Sub

        Friend Overrides Sub LookupInSingleBinder(lookupResult As LookupResult,
                                                 name As String,
                                                 arity As Integer,
                                                 options As LookupOptions,
                                                 originalBinder As Binder,
                                                 <[In], Out> ByRef useSiteDiagnostics As HashSet(Of DiagnosticInfo))
            _submissionBinder.Lookup(lookupResult, name, arity, options, useSiteDiagnostics)
            Dim submission = Compilation.PreviousSubmission

            While Not lookupResult.HasSymbol AndAlso submission IsNot Nothing
                Dim tree = submission.SyntaxTrees.SingleOrDefault()
                If tree IsNot Nothing Then
                    Dim binder = BinderBuilder.CreateBinderForNamespace(DirectCast(submission.SourceModule, SourceModuleSymbol), tree, submission.RootNamespace)
                    binder.Lookup(lookupResult, name, arity, options, useSiteDiagnostics)
                End If

                submission = submission.PreviousSubmission
            End While
        End Sub

        Public Overrides ReadOnly Property ContainingType As NamedTypeSymbol
            Get
                Return _typeSymbol
            End Get
        End Property

        Public Overrides ReadOnly Property ContainingMember As Symbol
            Get
                Return _typeSymbol
            End Get
        End Property

        Public Overrides ReadOnly Property IsInQuery As Boolean
            Get
                Return False
            End Get
        End Property
    End Class
End Namespace