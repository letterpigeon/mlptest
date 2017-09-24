using System;
using Xunit;
using mlp.interviews.boxing.problem;
using mlp.interviews.boxing.problem.domain;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace mlp.interviews.boxing.problem.Tests
{
    public class PositionCalculatorTests
    {
        private readonly PositionCalculator positionCalculator;

        public PositionCalculatorTests()
        {
            positionCalculator = new PositionCalculator();
        }

        [Fact]
        public void TestNetPosition()
        {
            IEnumerable<Position> inputPositions = new List<Position> {
                new Position("Mike", "MS", "AAPL.N", 100, 20),
                new Position("Mike", "ML", "AAPL.N", 200, 20)
            };

            //output positions
            IEnumerable<NetPosition> expectedNetPositions = new List<NetPosition> 
                { new NetPosition("Mike", "AAPL.N", 300) };

            IEnumerable<NetPosition> actualNetPositions = positionCalculator.calculateNetPositions(inputPositions);

            Assert.Equal(expectedNetPositions, actualNetPositions);
        }

        [Fact]
        public void TestBoxedPosition_Boxed()
        {
            List<Position> inputPositions =  new List<Position> {
                new Position("Mike", "MS", "AAPL.N", 100, 20),
                new Position("Mike", "ML", "AAPL.N", -70, 20)
            };

            List<BoxedPosition> expectedBoxedPositions = new List<BoxedPosition>
            {
                new BoxedPosition("Mike", "AAPL.N", 70)
            };

            IEnumerable<BoxedPosition> actualBoxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

            Assert.Equal(expectedBoxedPositions, actualBoxedPositions);
        }

		/*
         * Mike,MS,IBM.N,-30
         * Mike,MS,IBM.N,70
         * Mike,DB,IBM.N,-20
         * 
         * expect boxed position of 20 
         */
		[Fact]
		public void TestBoxedPosition_BoxedWithUpAndDownAtSameBroker()
		{
			//input positions
            List<Position> inputPositions =  new List<Position> {
				new Position("Mike", "MS", "IBM.N", -30, 20),
				new Position("Mike", "MS", "IBM.N", 70, 20),
				new Position("Mike", "DB", "IBM.N", -20, 20)
			};

			List<BoxedPosition> expectedBoxedPositions = new List<BoxedPosition>
			{
				new BoxedPosition("Mike", "IBM.N", 20)
			};

			IEnumerable<BoxedPosition> actualBoxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

			Assert.Equal(expectedBoxedPositions, actualBoxedPositions);
		}

        //up & down at same broker -> not a boxed position
        [Fact]
        public void TestBoxedPosition_NoBoxed()
        {
            List<Position> inputPositions =  new List<Position> {
                new Position("Mike", "MS", "AAPL.N", 100, 20),
                new Position("Mike", "MS", "AAPL.N", -70, 20)
            };

            //expect no boxed position
            List<BoxedPosition> expectedBoxedPositions = new List<BoxedPosition>();

            IEnumerable<BoxedPosition> actualBoxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

            Assert.Equal(expectedBoxedPositions, actualBoxedPositions);
        }

		/*
         * Mike,MS,IBM.N,-30
         * Mike,MS,IBM.N,70
         * Mike,DB,IBM.N,50
         * 
         * not a boxed position since Mike is net long IBM at MS and also long at DB 
         */
		[Fact]
		public void TestBoxedPosition_NoBoxedWithUpAndDownAtSameBroker()
		{

			List<Position> inputPositions =  new List<Position> {
				new Position("Mike", "MS", "IBM.N", -30, 20),
				new Position("Mike", "MS", "IBM.N", 70, 20),
				new Position("Mike", "DB", "IBM.N", 50, 20)
			};

			List<BoxedPosition> expectedBoxedPositions = new List<BoxedPosition>();

			IEnumerable<BoxedPosition> actualBoxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

			Assert.Equal(expectedBoxedPositions, actualBoxedPositions);
		}

        /*
         * this test can be annotated to run multiple tests accepting different input files and expected output files
         */
		[Theory]
		[InlineData("testdata/test1/test_data.csv", "testdata/test1/net_positions_expected.csv", "testdata/test1/boxed_positions_expected.csv")]
		public void TestWithCSVInputAndExpectedOutputs(String inputFileName, 
                                                       String expectedNetPositionFileName, 
                                                       String expectedBoxedPositionFileName)
		{
            string currentDirectory = Path.GetDirectoryName(Environment.CurrentDirectory);

            string inputFileFullPath = Path.Combine(currentDirectory, @"../../"+inputFileName);
            string expectedNetPositionFileFullPath = Path.Combine(currentDirectory, @"../../" + expectedNetPositionFileName);
            string expectedBoxedPositionFileFullPath = Path.Combine(currentDirectory, @"../../" + expectedBoxedPositionFileName);

            IEnumerable<Position> inputPositions = parseInputPositions(inputFileFullPath);

            //expected outputs
            IEnumerable<NetPosition> expectedNetPositions = parseOuputNetPositions(expectedNetPositionFileFullPath);
            IEnumerable<BoxedPosition> expectedBoxedPositions = parseOuputBoxedPositions(expectedBoxedPositionFileFullPath);

            //actual outputs
            IEnumerable<NetPosition> actualNetPositions = positionCalculator.calculateNetPositions(inputPositions);
            IEnumerable<BoxedPosition> actualBoxedPositions = positionCalculator.calculateBoxedPositions(inputPositions);

            //assert they are matching
            Assert.Equal(expectedNetPositions, actualNetPositions);
            Assert.Equal(expectedBoxedPositions, actualBoxedPositions);

		}

        private IEnumerable<Position> parseInputPositions(String inputFileFullPath)
        {
            List<Position> positions = new List<Position>();

            using (StreamReader streamreader = File.OpenText(inputFileFullPath))
            {
                string line = streamreader.ReadLine(); //discard header file

                while((line = streamreader.ReadLine())!=null)
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

        private IEnumerable<NetPosition> parseOuputNetPositions(String netPositionFileFullPath)
		{
            List<NetPosition> netPositions = new List<NetPosition>();

			using (StreamReader streamreader = File.OpenText(netPositionFileFullPath))
			{
				string line = streamreader.ReadLine(); //discard header file

				while ((line = streamreader.ReadLine()) != null)
				{
					string[] fields = line.Split(",");
                    NetPosition netPosition = new NetPosition(fields[0],
                                                     fields[1],
                                                     Convert.ToDecimal(fields[2]));
													 
					netPositions.Add(netPosition);
				}
			}

			return netPositions;
		}

		private IEnumerable<BoxedPosition> parseOuputBoxedPositions(String boxedPositionFileFullPath)
		{
            List<BoxedPosition> boxedPositions = new List<BoxedPosition>();

			using (StreamReader streamreader = File.OpenText(boxedPositionFileFullPath))
			{
				string line = streamreader.ReadLine(); //discard header file

				while ((line = streamreader.ReadLine()) != null)
				{
					string[] fields = line.Split(",");
					BoxedPosition netPosition = new BoxedPosition(fields[0],
													 fields[1],
													 Convert.ToDecimal(fields[2]));

					boxedPositions.Add(netPosition);
				}
			}

			return boxedPositions;
		}

    }
}
