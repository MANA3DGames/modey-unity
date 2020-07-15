
namespace MANA3DGames.Utilities.String
{
	public static class StringOperation
	{
		// ***************************************************************
		// Adds decimal mark to integer value and returns string.
		// ***************************************************************
		public static string AddCommaToNumber( int val )
		{
			// Decimal mark counter.
			int counter = 3;
			
			// Convert val to string.
			string valStr = val + "";
			
			// Check if we have more than 3 digits.
			if ( valStr.Length > 3 )
			{
				// Keep adding comma as long as we have enough digits.
				while ( valStr.Length > counter )
				{
					// Insert a comma after the next 3 digists.
					valStr = valStr.Insert( valStr.Length - counter, "," );
					
					// Increment the counter with 4 (0,000).
					counter += 4;
				}
			}
			
			// Return the final string with decimal mark(s).
			return valStr;
		}

		public static string AddCommaToNumber<T>( ref T val )
		{
			// Decimal mark counter.
			int counter = 3;

			// Convert val to string.
			string valStr = System.Convert.ChangeType( val, typeof(int) ) + "";

			// Check if we have more than 3 digits.
			if ( valStr.Length > 3 )
			{
				// Keep adding comma as long as we have enough digits.
				while ( valStr.Length > counter )
				{
					// Insert a comma after the next 3 digists.
					valStr = valStr.Insert( valStr.Length - counter, "," );

					// Increment the counter with 4 (0,000).
					counter += 4;
				}
			}

			// Return the final string with decimal mark(s).
			return valStr;
		}


        public static string AddDecimalPoint_One( int val )
		{
            if ( val < 10 )
                return "0." + val;

			// Convert val to string.
			string valStr = val.ToString();
		    
			valStr = valStr.Insert( valStr.Length - 1, "." );

			// Return the final string with decimal mark(s).
			return valStr;
		}
	}
}























