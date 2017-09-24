using System;
namespace mlp.interviews.boxing.problem.domain
{
    public class BoxedPosition
    {
		public BoxedPosition(String trader,
						  String symbol,
						  Decimal qty)

		{
			this.trader = trader;
			this.symbol = symbol;
			this.qty = qty;
		}

		private String trader;
		private String symbol;
		private Decimal qty;

		public String Trader
		{
			get { return trader; }
		}

		public String Symbol
		{
			get { return symbol; }
		}

		public Decimal Qty
		{
			get { return qty; }
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			BoxedPosition other = (BoxedPosition)obj;

			return Trader.Equals(other.Trader) &&
						 Symbol.Equals(other.Symbol) &&
						 Qty.Equals(other.qty);
		}

		public override int GetHashCode()
		{
			return new { Trader, Symbol, Qty }.GetHashCode();
		}
	}
}
