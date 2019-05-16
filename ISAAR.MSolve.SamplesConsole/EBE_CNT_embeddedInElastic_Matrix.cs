using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Elements.SupportiveClasses;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Skyline;
using System;
using System.Collections.Generic;
using System.Text;
using ISAAR.MSolve.Discretization.Interfaces;
using Xunit;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.FEM;
using ISAAR.MSolve.Discretization.Providers;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.FEM.Embedding;
using System.Linq;
using ISAAR.MSolve.Discretization.Integration.Quadratures;
using System.IO;
using ISAAR.MSolve.Solvers.Direct;
using ISAAR.MSolve.Discretization;
using ISAAR.MSolve.Analyzers.NonLinear;

namespace ISAAR.MSolve.SamplesConsole
{
    public class EBE_CNT_embeddedInElastic_Matrix
    {
        private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\MSolveResults";
        private const int subdomainID = 0;
        private const int hostElements = 10;
        private const int hostNodes = 44;
        private const int embeddedElements = 10;
        private const int embeddedNodes = 11;

        public static void EBEembeddedInMatrix_NewtonRaphson()
        {
            VectorExtensions.AssignTotalAffinityCount();

            // No. of increments
            int increments = 100;

            // Model creation
            var model = new Model_v2();

            // Subdomains
            //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Variables
            int monitorNode = hostNodes;
            DOFType monitorDof = DOFType.Z;

            // Choose model
            EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder(model);

            // Boundary Conditions - Left End [End-1]
            for (int iNode = 1; iNode <= 4; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
            }

            // Boundary Conditions - Bottom End [Bottom]
            for (int iNode = 1; iNode <= 41; iNode += 4)
            {
                for (int j = 0; j <= 1; j++)
                {
                    model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                }
            }

            //Compression Loading
            double nodalLoad = -10.0; //0.40;
            for (int iNode = 41; iNode <= 44; iNode++) //[End-2]
            {
                model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
            }

            // Choose linear equation system solver
            var solverBuilder = new SkylineSolver.Builder();
            SkylineSolver solver = solverBuilder.BuildSolver(model);
            //var solverBuilder = new SuiteSparseSolver.Builder();
            //SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
            var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
            {
                ResidualTolerance = 5E-03
            };

            LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            string outputFile = outputDirectory + "\\EBE-CNT-Embedded-3D_Results.txt";
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }

        public static void EBEembeddedInMatrixCohesive_NewtonRaphson()
        {
            VectorExtensions.AssignTotalAffinityCount();

            // No. of increments
            int increments = 100;

            // Model creation
            var model = new Model_v2();

            // Subdomains
            //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Variables
            int monitorNode = hostNodes;
            DOFType monitorDof = DOFType.Z;

            // Choose model
            EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder(model);

            // Boundary Conditions - Left End [End-1]
            for (int iNode = 1; iNode <= 4; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
            }

            // Boundary Conditions - Bottom End [Bottom]
            for (int iNode = 1; iNode <= 41; iNode += 4)
            {
                for (int j = 0; j <= 1; j++)
                {
                    model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                }
            }

            //Compression Loading
            double nodalLoad = -10.0; //0.40;
            for (int iNode = 41; iNode <= 44; iNode++) //[End-2]
            {
                model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
            }

            // Choose linear equation system solver
            var solverBuilder = new SkylineSolver.Builder();
            SkylineSolver solver = solverBuilder.BuildSolver(model);
            //var solverBuilder = new SuiteSparseSolver.Builder();
            //SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
            var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
            {
                ResidualTolerance = 5E-03
            };

            LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            string outputFile = outputDirectory + "\\EBE-CNT-Embedded-Cohesive-3D_Results.txt";
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }

        public static void EBEembeddedInMatrix_DisplacementControl()
        {
            // Not implemented yet
            throw new NotImplementedException();
        }

        public static void EBEembeddedInMatrixCohesive_DisplacementControl()
        {
            // Not implemented yet
            throw new NotImplementedException();
        }

        public static class EBEEmbeddedModelBuilder
        {
            public static void FullyBondedEmbeddedBuilder(Model_v2 model)
            {
                HostElements(model);
                EmbeddedElements(model);                
                var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
            }

            public static void CohesiveEmbeddedBuilder(Model_v2 model)
            {
                HostElements(model);
                EmbeddedElements(model);
                CohesiveBeamElements(model);
                var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
            }

