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
    public static class StochasticEmbeddedExample_7
    {
        // Run-0
        public static class Run0_Elastic
        {            
            private const int subdomainID = 0;
            private const double nodalLoad = -50.0;
            private const double nodalDisplacement = -1.0;
            private const int monitorNode = 5;
            private const DOFType monitorDof = DOFType.Z;
            
            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\output files\elastic\NewtonRaphson";
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 4; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 5; iNode += 4)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 5; iNode <= 8; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static void PEEKMatrix_DisplacementControl(int noStochasticSimulation)
            {
                string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\output files\elastic\DisplacementControl";
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 4; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 5; iNode += 4)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
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

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                //var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                //{
                //    MaxIterationsPerIncrement = 10,
                //    NumIterationsForMatrixRebuild = 1,
                //    ResidualTolerance = 5E-03
                //};
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };
                var childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void ElasticPEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-1x1x1-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-1x1x1-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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
            }
        }
        
        public static class Run0_Plastic
        {            
            private const int subdomainID = 0;
            private const double nodalLoad = -50.0;
            private const double nodalDisplacement = -10.0;
            private const int monitorNode = 5;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\output files\plastic\NewtonRaphson";

                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 4; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 5; iNode += 4)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 5; iNode <= 8; iNode++)
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
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static void PEEKMatrix_DisplacementControl(int noStochasticSimulation)
            {
                string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\output files\plastic\DisplacementControl";

                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 1000;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 4; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 5; iNode += 4)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 5; iNode <= 8; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);
                
                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, increments)
                {
                    MaxIterationsPerIncrement = 100,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };
                var childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void ElasticPEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run0\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-1x1x1-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-1x1x1-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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
            }
        }

        // Run-1
        public static class Run1_2a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-2a\output files\elastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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
            
            public static void PEEKMatrix_DisplacementControl(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                double nodalDisplacement = -10.0;
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    //model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                //var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                //{
                //    MaxIterationsPerIncrement = 10,
                //    NumIterationsForMatrixRebuild = 1,
                //    ResidualTolerance = 5E-03
                //};
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, increments);
                var childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void ElasticPEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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
            }
        }

        public static class Run1_2a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-2a\output files\plastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void ElasticPEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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
            }
        }

        public static class Run1_3a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-3a\output files\elastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.ElasticPEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run1-3a-Matrix-Elastic-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void ElasticPEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-3a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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
            }
        }

        public static class Run1_3a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-3a\output files\plastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.PEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1-3a-Matrix-Plastic-Results.txt";
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

                public static void PEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-3a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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
            }
        }

        public static class Run1_4a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-4a\output files\elastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.PEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1-4a-Matrix-Elastic-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void PEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-4a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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
            }
        }

        public static class Run1_4a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-4a\output files\plastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.PEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1-4a-Matrix-Plastic-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void PEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-4a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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
            }
        }

        public static class Run1_5a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-5a\output files\elastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -4.0;
            private const int monitorNode = 8821;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.PEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run1-5a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void PEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-5a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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
            }
        }

        public static class Run1_5a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run1-5a\output files\plastic";
            private const int subdomainID = 0;
            private const double nodalLoad = -4.0;
            private const int monitorNode = 8821;
            private const DOFType monitorDof = DOFType.Z;

            public static void PEEKMatrix_NewtonRaphson(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.PEEKMatrixBuilder(model);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run5a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void PEEKMatrixBuilder(Model_v2 model)
                {
                    HostElements(model);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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
            }
        }


        // Run-2
        public static class Run2a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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

            public static void EBEembeddedInMatrix_DisplacementControl(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Applied displacement
                double nodalDisplacement = -10.0;
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                //var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                //{
                //    MaxIterationsPerIncrement = 10,
                //    NumIterationsForMatrixRebuild = 1,
                //    ResidualTolerance = 5E-03
                //};
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                int numIncrements = 10;
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, numIncrements);
                var childAnalyzer = childAnalyzerBuilder.Build();

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

            public static void EBEembeddedInMatrixCohesive_DisplacementControl(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Applied displacement
                double nodalDisplacement = -10.0;
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                //var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                //{
                //    MaxIterationsPerIncrement = 10,
                //    NumIterationsForMatrixRebuild = 1,
                //    ResidualTolerance = 5E-03
                //};
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                int numIncrements = 10;
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, numIncrements);
                var childAnalyzer = childAnalyzerBuilder.Build();

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

            public static class EBEEmbeddedModelBuilder
            {
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";

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

        public static class Run2a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2a\input files";

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
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.10, new double[2], new double[2], 1e-3);

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
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.10, new double[2], new double[2], 1e-3);

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
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.20, new double[2], new double[2], 1e-3);

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
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.20, new double[2], new double[2], 1e-3);

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

        public static class Run2d_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run2d-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run2d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Elastic Material
                    var solidMaterial = new ElasticMaterial3D_v2()
                    {
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.50, new double[2], new double[2], 1e-3);

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

        public static class Run2d_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 125;
            private const int hostNodes = 216;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -50.0;
            private const int monitorNode = 181;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2d-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 36; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 181; iNode += 36)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End] - {36 nodes}
                for (int iNode = 181; iNode <= 216; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run2d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-5x5x5-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-2d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.50, new double[2], new double[2], 1e-3);

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


        // Run-3
        public static class Run3a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run3a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                string currentOutputFileName = "Run3a-Stochastic-CNT-Cohesive-Results.txt";
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
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";

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

        public static class Run3a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3a-Stochastic-CNT-Cohesive-Results.txt";
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
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3a\input files";

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

        public static class Run3b_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3b-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.04, new double[2], new double[2], 1e-3);

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

        public static class Run3b_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3b-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.04, new double[2], new double[2], 1e-3);

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

        public static class Run3c_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3c-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.03, new double[2], new double[2], 1e-3);

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

        public static class Run3c_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3c-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.03, new double[2], new double[2], 1e-3);

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

        public static class Run3d_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.02, new double[2], new double[2], 1e-3);

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

        public static class Run3d_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 1000;
            private const int hostNodes = 1331;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -15.0;
            private const int monitorNode = 1211;
            private const DOFType monitorDof = DOFType.Z;


            public static void EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 121; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 1211; iNode += 121)
                {
                    for (int j = 0; j <= 10; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 1211; iNode <= 1331; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run3d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-10x10x10-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-3d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.02, new double[2], new double[2], 1e-3);

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


        // Run-4                                            
        public static class Run4a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Cohesive-Results.txt";
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

            public static void EBEembeddedInMatrix_DisplacementControl(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Applied displacement
                double nodalDisplacement = -10.0;
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                // var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                // {
                //     MaxIterationsPerIncrement = 10,
                //     NumIterationsForMatrixRebuild = 1,
                //     ResidualTolerance = 5E-03
                // };
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                int numIncrements = 10;
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, numIncrements);
                var childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Results.txt";
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

            public static void EBEembeddedInMatrixCohesive_DisplacementControl(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Applied displacement
                double nodalDisplacement = -10.0;
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z, Amount = nodalDisplacement });
                }

                // Choose linear equation system solver
                //var solverBuilder = new SkylineSolver.Builder();
                //SkylineSolver solver = solverBuilder.BuildSolver(model);
                var solverBuilder = new SuiteSparseSolver.Builder();
                SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

                // Choose the provider of the problem -> here a structural problem
                var provider = new ProblemStructural_v2(model, solver);

                // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer     
                // var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments)
                // {
                //     MaxIterationsPerIncrement = 10,
                //     NumIterationsForMatrixRebuild = 1,
                //     ResidualTolerance = 5E-03
                // };
                //LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose child analyzer -> Child: DisplacementControlAnalyzer 
                var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
                int numIncrements = 10;
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider, numIncrements);
                var childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Results.txt";
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

            public static class EBEEmbeddedModelBuilder
            {
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";

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

        public static class Run4a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4a-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4a\input files";

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

        public static class Run4b_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4b-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4b-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.10, new double[2], new double[2], 1e-3);

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

        public static class Run4b_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4b-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4b-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4b\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.10, new double[2], new double[2], 1e-3);

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

        public static class Run4c_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4c-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4c-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.20, new double[2], new double[2], 1e-3);

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

        public static class Run4c_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4c-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4c-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4c\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.20, new double[2], new double[2], 1e-3);

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

        public static class Run4d_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4d-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.50, new double[2], new double[2], 1e-3);

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

        public static class Run4d_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 3375;
            private const int hostNodes = 4096;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -7.0;
            private const int monitorNode = 3841;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4d-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 256; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 3841; iNode += 256)
                {
                    for (int j = 0; j <= 15; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 3841; iNode <= 4096; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run4d-Stochastic-CNT-Cohesive-Results.txt";
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
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-15x15x15-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-4d\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.0, 1.0, 10.0, 0.50, new double[2], new double[2], 1e-3);

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


        // Run-5
        public static class Run5a_Elastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\output files\elastic";
            private const int subdomainID = 0;
            private const int hostElements = 8000;
            private const int hostNodes = 9261;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -4.0;
            private const int monitorNode = 8821;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run5a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run5a-Stochastic-CNT-Cohesive-Results.txt";
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
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-ConnMatr_MSolve.inp";
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
                        YoungModulus = 4.00,
                        PoissonRatio = 0.40,
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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.00, 1.00, 10.00, 0.05, new double[2], new double[2], 1e-3);

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

        public static class Run5a_Plastic
        {
            private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\output files\plastic";
            private const int subdomainID = 0;
            private const int hostElements = 8000;
            private const int hostNodes = 9261;
            private const int embeddedElements = 300;
            private const int embeddedNodes = 330;
            private const double nodalLoad = -4.0;
            private const int monitorNode = 8821;
            private const DOFType monitorDof = DOFType.Z;

            public static void EBEembeddedInMatrix_NewtonRaphson_Stochastic(int noStochasticSimulation)
            {
                VectorExtensions.AssignTotalAffinityCount();

                // No. of increments
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.FullyBondedEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run5a-Stochastic-CNT-Results.txt";
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
                int increments = 100;

                // Model creation
                var model = new Model_v2();

                // Subdomains
                //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
                model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

                // Choose model
                EBEEmbeddedModelBuilder.CohesiveEmbeddedBuilder_Stochastic(model, noStochasticSimulation);

                // Boundary Conditions - [Left-End]
                for (int iNode = 1; iNode <= 441; iNode++)
                {
                    model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
                }

                // Boundary Conditions - [Bottom-End]
                for (int iNode = 1; iNode <= 8821; iNode += 441)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        model.NodesDictionary[iNode + j].Constraints.Add(new Constraint { DOF = DOFType.Y });
                    }
                }

                // Loading Conditions - [Right-End]
                for (int iNode = 8821; iNode <= 9261; iNode++)
                {
                    model.Loads.Add(new Load_v2() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
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
                    MaxIterationsPerIncrement = 10,
                    NumIterationsForMatrixRebuild = 1,
                    ResidualTolerance = 5E-03
                };

                LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

                // Choose parent analyzer -> Parent: Static
                var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

                // Request output
                string currentOutputFileName = "Run5a-Stochastic-CNT-Cohesive-Results.txt";
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
                public static void FullyBondedEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateFullyBonded(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > hostElements).Select(kv => kv.Value), true);
                }

                public static void CohesiveEmbeddedBuilder_Stochastic(Model_v2 model, int i)
                {
                    HostElements(model);
                    EmbeddedElements_Stochastic(model, i);
                    CohesiveBeamElements_Stochastic(model, i);
                    //var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                    var embeddedGrouping = EmbeddedBeam3DGrouping.CreateCohesive(model, model.ElementsDictionary.Where(x => x.Key <= hostElements).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > (hostElements + embeddedElements)).Select(kv => kv.Value), true);
                }

                private static void HostElements(Model_v2 model)
                {
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";
                    string MatrixGeometryFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-Geometry_MSolve.inp";
                    string MatrixGonnectivityFileName = "MATRIX_3D-L_x=50-L_y=50-L_z=50-20x20x20-ConnMatr_MSolve.inp";
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

                    // Create Plastic Material
                    var solidMaterial = new VonMisesMaterial3D_v2(4.0, 0.4, 0.120, 0.1);

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

                private static void EmbeddedElements_Stochastic(Model_v2 model, int noStochasticSimulation)
                {
                    // define mechanical properties
                    double youngModulus = 1.0; // 5490; // 
                    double shearModulus = 1.0; // 871; // 
                    double poissonRatio = (youngModulus / (2 * shearModulus)) - 1; //2.15; // 0.034;
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";

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
                    double area = 694.77; // 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                    double inertiaY = 100.18; //1058.55;
                    double inertiaZ = 100.18; //1058.55;1058.55;
                    double torsionalInertia = 68.77; //496.38;
                    double effectiveAreaY = area;
                    double effectiveAreaZ = area;
                    string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\Stochastic Embedded Example 7\run-5a\input files";

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
                    var cohesiveMaterial = new BondSlipCohMatUniaxial(10.00, 1.00, 10.00, 0.05, new double[2], new double[2], 1e-3);

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
    }
}