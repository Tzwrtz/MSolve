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
            //*******************//
            // EmbeddedExample-3 //
            //*******************//
            //Stochastic Analysis - StochasticEmbeddedExample_1
            //for (int i = 1; i <= 10000; i++)
            //{
            //    //StochasticEmbeddedExample_1.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_1.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //Stochastic Analysis - StochasticEmbeddedExample_3_Run-2-Vf=0.72% - Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    //StochasticEmbeddedExample_3.Run2_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_3.Run2_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    //StochasticEmbeddedExample_3.Run2_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_3.Run2_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //Stochastic Analysis - StochasticEmbeddedExample_3_Run-3-Vf=4.76% - Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    //StochasticEmbeddedExample_3.Run3_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_3.Run3_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_3.Run3_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_3.Run3_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //Stochastic Analysis - StochasticEmbeddedExample_3_Run-4-Vf=11.38% - Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 34; i <= 50; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_3.Run4_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_3.Run4_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_3.Run4_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_3.Run4_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//



            //*******************//
            // EmbeddedExample-4 //
            //*******************//

            // // Stochastic Analysis -StochasticEmbeddedExample_4_Run-2 - Vf = 0.72 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------Run2a_Elastic-----------------//
            //    StochasticEmbeddedExample_4.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_4.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2a_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_4.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2b_Plastic-----------------//                
            //    //StochasticEmbeddedExample_4.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2c_Elastic-----------------//
            //    StochasticEmbeddedExample_4.Run2c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2c_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run2c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2d_Elastic-----------------//
            //    StochasticEmbeddedExample_4.Run2d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2d_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run2d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}


            //// // Stochastic Analysis -StochasticEmbeddedExample_4_Run-3 - Vf = 2.65 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------Run3a_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3a_Plastic-----------------//
            //    StochasticEmbeddedExample_4.Run3a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3b_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3b_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3b_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3b_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3c_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3c_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3c_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3c_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3d_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3d_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3d_Plastic-----------------//
            //    //StochasticEmbeddedExample_4.Run3d_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run3d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}


            //// Stochastic Analysis -StochasticEmbeddedExample_4_Run-4 - Vf = 4.76 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 44; i <= 100; i++)
            //{
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_4.Run4a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run4a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    ////-----------------Run4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_4.Run4a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    ////-----------------Run4b_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    ////-----------------Run4b_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    ////-----------------Run4c_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    ////-----------------Run4c_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    ////-----------------Run4d_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    ////-----------------Run4d_Elastic-----------------//
            //    //StochasticEmbeddedExample_4.Run4d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            // // Stochastic Analysis -StochasticEmbeddedExample_4_Run-5 - Vf = 11.38 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 50; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_4.Run5a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_4.Run5a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_4.Run5a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_4.Run5a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//


            //*******************//
            // EmbeddedExample-5 //
            //*******************//
            //Stochastic Analysis -StochasticEmbeddedExample_5_Run - 2 - Vf = 2.65 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_5.Run2a_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_5.Run2a_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_5.Run2b_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2b_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_5.Run2b_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2b_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_5.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //*******************//
            // EmbeddedExample-6 //
            //*******************//
            // Stochastic Analysis -StochasticEmbeddedExample_6_Run-2 - Vf = 0.72 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------Run2a_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2a_Plastic-----------------//
            //    StochasticEmbeddedExample_6.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2b_Plastic-----------------//                
            //    StochasticEmbeddedExample_6.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2c_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run2c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2c_Plastic-----------------//
            //    StochasticEmbeddedExample_6.Run2c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2d_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run2d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2d_Plastic-----------------//
            //    StochasticEmbeddedExample_6.Run2d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //// Stochastic Analysis -StochasticEmbeddedExample_6_Run-3 - Vf = 2.65 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 10; i++)
            //{
            //    //-----------------Run3a_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run3a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run3a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3a_Plastic-----------------//
            //    StochasticEmbeddedExample_6.Run3a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run3a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3b_Elastic-----------------//                
            //    StochasticEmbeddedExample_6.Run3b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3b_Plastic-----------------//                
            //    StochasticEmbeddedExample_6.Run3b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3c_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run3c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3c_Plastic-----------------//                
            //    StochasticEmbeddedExample_6.Run3c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3d_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run3d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3d_Plastic-----------------//
            //    StochasticEmbeddedExample_6.Run3d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //// Stochastic Analysis -StochasticEmbeddedExample_6_Run-4 - Vf = 4.76 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 100; i++)
            //{
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run4a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run4a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4b_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4b_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4c_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4c_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4d_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4d_Elastic-----------------//
            //    StochasticEmbeddedExample_6.Run4d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //// Stochastic Analysis -StochasticEmbeddedExample_6_Run-5 - Vf = 11.38 % -Elastic matrix
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //for (int i = 1; i <= 50; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_6.Run5a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run5a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_6.Run5a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_6.Run5a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//


            //*******************//
            // EmbeddedExample-7 //
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            //***RUN-0_NEWTON RAPHSON***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run0_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run0_Elastic.PEEKMatrix_NewtonRaphson(i);
            //    //-----------------Run0_Plastic-----------------//
            //    StochasticEmbeddedExample_7.Run0_Plastic.PEEKMatrix_NewtonRaphson(i);
            //}

            // //***RUN - 0_DISPLACEMENT CONTROL***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run0_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run0_Elastic.PEEKMatrix_DisplacementControl(i);
            //    //-----------------Run0_Plastic-----------------//
            //    StochasticEmbeddedExample_7.Run0_Plastic.PEEKMatrix_DisplacementControl(i);
            //}

            //***RUN-1_NEWTON RAPHSON***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //    //-----------------Run1-2a_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run1_2a_Elastic.PEEKMatrix_NewtonRaphson(i);
            //    //    //-----------------Run1-2a_Plastic-----------------//
            //    StochasticEmbeddedExample_7.Run1_2a_Plastic.PEEKMatrix_NewtonRaphson(i);

            //    //    //-----------------Run1-3a_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run1_3a_Elastic.PEEKMatrix_NewtonRaphson(i);
            //    //    //-----------------Run1-3a_Plastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run1_3a_Plastic.PEEKMatrix_NewtonRaphson(i);

            //    //    //-----------------Run1-4a_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run1_4a_Elastic.PEEKMatrix_NewtonRaphson(i);
            //    //    //-----------------Run1-4a_Plastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run1_4a_Plastic.PEEKMatrix_NewtonRaphson(i);

            //    //    //-----------------Run1-5a_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run1_5a_Elastic.PEEKMatrix_NewtonRaphson(i);
            //    //    //-----------------Run1 - 5a_Plastic---------------- -//
            //    //    StochasticEmbeddedExample_7.Run1_5a_Plastic.PEEKMatrix_NewtonRaphson(i);
            //}

            //***RUN-1_DISPLACEMENT CONTROL***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run1-2a_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run1_2a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    //-----------------Run1-2a_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_2a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-3a_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_3a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1-3a_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_3a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_4a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1-4a_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_4a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-5a_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run1_5a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1 - 5a_Plastic---------------- -//
            //    //StochasticEmbeddedExample_7.Run1_5a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    //***RUN-2_NEWTON RAPHSON***//
            //    // Stochastic Analysis -StochasticEmbeddedExample_7_Run - 2 - Vf = 2.83 % -HostElemets = 125
            //    //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //    //{
            //    //    //-----------------Run2a_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //    StochasticEmbeddedExample_7.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //    //-----------------Run2a_Plastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //    StochasticEmbeddedExample_7.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //    //-----------------Run2b_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //    //-----------------Run2b_Plastic-----------------//                
            //    //    StochasticEmbeddedExample_7.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //    //-----------------Run2c_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //    //-----------------Run2c_Plastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //    //-----------------Run2d_Elastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //    //-----------------Run2d_Plastic-----------------//
            //    //    StochasticEmbeddedExample_7.Run2d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //***RUN-2_DISPLACEMENT CONTROL***//
            //Stochastic Analysis -StochasticEmbeddedExample_7_Run - 2 - Vf = 2.83 % -HostElemets = 125
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //-----------------Run2a_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_7.Run2a_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2a_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_7.Run2a_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    //////-----------------Run2b_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2b_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    //////-----------------Run2b_Plastic-----------------//                
            //    //StochasticEmbeddedExample_7.Run2b_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    //////-----------------Run2c_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2c_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    //////-----------------Run2c_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2c_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    //////-----------------Run2d_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2d_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    //////-----------------Run2d_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run2d_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //}

            //***RUN-3_NEWTON RAPHSON***//
            // Stochastic Analysis -StochasticEmbeddedExample_7_Run-3 - Vf = 2.83 % - HostElemets = 1,000
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    ////-----------------Run3a_Elastic-----------------//
            //    //StochasticEmbeddedExample_7.Run3a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_7.Run3a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    ////-----------------Run3a_Plastic-----------------//
            //    //StochasticEmbeddedExample_7.Run3a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_7.Run3a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3b_Elastic-----------------//                
            //    StochasticEmbeddedExample_7.Run3b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3b_Plastic-----------------//                
            //    StochasticEmbeddedExample_7.Run3b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3c_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run3c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3c_Plastic-----------------//                
            //    StochasticEmbeddedExample_7.Run3c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run3d_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run3d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run3d_Plastic-----------------//
            //    StochasticEmbeddedExample_7.Run3d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            // Stochastic Analysis -StochasticEmbeddedExample_7_Run-4 - Vf = 2.83 % - HostElemets = 3,375
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_7.Run4a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4a_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_7.Run4a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4b_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4b_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4c_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4c_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run4d_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4d_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run4d_Elastic-----------------//
            //    StochasticEmbeddedExample_7.Run4d_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //// Stochastic Analysis -StochasticEmbeddedExample_7_Run-5 - Vf = 2.83 % - HostElemets = 8,000
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //-----------------ELASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_7.Run5a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_7.Run5a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------PLASTIC MATRIX-----------------//
            //    StochasticEmbeddedExample_7.Run5a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_7.Run5a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//


            //********************//
            // EmbeddedExample-8 //
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run2a_Elastic-----------------//
            //    StochasticEmbeddedExample_8.Run2a_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2a_Elastic-----------------//
            //    StochasticEmbeddedExample_8.Run2a_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_8.Run2b_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2b_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_8.Run2b_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2b_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run2c_Elastic-----------------//
            //    //StochasticEmbeddedExample_8.Run2c_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_8.Run2c_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_8.Run2c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //    //-----------------Run2c_Elastic-----------------//
            //    //StochasticEmbeddedExample_8.Run2c_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_8.Run2c_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_8.Run2c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}
            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //*******************//
            // EmbeddedExample-9 //
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;

            ////for (int i = 1; i <= 1; i++)
            ////{
            ////    //-----------------Run2a_Elastic-----------------//
            ////    //StochasticEmbeddedExample_9.Run2a_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            ////    //StochasticEmbeddedExample_9.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            ////    //StochasticEmbeddedExample_9.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);


            ////    //-----------------Run2a_Plastic-----------------//
            ////    //StochasticEmbeddedExample_9.Run2a_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            ////    //StochasticEmbeddedExample_9.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            ////    //StochasticEmbeddedExample_9.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            ////}

            //for (int i = 1; i <= 1; i++)
            //{
            ////    //-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_9.Run2b_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_9.Run2b_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    StochasticEmbeddedExample_9.Run2b_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            ////    //-----------------Run2b_Plastic-----------------//
            ////    StochasticEmbeddedExample_9.Run2b_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2b_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2b_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            //}

            ////for (int i = 1; i <= 1; i++)
            ////{
            ////    //-----------------Run2c_Elastic-----------------//
            ////    StochasticEmbeddedExample_9.Run2c_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2c_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2c_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            ////    //-----------------Run2c_Plastic-----------------//
            ////    StochasticEmbeddedExample_9.Run2c_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2c_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            ////    StochasticEmbeddedExample_9.Run2c_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);
            ////}
            ////**************************************************************************************************************************************************************************************//
            ////**************************************************************************************************************************************************************************************//

            ////********************//
            //// EmbeddedExample-10 //
            ////*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;

            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run2a_Elastic_NewtonRaphson-----------------//
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2a_Elastic_DisplacementControl-----------------//
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.SingleMatrix_DisplacementControl_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl_Stochastic(i);

            //    //-----------------Run2a_Plastic-----------------//
            //    //StochasticEmbeddedExample_10.Run2a_Plastic.SingleMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Plastic.EBEembeddedInMatrix_NewtonRaphson_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2a_Plastic.EBEembeddedInMatrixCohesive_NewtonRaphson_Stochastic(i);

            //    //-----------------Run2b_Elastic_DisplacementControl-----------------//
            //    //StochasticEmbeddedExample_10.Run2b_Elastic.SingleMatrix_DisplacementControl_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2b_Elastic.EBEembeddedInMatrix_DisplacementControl_Stochastic(i);
            //    //StochasticEmbeddedExample_10.Run2b_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl_Stochastic(i);

            //    //-----------------Run2a_Elastic_DisplacementControl-Jerry_Cohesive_Example-----------------//
            //    //StochasticEmbeddedExample_10.Jerry_Example_Elastic.Jerry_SingleMatrix(i);
            //    //StochasticEmbeddedExample_10.Jerry_Example_Elastic.Jerry_Embedded(i);
            //    StochasticEmbeddedExample_10.Jerry_Example_Elastic.Jerry_Cohesive(i);
            //}

            ////**************************************************************************************************************************************************************************************//
            ////**************************************************************************************************************************************************************************************//

            ////********************//
            //// EmbeddedExample-11 //
            ////*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            //***RUN-1_DISPLACEMENT CONTROL***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run1 - 2a_Elastic---------------- -//
            //    //StochasticEmbeddedExample_11.Run1_2a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    //-----------------Run1 - 2a_Plastic---------------- -//
            //    //StochasticEmbeddedExample_11.Run1_2a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-3a_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run1_3a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1-3a_Plastic-----------------//
            //    //StochasticEmbeddedExample_11.Run1_3a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-4a_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run1_4a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1-4a_Plastic-----------------//
            //    //StochasticEmbeddedExample_11.Run1_4a_Plastic.PEEKMatrix_DisplacementControl(i);

            //    ////-----------------Run1-5a_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run1_5a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    ////-----------------Run1 - 5a_Plastic---------------- -//
            //    //StochasticEmbeddedExample_11.Run1_5a_Plastic.PEEKMatrix_DisplacementControl(i);
            //}

            ////***RUN-2_DISPLACEMENT CONTROL***//
            ////Stochastic Analysis -StochasticEmbeddedExample_7_Run - 2 - Vf = 2.83 % -HostElemets = 125
            //StochasticEmbeddedExample_11.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_11.Run2a_Plastic.SingleMatrix_DisplacementControl();
            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //-----------------Run2a_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_11.Run2a_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    //-----------------Run2a_Plastic-----------------//
            //    //StochasticEmbeddedExample_11.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_11.Run2a_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2b_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run2b_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2b_Plastic-----------------//                
            //    //StochasticEmbeddedExample_11.Run2b_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2c_Elastic-----------------//
            //    //StochasticEmbeddedExample_11.Run2c_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2c_Plastic-----------------//
            //    //StochasticEmbeddedExample_11.Run2c_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2d_Elastic-----------------//
            //    StochasticEmbeddedExample_11.Run2d_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2d_Plastic-----------------//
            //    StochasticEmbeddedExample_11.Run2d_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            ////********************//
            //// EmbeddedExample-12 //
            ////*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            ////***RUN-1_DISPLACEMENT CONTROL***//
            //for (int i = 1; i <= 1; i++)
            //{
            //    //-----------------Run1 - 2a_Elastic---------------- -//
            //    StochasticEmbeddedExample_12.Run1_2a_Elastic.PEEKMatrix_DisplacementControl(i);
            //    //-----------------Run1 - 2a_Plastic---------------- -//
            //    StochasticEmbeddedExample_12.Run1_2a_Plastic.PEEKMatrix_DisplacementControl(i);
            //}

            ////***RUN-2_DISPLACEMENT CONTROL***//
            ////Stochastic Analysis -StochasticEmbeddedExample_7_Run - 2 - Vf = 2.83 % -HostElemets = 125
            //StochasticEmbeddedExample_11.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_11.Run2a_Plastic.SingleMatrix_DisplacementControl();

            ////-----------------Run2c_Elastic-----------------//
            //StochasticEmbeddedExample_12.Run2c_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(2);
            ////-----------------Run2c_Plastic-----------------//
            //StochasticEmbeddedExample_12.Run2c_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            ////-----------------Run2d_Elastic-----------------//
            //StochasticEmbeddedExample_12.Run2d_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(2);

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //-----------------Run2a_Elastic---------------- -//
            //    StochasticEmbeddedExample_12.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_12.Run2a_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    //-----------------Run2a_Plastic---------------- -//
            //    //StochasticEmbeddedExample_12.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_12.Run2a_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2b_Elastic-----------------//
            //    StochasticEmbeddedExample_12.Run2b_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2b_Plastic-----------------//                
            //    //StochasticEmbeddedExample_12.Run2b_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2c_Elastic-----------------//
            //    StochasticEmbeddedExample_12.Run2c_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2c_Plastic-----------------//
            //    //StochasticEmbeddedExample_12.Run2c_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);

            //    ////-----------------Run2d_Elastic-----------------//
            //    StochasticEmbeddedExample_12.Run2d_Elastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //    ////-----------------Run2d_Plastic-----------------//
            //    //StochasticEmbeddedExample_12.Run2d_Plastic.EBEembeddedInMatrixCohesive_DisplacementControl(i);
            //}


            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-13 // Vf = 1.5%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            //Run_2 - SingleMatrix
            // Vf = 1.5 % -HostElemets = 1000
            //StochasticEmbeddedExample_13.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_13.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    StochasticEmbeddedExample_13.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_13.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_13.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_13.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2b-Elastic
            //    StochasticEmbeddedExample_13.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic               
            //    //StochasticEmbeddedExample_13.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2c-Elastic
            //    StochasticEmbeddedExample_13.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic
            //    //StochasticEmbeddedExample_13.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2d-Elastic
            //    StochasticEmbeddedExample_13.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2d-Plastic
            //    //StochasticEmbeddedExample_13.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    //StochasticEmbeddedExample_13.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3a-Plastic
            //    //StochasticEmbeddedExample_13.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3b-Elastic
            //    //StochasticEmbeddedExample_13.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3b-Plastic               
            //    //StochasticEmbeddedExample_13.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3c-Elastic
            //    //StochasticEmbeddedExample_13.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3c-Plastic
            //    //StochasticEmbeddedExample_13.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3d-Elastic
            //    //StochasticEmbeddedExample_13.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3d-Plastic
            //    //StochasticEmbeddedExample_13.Run3d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_4 ** //

            //    // Run_4a-Elastic
            //    //StochasticEmbeddedExample_13.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4a-Plastic
            //    //StochasticEmbeddedExample_13.Run4a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_4b-Elastic
            //    //StochasticEmbeddedExample_13.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4b-Plastic               
            //    //StochasticEmbeddedExample_13.Run4b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_4c-Elastic
            //    //StochasticEmbeddedExample_13.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_4c-Plastic
            //    //StochasticEmbeddedExample_13.Run4c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-14 // Vf = 4.0%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 10;
            //int numberOfSimulations = 10;

            ////Run_2 - SingleMatrix
            //StochasticEmbeddedExample_14.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_14.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    //**Run_2 * * //

            //    //Run_2a - Elastic
            //    StochasticEmbeddedExample_14.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_14.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2a - Plastic
            //    //StochasticEmbeddedExample_14.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_14.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2b - Elastic
            //    StochasticEmbeddedExample_14.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2b - Plastic
            //    //StochasticEmbeddedExample_14.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2c - Elastic
            //    StochasticEmbeddedExample_14.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2b - Plastic
            //    //StochasticEmbeddedExample_14.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_2d - Elastic
            //    StochasticEmbeddedExample_14.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_2d - Plastic
            //    //StochasticEmbeddedExample_14.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    //**Run_3 * * //

            //    //Run_3a - Elastic
            //    //StochasticEmbeddedExample_14.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3a - Plastic
            //    //StochasticEmbeddedExample_14.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3b - Elastic
            //    //StochasticEmbeddedExample_14.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3b - Plastic
            //    //StochasticEmbeddedExample_14.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3c - Elastic
            //    //StochasticEmbeddedExample_14.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3b - Plastic
            //    //StochasticEmbeddedExample_14.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //Run_3d - Elastic
            //    //StochasticEmbeddedExample_14.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //Run_3d - Plastic
            //    //StochasticEmbeddedExample_14.Run3d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    //**Run_4 * * //

            //    ////Run_4a - Elastic
            //    //StochasticEmbeddedExample_14.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4a - Plastic
            //    ////StochasticEmbeddedExample_14.Run4a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    ////Run_4b - Elastic
            //    //StochasticEmbeddedExample_14.Run4b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4b - Plastic
            //    ////StochasticEmbeddedExample_14.Run4b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    ////Run_4c - Elastic
            //    //StochasticEmbeddedExample_14.Run4c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    ////Run_4b - Plastic
            //    ////StochasticEmbeddedExample_14.Run4c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-15 // Vf = 6.1525 %
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //// Run_2-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_15.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_15.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //// Run_3-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_15.Run3a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_15.Run3a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    StochasticEmbeddedExample_15.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_15.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_15.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_15.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2b-Elastic
            //    //StochasticEmbeddedExample_15.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic               
            //    //StochasticEmbeddedExample_15.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2c-Elastic
            //    //StochasticEmbeddedExample_15.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic
            //    //StochasticEmbeddedExample_15.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    StochasticEmbeddedExample_15.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_15.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3a-Plastic
            //    //StochasticEmbeddedExample_15.Run3a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_15.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3b-Elastic
            //    //StochasticEmbeddedExample_15.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic               
            //    //StochasticEmbeddedExample_15.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3c-Elastic
            //    //StochasticEmbeddedExample_15.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic
            //    //StochasticEmbeddedExample_15.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-16 // Vf = 1.5%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 10;

            ////Run_2 - SingleMatrix
            //// Vf = 1.5 % -HostElemets = 1000
            ////StochasticEmbeddedExample_16.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_16.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    //StochasticEmbeddedExample_16.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_16.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_16.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_16.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2b-Elastic
            //    //StochasticEmbeddedExample_16.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic               
            //    //StochasticEmbeddedExample_16.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2c-Elastic
            //    //StochasticEmbeddedExample_16.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2b-Plastic
            //    //StochasticEmbeddedExample_16.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_2d-Elastic
            //    //StochasticEmbeddedExample_16.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2d-Plastic
            //    //StochasticEmbeddedExample_16.Run2d_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    StochasticEmbeddedExample_16.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3a-Plastic
            //    //StochasticEmbeddedExample_16.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3b-Elastic
            //    StochasticEmbeddedExample_16.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_3b-Plastic               
            //    //StochasticEmbeddedExample_16.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // Run_3c-Elastic
            //    StochasticEmbeddedExample_16.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
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

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-18 // Vf=1.5% - 1 Wavy CNT
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //// Run_2-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_18.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_18.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //// Run_3-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_18.Run3a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_18.Run3a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    StochasticEmbeddedExample_18.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_18.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_18.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_18.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2b-Elastic
            //    //StochasticEmbeddedExample_18.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic               
            //    //StochasticEmbeddedExample_18.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2c-Elastic
            //    //StochasticEmbeddedExample_18.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic
            //    //StochasticEmbeddedExample_18.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    //StochasticEmbeddedExample_18.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_18.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3a-Plastic
            //    //StochasticEmbeddedExample_18.Run3a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_18.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3b-Elastic
            //    //StochasticEmbeddedExample_18.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic               
            //    //StochasticEmbeddedExample_18.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3c-Elastic
            //    //StochasticEmbeddedExample_18.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic
            //    //StochasticEmbeddedExample_18.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-19 // Vf=1.5% - 1 Straight CNT
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            // Run_2-SingleMatrix
            // Vf = 6.1525 %
            //StochasticEmbeddedExample_19.Run2a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_19.Run2a_Plastic.SingleMatrix_DisplacementControl();

            // Run_3-SingleMatrix
            // Vf = 6.1525 %
            //StochasticEmbeddedExample_19.Run3a_Elastic.SingleMatrix_DisplacementControl();
            //StochasticEmbeddedExample_19.Run3a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //

            //    // Run_2a-Elastic
            //    StochasticEmbeddedExample_19.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_19.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    // Run_2a-Plastic
            //    //StochasticEmbeddedExample_19.Run2a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_19.Run2a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2b-Elastic
            //    //StochasticEmbeddedExample_19.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic               
            //    //StochasticEmbeddedExample_19.Run2b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_2c-Elastic
            //    //StochasticEmbeddedExample_19.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_2b-Plastic
            //    //StochasticEmbeddedExample_19.Run2c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);


            //    // ** Run_3 ** //

            //    // Run_3a-Elastic
            //    //StochasticEmbeddedExample_19.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_19.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3a-Plastic
            //    //StochasticEmbeddedExample_19.Run3a_Plastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_19.Run3a_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3b-Elastic
            //    //StochasticEmbeddedExample_19.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic               
            //    //StochasticEmbeddedExample_19.Run3b_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    //// Run_3c-Elastic
            //    //StochasticEmbeddedExample_19.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //// Run_3b-Plastic
            //    //StochasticEmbeddedExample_19.Run3c_Plastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}            

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-20 // Convergence Study - Vf=1.5% - Wavy CNTs of Example_13
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 1;
            //int numberOfSimulations = 1;

            //// Run_2-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_20.Run2a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_20.Run2a_Plastic.SingleMatrix_DisplacementControl();

            //// Run_3-SingleMatrix
            //// Vf = 6.1525 %
            ////StochasticEmbeddedExample_20.Run3a_Elastic.SingleMatrix_DisplacementControl();
            ////StochasticEmbeddedExample_20.Run3a_Plastic.SingleMatrix_DisplacementControl();

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    // ** Run_2 ** //
            //    // Run_2a-Elastic
            //    //StochasticEmbeddedExample_20.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // ** Run_3 ** //
            //    // Run_3a-Elastic
            //    //StochasticEmbeddedExample_20.Run3a_Elastic.SingleMatrix_DisplacementControl();
            //    //StochasticEmbeddedExample_20.Run3a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run3a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_20.Run3b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run3c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run3d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);

            //    // ** Run_4 ** //
            //    // Run_4a-Elastic
            //    //StochasticEmbeddedExample_20.Run4a_Elastic.SingleMatrix_DisplacementControl();
            //    //StochasticEmbeddedExample_20.Run4a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    //StochasticEmbeddedExample_20.Run4a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);                
            //}

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//
            
            //********************//
            // EmbeddedExample-21 // Wavy CNTs- Vf=10%
            //*******************//
            LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            int startingNumofSimulations = 4;
            int numberOfSimulations = 10;

            for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            {
                StochasticEmbeddedExample_21.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
                StochasticEmbeddedExample_21.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
                StochasticEmbeddedExample_21.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
                StochasticEmbeddedExample_21.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
                StochasticEmbeddedExample_21.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            }

            //**************************************************************************************************************************************************************************************//
            //**************************************************************************************************************************************************************************************//

            //********************//
            // EmbeddedExample-22 // Straight CNTs- Vf=10%
            //*******************//
            //LinearAlgebra.LibrarySettings.LinearAlgebraProviders = LinearAlgebra.LinearAlgebraProviderChoice.MKL;
            //int startingNumofSimulations = 2;
            //int numberOfSimulations = 10;

            //for (int i = startingNumofSimulations; i <= numberOfSimulations; i++)
            //{
            //    StochasticEmbeddedExample_22.Run2a_Elastic.EBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_22.Run2a_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_22.Run2b_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_22.Run2c_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //    StochasticEmbeddedExample_22.Run2d_Elastic.CohesiveEBEembeddedInMatrix_DisplacementControl(i);
            //}

        }
    }
}
