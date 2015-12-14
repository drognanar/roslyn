Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Recommendations.PreprocessorDirectives
    Public Class ReferenceDirectiveKeywordRecommenderTests
        <WpfFact>
        <Trait(Traits.Feature, Traits.Features.KeywordRecommending)>
        Public Sub HashReferenceInScriptFile()
            VerifyRecommendationsContain(TestOptions.Script, <File>|</File>, "#R")
        End Sub

        <WpfFact>
        <Trait(Traits.Feature, Traits.Features.KeywordRecommending)>
        Public Sub HashReferenceNotInScriptFile()
            VerifyRecommendationsMissing(TestOptions.Regular, <File>|</File>, "#R")
        End Sub

        <WpfFact>
        <Trait(Traits.Feature, Traits.Features.KeywordRecommending)>
        Public Sub HashReferenceNotInMethod()
            VerifyRecommendationsMissing(TestOptions.Script, <MethodBody>|</MethodBody>, "#R")
        End Sub

        <WpfFact>
        <Trait(Traits.Feature, Traits.Features.KeywordRecommending)>
        Public Sub HashReferenceNotAfterDeclaration()
            VerifyRecommendationsMissing(TestOptions.Script,
<File>
Dim x = 1
|
</File>, "#R")
        End Sub
    End Class
End Namespace
