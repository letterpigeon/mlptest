using System;

namespace mlp.interviews.boxing.problem.domain
{
    public class Position
    {
        public Position(String trader,
                       String broker,
                       String symbol,
                       Decimal qty,
                       Decimal price)
        {
            this.trader = trader;
            this.broker = broker;
            this.symbol = symbol;
            this.qty = qty;
            this.price = price;
        }

        private String trader;
        private String broker;
        private String symbol;
        private Decimal qty;
        private Decimal price;

        public String Trader
        {
            get { return trader; }
        }

        public String Broker
        {
            get { return broker; }
        }

        public String Symbol
        {
            get { return symbol; }
        }

        public Decimal Qty
        {
            get { return qty; }
        }

        public Decimal Price
        {
            get { return price; }
        }
    }
}   
  