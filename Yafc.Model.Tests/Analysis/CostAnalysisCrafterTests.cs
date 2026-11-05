using Xunit;

namespace Yafc.Model.Tests;

[Collection("LuaDependentTests")]
public class CostAnalysisCrafterTests {
    private static Recipe GetRecipe(string name) => (Recipe)Database.objectsByTypeName["Recipe." + name];

    // Each test recipe takes 1 second, has no ingredients and makes one item: the size cost of a 3x3 crafter plus the item cost.
    private const float SmallCrafterCost = (0.1f * 3 * 1.2f) + 0.02f;

    [Fact]
    public void CostAnalysis_CountsEnergyAbove1000MJ() {
        _ = LuaDependentTestHelper.GetProjectForLua("Yafc.Model.Tests.Analysis.CostAnalysisCrafterTests.lua");
        var costs = CostAnalysis.Instance.cost;
        float woodCost = costs[Database.objectsByTypeName["Item.wood"]];

        // These burners use only wood (2 MJ), so the wood pays for the energy, without the flat energy cost on top.
        Assert.Equal(SmallCrafterCost, CostAnalysis.Instance.recipeCost[GetRecipe("modest")], 1e-3f);
        Assert.Equal(SmallCrafterCost + (250 * woodCost), costs[Database.objectsByTypeName["Item.modest"]], 1e-1f);
        Assert.Equal(SmallCrafterCost + (1000 * woodCost), costs[Database.objectsByTypeName["Item.hungry"]], 1e-1f);
    }

    [Fact]
    public void CostAnalysis_CountsCraftersBiggerThan15Tiles() {
        _ = LuaDependentTestHelper.GetProjectForLua("Yafc.Model.Tests.Analysis.CostAnalysisCrafterTests.lua");

        Assert.Equal((0.1f * 20 * 1.2f) + 0.02f, CostAnalysis.Instance.recipeCost[GetRecipe("huge")], 1e-3f);
    }

    [Fact]
    public void CostAnalysis_CraftersWithoutEmissions_HaveNoPollutionCost() {
        Project project = LuaDependentTestHelper.GetProjectForLua("Yafc.Model.Tests.Analysis.CostAnalysisCrafterTests.lua");
        project.settings.PollutionCostModifier = 1;
        Analysis.Do<CostAnalysis>(project);
        var costs = CostAnalysis.Instance.recipeCost;

        Assert.Equal(SmallCrafterCost, costs[GetRecipe("clean")], 1e-3f);
        Assert.Equal(SmallCrafterCost + (5 * 0.01f), costs[GetRecipe("dirty")], 1e-3f);
    }

    [Fact]
    public void CostAnalysis_HeatPoweredCrafter_UsesItsRealEnergyAndSize() {
        _ = LuaDependentTestHelper.GetProjectForLua("Yafc.Model.Tests.Analysis.CostAnalysisCrafterTests.lua");

        // The crafter is 3 wide and uses 1 kW, which is 0.001 MJ per craft.
        Assert.Equal(SmallCrafterCost + (0.1f * 0.001f), CostAnalysis.Instance.recipeCost[GetRecipe("heated")], 1e-3f);
    }
}
