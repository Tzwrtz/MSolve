using System;
using System.Collections.Generic;
using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.Dynamic;
using ISAAR.MSolve.Discretization;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.Materials.Interfaces;
using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
//using ISAAR.MSolve.SamplesConsole.DdmBenchmarks1;
using ISAAR.MSolve.SamplesConsole.Solvers;
using ISAAR.MSolve.Solvers;
using ISAAR.MSolve.Solvers.Direct;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Skyline;
using ISAAR.MSolve.Tests.FEMpartB;

namespace ISAAR.MSolve.SamplesConsole
{
    class Program
    {
        private const int subdomainID = 0;

        static void Main(string[] args)
        {
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-15 // - 1 straight CNT in Matrix(10*10*10)-nelems=[1x1x1]
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //// Run_2-SingleMatrix
            //// Vf = 6.1525 %
            //StochasticEmbeddedExample_15.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_15.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //// Run_3-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_15.Run3a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_15.Run3a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //    // ** Run_2 ** //

            //    //    // Run_2a-Elastic
            //    //StochasticEmbeddedExample_15.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_15.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    // Run_2a-Plastic
            //    //    //StochasticEmbeddedExample_15.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //    //StochasticEmbeddedExample_15.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //    //// Run_2b-Elastic
            //    //    //StochasticEmbeddedExample_15.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    //// Run_2b-Plastic               
            //    //    //StochasticEmbeddedExample_15.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //    //// Run_2c-Elastic
            //    //    //StochasticEmbeddedExample_15.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    //// Run_2b-Plastic
            //    //    //StochasticEmbeddedExample_15.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    //    // ** Run_3 ** //

            //    //    // Run_3a-Elastic
            //    //    StochasticEmbeddedExample_15.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //    StochasticEmbeddedExample_15.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    //// Run_3a-Plastic
            //    //    //StochasticEmbeddedExample_15.Run3a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //    //StochasticEmbeddedExample_15.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //    //// Run_3b-Elastic
            //    //    //StochasticEmbeddedExample_15.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    //// Run_3b-Plastic               
            //    //    //StochasticEmbeddedExample_15.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //    //// Run_3c-Elastic
            //    //    //StochasticEmbeddedExample_15.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //    //// Run_3b-Plastic
            //    //    //StochasticEmbeddedExample_15.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-16 // Vf = 1.5%
            //*******************//
            LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl_AlongX(1);
            StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl_AlongY(1);
            StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl_AlongZ(1);
            //StochasticEmbeddedExample_17.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //StochasticEmbeddedExample_23.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //StochasticEmbeddedExample_23.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //StochasticEmbeddedExample_24.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //StochasticEmbeddedExample_24.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            ////int startingNumofSimulations = 1;
            ////int numberOfSimulations = 10;

            ////Run_2 - SingleMatrix
            //// Vf = 1.5 % -HostElemets = 1000
            //StochasticEmbeddedExample_16.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_16.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    StochasticEmbeddedExample_16.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_16.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_16.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2b-Elastic
            //    StochasticEmbeddedExample_16.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic               
            //    //StochasticEmbeddedExample_16.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2c-Elastic
            //    StochasticEmbeddedExample_16.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic
            //    //StochasticEmbeddedExample_16.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2d-Elastic
            //    StochasticEmbeddedExample_16.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2d-Plastic
            //    //StochasticEmbeddedExample_16.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    //StochasticEmbeddedExample_16.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3a-Plastic
            //    //StochasticEmbeddedExample_16.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3b-Elastic
            //    //StochasticEmbeddedExample_16.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3b-Plastic               
            //    //StochasticEmbeddedExample_16.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3c-Elastic
            //    //StochasticEmbeddedExample_16.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3c-Plastic
            //    //StochasticEmbeddedExample_16.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3d-Elastic
            //    //StochasticEmbeddedExample_16.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3d-Plastic
            //    //StochasticEmbeddedExample_16.Run3d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_4 ** //

            //    // Run_4a-Elastic
            //    //StochasticEmbeddedExample_16.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4a-Plastic
            //    //StochasticEmbeddedExample_16.Run4a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_4b-Elastic
            //    //StochasticEmbeddedExample_16.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4b-Plastic               
            //    //StochasticEmbeddedExample_16.Run4b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_4c-Elastic
            //    //StochasticEmbeddedExample_16.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4b-Plastic
            //    //StochasticEmbeddedExample_16.Run4c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //Console.WriteLine($" ");
            //Console.WriteLine($"*** EXAMPLE 16 ***");
            //Console.WriteLine($" ");

            //var watch_1 = new System.Diagnostics.Stopwatch();
            //watch_1.Start();
            //StochasticEmbeddedExample_16.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_1.Stop();
            //var elapsedTime_1 = watch_1.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_1: {elapsedTime_1} ms");

            //var watch_2 = new System.Diagnostics.Stopwatch();
            //watch_2.Start();
            //StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_2.Stop();
            //var elapsedTime_2 = watch_2.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_2: {elapsedTime_2} ms");

            //var watch_3 = new System.Diagnostics.Stopwatch();
            //watch_3.Start();
            //StochasticEmbeddedExample_16.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_3.Stop();
            //var elapsedTime_3 = watch_3.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_3: {elapsedTime_3} ms");

            //var watch_4 = new System.Diagnostics.Stopwatch();
            //watch_4.Start();
            //StochasticEmbeddedExample_16.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_4.Stop();
            //var elapsedTime_4 = watch_4.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_4: {elapsedTime_4} ms");

            //var watch_5 = new System.Diagnostics.Stopwatch();
            //watch_5.Start();
            //StochasticEmbeddedExample_16.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_5.Stop();
            //var elapsedTime_5 = watch_5.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_5: {elapsedTime_5} ms");

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-17 // Vf = 4.0%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            ////Run_2 - SingleMatrix
            ////StochasticEmbeddedExample_17.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_17.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //**Run_2 * * //

            //    //Run_2a - Elastic
            //    StochasticEmbeddedExample_17.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_17.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2a - Plastic
            //    //StochasticEmbeddedExample_17.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_17.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2b - Elastic
            //    StochasticEmbeddedExample_17.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2b - Plastic
            //    //StochasticEmbeddedExample_17.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2c - Elastic
            //    StochasticEmbeddedExample_17.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2b - Plastic
            //    //StochasticEmbeddedExample_17.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2d - Elastic
            //    StochasticEmbeddedExample_17.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2d - Plastic
            //    //StochasticEmbeddedExample_17.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    //**Run_3 * * //

            //    //Run_3a - Elastic
            //    StochasticEmbeddedExample_17.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3a - Plastic
            //    //StochasticEmbeddedExample_17.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3b - Elastic
            //    StochasticEmbeddedExample_17.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3b - Plastic
            //    //StochasticEmbeddedExample_17.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3c - Elastic
            //    StochasticEmbeddedExample_17.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3b - Plastic
            //    //StochasticEmbeddedExample_17.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3d - Elastic
            //    //StochasticEmbeddedExample_17.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3d - Plastic
            //    //StochasticEmbeddedExample_17.Run3d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    //**Run_4 * * //

            //    ////Run_4a - Elastic
            //    //StochasticEmbeddedExample_17.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4a - Plastic
            //    ////StochasticEmbeddedExample_17.Run4a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    ////Run_4b - Elastic
            //    //StochasticEmbeddedExample_17.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4b - Plastic
            //    ////StochasticEmbeddedExample_17.Run4b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    ////Run_4c - Elastic
            //    //StochasticEmbeddedExample_17.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4b - Plastic
            //    ////StochasticEmbeddedExample_17.Run4c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //Console.WriteLine($" ");
            //Console.WriteLine($"*** EXAMPLE 17 ***");
            //Console.WriteLine($" ");

            //var watch_6 = new System.Diagnostics.Stopwatch();
            //watch_6.Start();
            //StochasticEmbeddedExample_17.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_6.Stop();
            //var elapsedTime_6 = watch_6.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_6: {elapsedTime_6} ms");

            //var watch_7 = new System.Diagnostics.Stopwatch();
            //watch_7.Start();
            //StochasticEmbeddedExample_17.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_7.Stop();
            //var elapsedTime_7 = watch_7.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_7: {elapsedTime_7} ms");

            //var watch_8 = new System.Diagnostics.Stopwatch();
            //watch_8.Start();
            //StochasticEmbeddedExample_17.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_8.Stop();
            //var elapsedTime_8 = watch_8.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_8: {elapsedTime_8} ms");

            //var watch_9 = new System.Diagnostics.Stopwatch();
            //watch_9.Start();
            //StochasticEmbeddedExample_17.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_9.Stop();
            //var elapsedTime_9 = watch_9.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_9: {elapsedTime_9} ms");

            //var watch_10 = new System.Diagnostics.Stopwatch();
            //watch_10.Start();
            //StochasticEmbeddedExample_17.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_10.Stop();
            //var elapsedTime_10 = watch_10.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_10: {elapsedTime_10} ms");


            //// Print Results for examples 16 & 17
            //using (var fileName = new System.IO.StreamWriter(@"E:\GEORGE_DATA\DESKTOP\phd\EmbeddedExamples\ElapsedTimesExamples16and17.txt", true))
            //{
            //    string strElapsedTime_1 = elapsedTime_1.ToString();
            //    string strElapsedTime_2 = elapsedTime_2.ToString();
            //    string strElapsedTime_3 = elapsedTime_3.ToString();
            //    string strElapsedTime_4 = elapsedTime_4.ToString();
            //    string strElapsedTime_5 = elapsedTime_5.ToString();
            //    string strElapsedTime_6 = elapsedTime_6.ToString();
            //    string strElapsedTime_7 = elapsedTime_7.ToString();
            //    string strElapsedTime_8 = elapsedTime_8.ToString();
            //    string strElapsedTime_9 = elapsedTime_9.ToString();
            //    string strElapsedTime_10 = elapsedTime_10.ToString();
            //    fileName.WriteLine("Execution Time watch_1: " + elapsedTime_1 + " ms");
            //    fileName.WriteLine("Execution Time watch_2: " + elapsedTime_2 + " ms");
            //    fileName.WriteLine("Execution Time watch_3: " + elapsedTime_3 + " ms");
            //    fileName.WriteLine("Execution Time watch_4: " + elapsedTime_4 + " ms");
            //    fileName.WriteLine("Execution Time watch_5: " + elapsedTime_5 + " ms");
            //    fileName.WriteLine("Execution Time watch_6: " + elapsedTime_6 + " ms");
            //    fileName.WriteLine("Execution Time watch_7: " + elapsedTime_7 + " ms");
            //    fileName.WriteLine("Execution Time watch_8: " + elapsedTime_8 + " ms");
            //    fileName.WriteLine("Execution Time watch_9: " + elapsedTime_9 + " ms");
            //    fileName.WriteLine("Execution Time watch_10: " + elapsedTime_10 + " ms");
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-20 // Convergence Study - Vf=1.5% - Wavy CNTs of Example_23
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumOfSimulations = 1;
            //int numberOfSimulations = 1;

            ////StochasticEmbeddedExample_20.Run3a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_20.Run4a_Elastic.SingleMatrix_DisplacementControl();

            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            //for (int i = startingNumOfSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //
            //    // Run_2a-Elastic
            //    //StochasticEmbeddedExample_20.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // ** Run_3 ** //
            //    // Run_3a-Elastic - nElems = 20*20*20 = 8000             
            //    StochasticEmbeddedExample_20.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // ** Run_4 ** //
            //    // Run_4a-Elastic - nElems = 5*5*5 = 125                
            //    //StochasticEmbeddedExample_20.Run4a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}
            //watch.Stop();
            //var elapsedTime = watch.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time: {elapsedTime} ms");

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-23 // Wavy CNTs- Vf=1.5% - Spectral Representation
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            //// //Single Matrix analysis
            ////StochasticEmbeddedExample_23.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_23.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ELASTIC MATRIX
            //    //StochasticEmbeddedExample_23.Run2a_Elastic.FullyBonded_DisplacementControl(i);
            //    StochasticEmbeddedExample_23.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //StochasticEmbeddedExample_23.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //StochasticEmbeddedExample_23.Run4a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // PLASTIC MATRIX
            //    //StochasticEmbeddedExample_23.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //StochasticEmbeddedExample_23.Run3a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run3d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //StochasticEmbeddedExample_23.Run4a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_23.Run4d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            // Elapsed Time for each analysis
            //***Run-2a***//
            //var watch_1 = new System.Diagnostics.Stopwatch();
            //watch_1.Start();
            //StochasticEmbeddedExample_23.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_1.Stop();
            //var elapsedTime_1 = watch_1.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_1: {elapsedTime_1} ms");

            //var watch_2 = new System.Diagnostics.Stopwatch();
            //watch_2.Start();
            //StochasticEmbeddedExample_23.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_2.Stop();
            //var elapsedTime_2 = watch_2.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_2: {elapsedTime_2} ms");

            //var watch_3 = new System.Diagnostics.Stopwatch();
            //watch_3.Start();
            //StochasticEmbeddedExample_23.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_3.Stop();
            //var elapsedTime_3 = watch_3.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_3: {elapsedTime_3} ms");

            //var watch_4 = new System.Diagnostics.Stopwatch();
            //watch_4.Start();
            //StochasticEmbeddedExample_23.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_4.Stop();
            //var elapsedTime_4 = watch_4.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_4: {elapsedTime_4} ms");

            //var watch_5 = new System.Diagnostics.Stopwatch();
            //watch_5.Start();
            //StochasticEmbeddedExample_23.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_5.Stop();
            //var elapsedTime_5 = watch_5.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_5: {elapsedTime_5} ms");

            ////***Run-3a***//
            //var watch_6 = new System.Diagnostics.Stopwatch();
            //watch_6.Start();
            //StochasticEmbeddedExample_23.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_6.Stop();
            //var elapsedTime_6 = watch_6.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_6: {elapsedTime_6} ms");

            //var watch_7 = new System.Diagnostics.Stopwatch();
            //watch_7.Start();
            //StochasticEmbeddedExample_23.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_7.Stop();
            //var elapsedTime_7 = watch_7.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_7: {elapsedTime_7} ms");

            //var watch_8 = new System.Diagnostics.Stopwatch();
            //watch_8.Start();
            //StochasticEmbeddedExample_23.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_8.Stop();
            //var elapsedTime_8 = watch_8.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_8: {elapsedTime_8} ms");

            //var watch_9 = new System.Diagnostics.Stopwatch();
            //watch_9.Start();
            //StochasticEmbeddedExample_23.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_9.Stop();
            //var elapsedTime_9 = watch_9.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_9: {elapsedTime_9} ms");

            //var watch_10 = new System.Diagnostics.Stopwatch();
            //watch_10.Start();
            //StochasticEmbeddedExample_23.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_10.Stop();
            //var elapsedTime_10 = watch_10.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_10: {elapsedTime_10} ms");


            ////**************************************************************************************************************************************************************************************//
            ////**************************************************************************************************************************************************************************************//

            ////********************//
            //// EmbeddedExample-24 // Wavy CNTs- Vf=4% - Spectral Representation
            ////*******************//
            ////LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            ////int startingNumofSimulations = 1;
            ////int numberOfSimulations = 1;

            //for (int i = 1; i <= 10; i++)
            //{
            //    StochasticEmbeddedExample_24.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_24.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    StochasticEmbeddedExample_24.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_24.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    StochasticEmbeddedExample_24.Run4a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_24.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_24.Run4d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            // Elapsed Time for each analysis
            //***Run-2a***//
            //Console.WriteLine($" ");
            //Console.WriteLine($"*** EXAMPLE 24 ***");
            //Console.WriteLine($" ");

            //var watch_11 = new System.Diagnostics.Stopwatch();
            //watch_11.Start();
            //StochasticEmbeddedExample_24.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_11.Stop();
            //var elapsedTime_11 = watch_11.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_11: {elapsedTime_11} ms");

            //var watch_12 = new System.Diagnostics.Stopwatch();
            //watch_12.Start();
            //StochasticEmbeddedExample_24.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_12.Stop();
            //var elapsedTime_12 = watch_12.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_12: {elapsedTime_12} ms");

            //var watch_13 = new System.Diagnostics.Stopwatch();
            //watch_13.Start();
            //StochasticEmbeddedExample_24.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_13.Stop();
            //var elapsedTime_13 = watch_13.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_13: {elapsedTime_13} ms");

            //var watch_14 = new System.Diagnostics.Stopwatch();
            //watch_14.Start();
            //StochasticEmbeddedExample_24.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_14.Stop();
            //var elapsedTime_14 = watch_14.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_14: {elapsedTime_14} ms");

            //var watch_15 = new System.Diagnostics.Stopwatch();
            //watch_15.Start();
            //StochasticEmbeddedExample_24.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_15.Stop();
            //var elapsedTime_15 = watch_15.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_15: {elapsedTime_15} ms");

            ////***Run-3a***//
            //var watch_16 = new System.Diagnostics.Stopwatch();
            //watch_16.Start();
            //StochasticEmbeddedExample_24.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(1);
            //watch_16.Stop();
            //var elapsedTime_16 = watch_16.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_16: {elapsedTime_16} ms");

            //var watch_17 = new System.Diagnostics.Stopwatch();
            //watch_17.Start();
            //StochasticEmbeddedExample_24.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_17.Stop();
            //var elapsedTime_17 = watch_17.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_17: {elapsedTime_17} ms");

            //var watch_18 = new System.Diagnostics.Stopwatch();
            //watch_18.Start();
            //StochasticEmbeddedExample_24.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_18.Stop();
            //var elapsedTime_18 = watch_18.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_18: {elapsedTime_18} ms");

            //var watch_19 = new System.Diagnostics.Stopwatch();
            //watch_19.Start();
            //StochasticEmbeddedExample_24.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_19.Stop();
            //var elapsedTime_19 = watch_19.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_19: {elapsedTime_19} ms");

            //var watch_20 = new System.Diagnostics.Stopwatch();
            //watch_20.Start();
            //StochasticEmbeddedExample_24.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(1);
            //watch_20.Stop();
            //var elapsedTime_20 = watch_20.ElapsedMilliseconds;
            //Console.WriteLine($"Execution Time watch_20: {elapsedTime_20} ms");

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-25 // RVE with 1 wavy CNT - Vf=1.5% - Matrix(10,10,50)-L_cnt=50nm
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //StochasticEmbeddedExample_25.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    StochasticEmbeddedExample_25.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_25.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-26 // RVE with 1 straight CNT - Vf=1.5% - Matrix(10,10,50)-L_cnt=50nm
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //StochasticEmbeddedExample_26.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    StochasticEmbeddedExample_26.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_26.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-27 // RVE: Matrix(50,50,50), L_cnt=50 - Vf=1.5%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            ////StochasticEmbeddedExample_27.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_27.Run2a_Plastic.SingleMatrix_DisplacementControl();
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //StochasticEmbeddedExample_27.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_27.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    StochasticEmbeddedExample_27.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_27.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-28 // RVE: Matrix(200,200,200), L_cnt=100
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            ////StochasticEmbeddedExample_28.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_28.Run2a_Plastic.SingleMatrix_DisplacementControl();
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //StochasticEmbeddedExample_28.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_28.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    StochasticEmbeddedExample_28.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_28.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

        }
    }
}
