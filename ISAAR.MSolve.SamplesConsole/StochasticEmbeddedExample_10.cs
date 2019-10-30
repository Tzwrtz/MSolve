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
    public class StochasticEmbeddedExample_10
    {
        public static class Run2a_Elastic
        {
            private const string outputDirectory = @"C:\Users\tzwrt\Desktop\output files\EmbeddedExample_10"; //"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 1;
            private const int hostNodes = 8;
            private const int embeddedElements = 1;
            private const int embeddedNodes = 2;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 5;
            private const DOFType monitorDof = DOFType.Z;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 7; iNode = iNode + 2)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }                               

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 2; iNode <= 6; iNode = iNode + 4)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-SingleMatrix-Elastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left - End]
                for (int iNode = 5; iNode <= 8; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 4; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 5; iNode <= 8; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 4; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }
                
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"C:\Users\tzwrt\Desktop\input files"; //"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=10-L_y=10-L_z=20-1x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=10-L_y=10-L_z=20-1x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
                    };

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"C:\Users\tzwrt\Desktop\input files"; //"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";

                    string CNTgeometryFileName = "nodesBeam.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"C:\Users\tzwrt\Desktop\input files"; //"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";

                    string CNTgeometryFileName = "nodesBeam.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    //var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(100.0, 1.0, 10.0, 0.500, new double[2], new double[2], 1e-3);

                    // Create Elastic 3D Material
                    var elasticMaterial = new ElasticMaterial3D_v2
                    {
                        YoungModulus = youngModulus,
                        PoissonRatio = poissonRatio,
                    };

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }

        public static class Run2a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 1;
            private const int hostNodes = 8;
            private const int embeddedElements = 1;
            private const int embeddedNodes = 2;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 2;
            private const DOFType monitorDof = DOFType.Y;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 7; iNode = iNode + 2)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 2; iNode <= 6; iNode = iNode + 4)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-SingleMatrix-Plastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 7; iNode = iNode + 2)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 2; iNode <= 6; iNode = iNode + 4)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 7; iNode = iNode + 2)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 2; iNode <= 6; iNode = iNode + 4)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2a-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }

                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-1x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-1x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2a\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);

                    // Create Elastic 3D Material
                    var elasticMaterial = new ElasticMaterial3D_v2
                    {
                        YoungModulus = youngModulus,
                        PoissonRatio = poissonRatio,
                    };

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }

        public static class Run2b_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 2;
            private const int hostNodes = 12;
            private const int embeddedElements = 1;
            private const int embeddedNodes = 2;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 3;
            private const DOFType monitorDof = DOFType.Y;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-SingleMatrix-Elastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }

                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
                    };

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);

                    // Create Elastic 3D Material
                    var elasticMaterial = new ElasticMaterial3D_v2
                    {
                        YoungModulus = youngModulus,
                        PoissonRatio = poissonRatio,
                    };

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }

        public static class Run2b_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 2;
            private const int hostNodes = 12;
            private const int embeddedElements = 1;
            private const int embeddedNodes = 2;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 3;
            private const DOFType monitorDof = DOFType.Y;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-SingleMatrix-Plastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2b-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }

                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2b\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);

                    // Create Elastic 3D Material
                    var elasticMaterial = new ElasticMaterial3D_v2
                    {
                        YoungModulus = youngModulus,
                        PoissonRatio = poissonRatio,
                    };

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }

        public static class Run2c_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 2;
            private const int hostNodes = 12;
            private const int embeddedElements = 2;
            private const int embeddedNodes = 3;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 3;
            private const DOFType monitorDof = DOFType.Y;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-SingleMatrix-Elastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }

                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
                    };

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);

                    // Create Elastic 3D Material
                    var elasticMaterial = new ElasticMaterial3D_v2
                    {
                        YoungModulus = youngModulus,
                        PoissonRatio = poissonRatio,
                    };

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }

        public static class Run2c_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 2;
            private const int hostNodes = 12;
            private const int embeddedElements = 2;
            private const int embeddedNodes = 3;
            private const double nodalLoad = +10.0; // +1000.0;//
            private const int monitorNode = 3;
            private const DOFType monitorDof = DOFType.Y;

            public static void SingleMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.SingleMatrixBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-SingleMatrix-plastic-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-Stochastic-CNT-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
                var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                    model.NodesDictionary[monitorNode], monitorDof, outputFile);
                childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

                // Run the analysis
                parentAnalyzer.Initialize();
                parentAnalyzer.Solve();
            }

            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 10; iNode = iNode + 3)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Loading Conditions - [Right-End] - {2 nodes}
                for (int iNode = 3; iNode <= 9; iNode = iNode + 6)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Y });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2c-Stochastic-CNT-Cohesive-Results.txt";
                string extension = Path.GetExtension(currentOutputFileName);
                string pathName = outputDirectory;
                string fileNameOnly = Path.Combine(pathName, Path.GetFileNameWithoutExtension(currentOutputFileName));
                string outputFile = string.Format("{0}_{1}{2}", fileNameOnly, noStochasticSimulation, extension);
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
                public static void SingleMatrixBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                }

                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-Geometry_MSolve.inp";
                    string MatrixConnectivityFileName = "MATRIX_3D-L_x=20-L_y=10-L_z=10-2x1x1-ConnMatr_MSolve.inp";
                    int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                    int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixConnectivityFileName).Count();

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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

                    // Generate elements
                    using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixConnectivityFileName))
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

                    // Create new Beam3D section and element
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);
                    // element nodes

                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
                    {
                        for (int i = 0; i < CNTElems; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int elementID = int.Parse(bits[0]) + hostElements; // matrixElements
                            int node1 = int.Parse(bits[1]) + hostNodes; // matrixNodes
                            int node2 = int.Parse(bits[2]) + hostNodes; // matrixNodes
                                                                        // element nodes
                            var elementNodes = new List<Node_v2>();
                            elementNodes.Add(model.NodesDictionary[node1]);
                            elementNodes.Add(model.NodesDictionary[node2]);
                            // create element
                            var beam_1 = new Beam3DCorotationalQuaternion_v2(elementNodes, solidMaterial, 7.85, beamSection);
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

                private static void CohesiveBeamElements_Stochastic(Model_v2 model, int noStochasticSimulation)
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 9\run-2c\input files";

                    string CNTgeometryFileName = "nodes.txt";
                    string CNTconnectivityFileName = "connectivity.txt";

                    string fileNameOnlyCNTgeometryFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTgeometryFileName));
                    string fileNameOnlyCNTconnectivityFileName = Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(CNTconnectivityFileName));
                    string extension = Path.GetExtension(CNTgeometryFileName);
                    string extension_2 = Path.GetExtension(CNTconnectivityFileName);

                    string currentCNTgeometryFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTgeometryFileName, noStochasticSimulation, extension);

                    //string currentCNTconnectivityFileName = string.Format("{0}_{1}{2}", fileNameOnlyCNTconnectivityFileName, noStochasticSimulation, extension_2);
                    string currentCNTconnectivityFileName = string.Format("{0}{1}", fileNameOnlyCNTconnectivityFileName, extension_2);

                    int CNTNodes = File.ReadLines(currentCNTgeometryFileName).Count();
                    int CNTElems = File.ReadLines(currentCNTconnectivityFileName).Count();

                    // Geometry
                    using (TextReader reader = File.OpenText(currentCNTgeometryFileName))
                    {
                        for (int i = 0; i < CNTNodes; i++)
                        {
                            string text = reader.ReadLine();
                            string[] bits = text.Split(',');
                            int nodeID = int.Parse(bits[0]) + (hostNodes + embeddedNodes); // matrixNodes
                            double nodeX = double.Parse(bits[1]);
                            double nodeY = double.Parse(bits[2]);
                            double nodeZ = double.Parse(bits[3]);
                            model.NodesDictionary.Add(nodeID, new Node_v2 { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                        }
                    }

                    // Create Cohesive Material
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.05, new double[2], new double[2], 1e-3);

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

                    // Create Beam3D Section
                    var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                    // element nodes
                    using (TextReader reader = File.OpenText(currentCNTconnectivityFileName))
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
                                    elementNodesClone, solidMaterial, 1, beamSection)
                            };
                            // Add beam element to the element and subdomains dictionary of the model
                            model.ElementsDictionary.Add(cohesiveElement.ID, cohesiveElement);
                            // Add Cohesive Element Nodes (!)
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2 - embeddedNodes]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node1]);
                            model.ElementsDictionary[cohesiveElement.ID].AddNode(model.NodesDictionary[node2]);
                            // Add Cohesive Element in Subdomain
                            model.SubdomainsDictionary[0].Elements.Add(cohesiveElement);
                        }
                    }
                }
            }
        }
    }
}
