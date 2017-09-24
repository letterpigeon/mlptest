using System;
using mlp.interviews.boxing.problem;
using mlp.interviews.boxing.problem.domain;
using System.Collections.Generic;
using System.IO;


namespace mlp.interviews.boxing.problem.client
{
    class PositionCalculatorClientApp
    {
        static int Main(string[] args)
        {
            if(args.Length<1)
            {
                Console.WriteLine("Usage: dotnet run <input_file_full_path>");
                return -1;
            }

            string inputfile = args[0];

            IEnumerable<Position> inputPositions = null;

            try
            {
                inputPositions = parseInputFile(inputfile);
            }
            catch(IOException ioe)
            {
                Console.WriteLine("IOException while processing input files {0}", inputfile);
                Console.WriteLine("StackTrace: {0} ", ioe.StackTrace);

                return -1;

            }
            catch(Exception e)
            {
                Console.WriteLine("The program has encountered an unexpected error");
                Console.WriteLine("StackTrace: {0} ", e.StackTrace);

                return -1;
            }


            PositionCalculator positionCalculator = new PositionCalculator();

            IEnumerable<NetPosition> netPositions = positionCalculator.calculateNetPositions(inputPositions);
            IEnumerable<BoxedPosition> boxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

            Console.WriteLine("Net Positions: \n");
            outputNetPositions(netPositions);

            Console.WriteLine("\n\nBox Positions: \n");
            outputBoxedPositions(boxedPositions);

            return 0;
        }

        private static IEnumerable<Position> parseInputFile(string filename)
        {
			List<Position> positions = new List<Position>();

			using (StreamReader streamreader = File.OpenText(filename))
			{
				string line = streamreader.ReadLine(); //discard header file

				while ((line = streamreader.ReadLine()) != null)
				{
					string[] fields = line.Split(",");
					Position position = new Position(fields[0],
													 fields[1],
													 fields[2],
													 Convert.ToDecimal(fields[3]),
													 Convert.ToDecimal(fields[4]));
					positions.Add(position);
				}
			}

            return positions;
        }

        private static void outputNetPositions(IEnumerable<NetPosition> netPositions)
        {
            Console.WriteLine("TRADER,SYMBOL,QUANTITY");
            foreach(NetPosition netPosition in netPositions)
            {
                Console.WriteLine("{0},{1},{2}", netPosition.Trader, netPosition.Symbol, netPosition.Qty);
            }
        }

        private static void outputBoxedPositions(IEnumerable<BoxedPosition> boxedPositions)
		{
			Console.WriteLine("TRADER,SYMBOL,QUANTITY");
            foreach (BoxedPosition boxedPosition in boxedPositions)
			{
				Console.WriteLine("{0},{1},{2}", boxedPosition.Trader, boxedPosition.Symbol, boxedPosition.Qty);
			}
		}
    }
}