            private static void HostElements(Model_v2 model)
            {
                string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\input files"; //"..\..\..\Resources\Beam3DInputFiles";

                string MatrixGeometryFileName = "MATRIX_3D-L_x=10-L_y=10-L_z=100-1x1x10-Geometry_MSolve.inp";
                
                string MatrixGonnectivityFileName = "MATRIX_3D-L_x=10-L_y=10-L_z=100-1x1x10-ConnMatr_MSolve.inp";
                
                int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixGonnectivityFileName).Count();

                // Nodes Geometry                
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixGeometryFileName))
                {
                    for (int i = 0; i < matrixNodes; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int nodeID = int.Parse(bits[0]);
                        double nodeX = double.Parse(bits[1]);
                        double nodeY = double.Parse(bits[2]);
                        double nodeZ = double.Parse(bits[3]);
                        model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                    }
                }

                // Create Material
                var solidMaterial = new ElasticMaterial3D_v2()
                {
                    YoungModulus = 1.00,
                    PoissonRatio = 0.30,
                };

                // Generate elements
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixGonnectivityFileName))
                {
                    for (int i = 0; i < matrixElements; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int elementID = int.Parse(bits[0]);
                        int node1 = int.Parse(bits[1]);
                        int node2 = int.Parse(bits[2]);
                        int node3 = int.Parse(bits[3]);
                        int node4 = int.Parse(bits[4]);
                        int node5 = int.Parse(bits[5]);
                        int node6 = int.Parse(bits[6]);
                        int node7 = int.Parse(bits[7]);
                        int node8 = int.Parse(bits[8]);
                        // Hexa8NL element definition
                        var hexa8NLelement = new Element_v2()
                        {
                            ID = elementID,
                            ElementType = new Hexa8NonLinear_v2(solidMaterial, GaussLegendre3D.GetQuadrature(3, 3, 3))
                        };
                        // Add nodes to the created element
                        hexa8NLelement.AddNode(model.NodesDictionary[node1]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node2]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node3]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node4]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node5]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node6]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node7]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node8]);
                        // Add Hexa element to the element and subdomains dictionary of the model
                        model.ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
                        //model.SubdomainsDictionary[0].ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
                        model.SubdomainsDictionary[0].Elements.Add(hexa8NLelement);
                    }
                }
            }
            
            private static void EmbeddedElements(Model_v2 model)
            {
                // define mechanical properties
                double youngModulus = 1.0; // 5490; // 
                double shearModulus = 1.0; // 871; // 
                double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                double area = 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                double inertiaY = 1058.55;
                double inertiaZ = 1058.55;
                double torsionalInertia = 496.38;
                double effectiveAreaY = area;
                double effectiveAreaZ = area;
                string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\input files"; //"..\..\..\Resources\Beam3DInputFiles";

                string CNTgeometryFileName = "EmbeddedCNT-20-20-L=100-h=2-k=1-EBE-L=10-NumberOfCNTs=1-Geometry_beam.inp";
                
                string CNTconnectivityFileName = "EmbeddedCNT-20-20-L=100-h=2-k=1-EBE-L=10-NumberOfCNTs=1-ConnMatr_beam.inp";
                
                int CNTNodes = File.ReadLines(workingDirectory + '\\' + CNTgeometryFileName).Count();
                int CNTElems = File.ReadLines(workingDirectory + '\\' + CNTconnectivityFileName).Count();

                // Geometry
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTgeometryFileName))
                {
                    for (int i = 0; i < CNTNodes; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int nodeID = int.Parse(bits[0]) + hostNodes; // matrixNodes
                        double nodeX = double.Parse(bits[1]);
                        double nodeY = double.Parse(bits[2]);
                        double nodeZ = double.Parse(bits[3]);
                        model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                    }
                }

                // Create new 3D material
                var beamMaterial = new ElasticMaterial3D_v2
                {
                    YoungModulus = youngModulus,
                    PoissonRatio = poissonRatio,
                };

                // Create new Beam3D section and element
                var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);
                // element nodes

                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTconnectivityFileName))
                {
                    for (int i = 0; i < CNTElems; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int elementID = int.Parse(bits[0]) + hostElements; // 8100; // matrixElements
                        int node1 = int.Parse(bits[1]) + hostNodes; // 10100; // matrixNodes
                        int node2 = int.Parse(bits[2]) + hostNodes; // 10100; // matrixNodes
                        // element nodes
                        var elementNodes = new List<Node_v2>();
                        elementNodes.Add(model.NodesDictionary[node1]);
                        elementNodes.Add(model.NodesDictionary[node2]);
                        // create element
                        var beam_1 = new Beam3DCorotationalQuaternion_v2(elementNodes, beamMaterial, 7.85, beamSection);
                        var beamElement = new Element_v2 { ID = elementID, ElementType = beam_1 };
                        // Add nodes to the created element
                        beamElement.AddNode(model.NodesDictionary[node1]);
                        beamElement.AddNode(model.NodesDictionary[node2]);
                        // beam stiffness matrix
                        // var a = beam_1.StiffnessMatrix(beamElement);
                        // Add beam element to the element and subdomains dictionary of the model
                        model.ElementsDictionary.Add(beamElement.ID, beamElement);
                        //model.SubdomainsDictionary[0].ElementsDictionary.Add(beamElement.ID, beamElement);
                        model.SubdomainsDictionary[0].Elements.Add(beamElement);
                    }
                }
            }

            private static void CohesiveBeamElements(Model_v2 model)
            {
                // define mechanical properties
                double youngModulus = 1.0;
                double shearModulus = 1.0; 
                double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; // 2.15; // 0.034; //
                double area = 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                double inertiaY = 1058.55;
                double inertiaZ = 1058.55;
                double torsionalInertia = 496.38;
                double effectiveAreaY = area;
                double effectiveAreaZ = area;
                string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\input files"; //"..\..\..\Resources\Beam3DInputFiles";

                string CNTgeometryFileName = "EmbeddedCNT-20-20-L=100-h=2-k=1-EBE-L=10-NumberOfCNTs=1-Geometry_beam.inp";
                
                string CNTconnectivityFileName = "EmbeddedCNT-20-20-L=100-h=2-k=1-EBE-L=10-NumberOfCNTs=1-ConnMatr_beam.inp";
                
                int CNTNodes = File.ReadLines(workingDirectory + '\\' + CNTgeometryFileName).Count();
                int CNTElems = File.ReadLines(workingDirectory + '\\' + CNTconnectivityFileName).Count();

                // Geometry
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTgeometryFileName))
                {
                    for (int i = 0; i < CNTNodes; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // 10100; // matrixNodes
                        double nodeX = double.Parse(bits[1]);
                        double nodeY = double.Parse(bits[2]);
                        double nodeZ = double.Parse(bits[3]);
                        model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                    }
                }

                // Create Cohesive Material
                var cohesiveMaterial = new BondSlipCohMat_v2(100, 10, 100, 10, 1, new double[2], new double[2], 1e-10);

                // Create Elastic 3D Material
                var elasticMaterial = new ElasticMaterial3D_v2
                {
                    YoungModulus = youngModulus,
                    PoissonRatio = poissonRatio,
                };

                // Create Beam3D Section
                var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                // element nodes
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTconnectivityFileName))
                {
                    for (int i = 0; i < CNTElems; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int elementID = int.Parse(bits[0]) + (hostElements + embeddedElements); // matrixElements + CNTelements
                        int node1 = int.Parse(bits[1]) + (hostNodes + embeddedNodes); // matrixNodes + CNTnodes
                        int node2 = int.Parse(bits[2]) + (hostNodes + embeddedNodes); // matrixNodes + CNTnodes
                        // element nodes clone
                        var elementNodesClone = new List<Node_v2>();
                        elementNodesClone.Add(model.NodesDictionary[node1]);
                        elementNodesClone.Add(model.NodesDictionary[node2]);
                        // element nodes beam
                        var elementNodesBeam = new List<Node_v2>();
                        elementNodesBeam.Add(model.NodesDictionary[node1 - embeddedNodes]);
                        elementNodesBeam.Add(model.NodesDictionary[node2 - embeddedNodes]);
                        // Create Cohesive Beam Element
                        var cohesiveElement = new Element_v2()
                        {
                            ID = elementID,
                            ElementType = new CohesiveBeam3DToBeam3D(cohesiveMaterial, GaussLegendre1D.GetQuadrature(2), elementNodesBeam,
                                elementNodesClone, elasticMaterial, 1, beamSection)
                        };
                        // Add beam element to the element and subdomains dictionary of the model
                        model.ElementsDictionary.Add(cohesiveElement.ID, cohesiveElement);
                        // Add Cohesive Element Nodes (!)
                        model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                        model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                        model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                        model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                        // Add Cohesive Element in Subdomain
                        model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                    }
                }
            }

        }
    }
}
