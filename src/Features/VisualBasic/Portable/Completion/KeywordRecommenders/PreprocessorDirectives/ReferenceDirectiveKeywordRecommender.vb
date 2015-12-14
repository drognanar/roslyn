' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading
Imports Microsoft.CodeAnalysis.Completion.Providers
Imports Microsoft.CodeAnalysis.VisualBasic.Extensions.ContextQuery
Imports Microsoft.CodeAnalysis.Shared.Extensions

Namespace Microsoft.CodeAnalysis.VisualBasic.Completion.KeywordRecommenders.PreprocessorDirectives
    ''' <summary>
    ''' Recommends the "#R" preprocessor directive
    ''' </summary>
    Friend Class ReferenceDirectiveKeywordRecommender
        Inherits AbstractKeywordRecommender

        Protected Overrides Function RecommendKeywords(context As VisualBasicSyntaxContext, cancellationToken As CancellationToken) As IEnumerable(Of RecommendedKeyword)
            Dim tree = context.SyntaxTree
            ' TODO: use the IsScript extension.
            If context.IsPreprocessorStartContext AndAlso
                    tree.Options.Kind <> SourceCodeKind.Regular AndAlso
                    tree.IsBeforeFirstToken(context.Position, cancellationToken) Then
                Return SpecializedCollections.SingletonEnumerable(New RecommendedKeyword("#R", VBFeaturesResources.ReferenceKeywordTooltip))
            End If

            Return SpecializedCollections.EmptyEnumerable(Of RecommendedKeyword)()
        End Function
    End Class
End Namespace
