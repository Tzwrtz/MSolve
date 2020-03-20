using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.NonLinear;
using ISAAR.MSolve.Discretization;
using ISAAR.MSolve.Discretization.Integration.Quadratures;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Direct;
using System.Collections.Generic;
using Xunit;

namespace ISAAR.MSolve.Tests
{
    public class DisplacementControlWithHexa8NonLinearTest
    {
        private const int subdomainID = 0;

        [Fact]
        public void DisplacementControlWithHexa8NonLinear_v2()
        {
            Numerical.LinearAlgebra.VectorExtensions.AssignTotalAffinityCount();
            const int increments = 10;
            const double nodalDisplacement = -5.0;
            const double youngModulus = 4.0;
            const double poissonRatio = 0.4;

            // Create Model
            Model_v2 model = new Model_v2();

            // Create Subdomain
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Create Elastic Material
            var solidMaterial = new ElasticMaterial3D_v2()
            {
                YoungModulus = youngModulus,
                PoissonRatio = poissonRatio,
            };

            // Node creation            
            Node_v2 node1 = new Node_v2 { ID = 1, X =  0.0, Y =  0.0, Z =  0.0 };
            Node_v2 node2 = new Node_v2 { ID = 2, X = 10.0, Y =  0.0, Z =  0.0 };
            Node_v2 node3 = new Node_v2 { ID = 3, X =  0.0, Y = 10.0, Z =  0.0 };
            Node_v2 node4 = new Node_v2 { ID = 4, X = 10.0, Y = 10.0, Z =  0.0 };
            Node_v2 node5 = new Node_v2 { ID = 5, X =  0.0, Y =  0.0, Z = 10.0 };
            Node_v2 node6 = new Node_v2 { ID = 6, X = 10.0, Y =  0.0, Z = 10.0 };
            Node_v2 node7 = new Node_v2 { ID = 7, X =  0.0, Y = 10.0, Z = 10.0 };
            Node_v2 node8 = new Node_v2 { ID = 8, X = 10.0, Y = 10.0, Z = 10.0 };

            // Create List of nodes
            IList<Node_v2> nodes = new List<Node_v2>();
            nodes.Add(node1);
            nodes.Add(node2);
            nodes.Add(node3);
            nodes.Add(node4);
            nodes.Add(node5);
            nodes.Add(node6);
            nodes.Add(node7);
            nodes.Add(node8);

            // Add nodes to the nodes dictonary of the model
            for (int i = 0; i < nodes.Count; ++i)
            {
                model.NodesDictionary.Add(i + 1, nodes[i]);
            }                                  

            // Hexa8NonLinear element definition
            var hexa8NLelement = new Element_v2()
            {
                ID = 1,
                ElementType = new Hexa8NonLinear_v2(solidMaterial, GaussLegendre3D.GetQuadrature(3, 3, 3))
            };

            // Add nodes to the created element
            hexa8NLelement.AddNode(model.NodesDictionary[node8.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node7.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node5.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node6.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node4.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node3.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node1.ID]);
            hexa8NLelement.AddNode(model.NodesDictionary[node2.ID]);

            // Add Hexa element to the element and subdomains dictionary of the model
            model.ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
            model.SubdomainsDictionary[subdomainID].Elements.Add(hexa8NLelement);

            // Boundary Condtitions
            for (int iNode = 1; iNode <= 4; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
            }

            // Boundary Condtitions - Prescribed DOFs          
            for (int iNode = 5; iNode <= 8; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
            }

            // Choose linear equation system solver
            var solverBuilder = new SkylineSolver.Builder();
            SkylineSolver solver = solverBuilder.BuildSolver(model);
            //var solverBuilder = new SuiteSparseSolver.Builder();
            //SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: DisplacementControlAnalyzer 
            var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
            var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, increments)
            {
                MaxIterationsPerIncrement = 50,
                NumIterationsForMatrixRebuild = 1,
                ResidualTolerance = 1E-03
            };
            var childAnalyzer = childAnalyzerBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();

            // Solution
            var analyzer = (DisplacementControlAnalyzer_v2)childAnalyzer;
            var solution = analyzer.uPlusdu[0];

            // Check output
            Assert.Equal(-1.019828463478385, solution[0], 8);
        }
    }
}
