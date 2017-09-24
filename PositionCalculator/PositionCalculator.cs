using System;
using mlp.interviews.boxing.problem.domain;
using System.Collections.Generic;

namespace mlp.interviews.boxing.problem
{
    public class PositionCalculator
    {
        public IEnumerable<NetPosition> calculateNetPositions(IEnumerable<Position> positions)
        {
			//aggregate the positions by trader+symbol to calculate the net positions
            var posMapByTraderAndSymbol
                = new Dictionary<Tuple<string /*trader*/, string /*symbol*/>, decimal /*qty*/>();

            foreach(Position position in positions)
            {
                Decimal netQty;
                var traderSymbolTuple = new Tuple<String, String>(position.Trader, position.Symbol);
                if(posMapByTraderAndSymbol.TryGetValue(new Tuple<string, string>(position.Trader, position.Symbol), 
                                      out netQty))
                {
                    posMapByTraderAndSymbol[traderSymbolTuple] = netQty + position.Qty;
                }
                else
                {
                    posMapByTraderAndSymbol.Add(traderSymbolTuple, position.Qty);
                }
            }

            //flatten out the map to an array of positions to return.
            //ordering is undefined 
            var netPositions = new List<NetPosition>();


            foreach(KeyValuePair<Tuple<String, String>, Decimal> kvp in posMapByTraderAndSymbol)
            {
                var netPosition = new NetPosition(kvp.Key.Item1, //trader
                                kvp.Key.Item2, //symbol
                                kvp.Value //netQty
                                                 );
                netPositions.Add(netPosition);
            }

            return netPositions;
        }

		/*
         * Implementation logic/assumption/reasoning:
         * The test instruction indicates that an up-and-down at the
         * same broker is not considered a boxed position, e.g.:
         * 
         * Trader,Broker,Symbol,Qty
         * Mike,MS,IBM.N,-30
         * Mike,MS,IBM.N,70
         * 
         * This suggests that, at the broker level, these up-and-down will be
         * flattened out, and we're treating the above two positions as a long
         * position of 40 at MS
         * 
         * As a result, this implementation of the calculateBoxedPositions 
         * is making the assumption that the below positions are would not result 
         * in boxed position, even though Mike has has a short -30 IBM.N at MS
         * and a long 50 IBM.N at DB, because he is net along 40 IBM.N at MS
         * 
         * Trader,Broker,Symbol,Qty
         * Mike,MS,IBM.N,-30
         * Mike,MS,IBM.N,70
         * Mike,DB,IBM.N,50
         * 
         * On the other hand, the below positions
         * 
         * Mike,MS,IBM.N,-30
         * Mike,MS,IBM.N,70
         * Mike,DB,IBM.N,-30
         * 
         * would result in a boxed position of 30, instead of 60
         * Mike,IBM.N,30
         */
        public IEnumerable<BoxedPosition> calculateBoxedPositions(IEnumerable<Position> positions)
        {
			//First, aggregate positions at each broker
			/*
             * Mike,MS,IBM.N,-30
             * Mike,MS,IBM.N,70
             * Mike,DB,IBM.N,-30
             * 
             * would become:
             * Mike,MS,IBM.N,40
             * Mike,DB,IBM.N,-30
             */
			var posMapByTraderAndBroker = 
                new Dictionary<Tuple<String, /*Trader*/ 
                                     String, /*Broker*/ 
                                     String>,/*Symbol*/
                               Decimal>(); /*netQty*/
			foreach(Position position in positions)
            {
				Decimal netQty;
                var traderBrokerSymbolTuple = new Tuple<String, String, String>(position.Trader, position.Broker, position.Symbol);
                if (posMapByTraderAndBroker.TryGetValue(traderBrokerSymbolTuple,
									  out netQty))
				{
					posMapByTraderAndBroker[traderBrokerSymbolTuple] = netQty + position.Qty;
				}
				else
				{
					posMapByTraderAndBroker.Add(traderBrokerSymbolTuple, position.Qty);
				}
            }

            //now separately aggregate the longs and the shorts across all brokers
            //i.e. group by trader and symbol

            var longPositions = new Dictionary<Tuple<String /*Trader*/, String /*Symbol*/>, Decimal>();
            var shortPositions = new Dictionary<Tuple<String /*Trader*/, String /*Symbol*/>, Decimal>();

			foreach (KeyValuePair<Tuple<String, String, String>, Decimal> kvp in posMapByTraderAndBroker)
			{
                Decimal qty = kvp.Value;
                if (qty == 0) //net flat at this broker, skip
                    continue;

                Tuple<String, String, String> traderBrokerSymbolTuple = kvp.Key;

                //sum up long positions and short positions by trader+symbol

                var traderSymbolTuple = new Tuple<String, String>(traderBrokerSymbolTuple.Item1, traderBrokerSymbolTuple.Item3);
                if(qty > 0) //long position
                {
                    Decimal netLong;
                    if(longPositions.TryGetValue(traderSymbolTuple, out netLong))
                    {
                        longPositions[traderSymbolTuple] = netLong + qty;
                    }
                    else
                    {
                        longPositions.Add(traderSymbolTuple, qty);
                    }
                }
                else //short position
                {
                    Decimal netShort;

                    if (shortPositions.TryGetValue(traderSymbolTuple, out netShort))
					{
						shortPositions[traderSymbolTuple] = netShort + qty;
					}
					else
					{
						shortPositions.Add(traderSymbolTuple, qty);
					}
                }
			}

            var boxedPositions = new List<BoxedPosition>();

            //Now, we will have a boxed position only if any traderSymbolTuple exists
            //in both the longPositions and shortPositions dictionaries
            Dictionary<Tuple<String, String>, Decimal>.KeyCollection traderSymbolTuples = longPositions.Keys;
            foreach(Tuple<String, String> traderSymbolTuple in traderSymbolTuples)
            {
                Decimal netShort;
                if(shortPositions.TryGetValue(traderSymbolTuple, out netShort))
                {
                    Decimal netLong = longPositions[traderSymbolTuple];

                    //boxed position is the minimum of long and the absolute value of short
                    Decimal boxed = Math.Min(netLong, Math.Abs(netShort));

                    boxedPositions.Add(new BoxedPosition(traderSymbolTuple.Item1,
                                                         traderSymbolTuple.Item2,
                                                         boxed));
                }
            }

            return boxedPositions;
		}
    }
}
